using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Domain;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;

namespace BusinessManagement.Application.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<BookingService> _logger;

    public BookingService(IUnitOfWork unitOfWork, ICurrentUserProvider currentUserProvider, ILogger<BookingService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    #region Passenger Operations
    public async Task<IEnumerable<PassengerDto>> GetAllPassengersAsync()
    {
        _logger.LogInformation("Fetching all passengers list.");
        var passengers = await _unitOfWork.GetRepository<Passenger>().GetAllAsync();
        return passengers.Select(p => new PassengerDto
        {
            Id = p.Id,
            FullName = p.FullName,
            PassportNumber = p.PassportNumber,
            Nationality = p.Nationality,
            DateOfBirth = p.DateOfBirth,
            PhoneNumber = p.PhoneNumber,
            PartnerId = p.PartnerId,
            GroupNumber = p.GroupNumber,
            CreatedAt = p.CreatedAt
        }).OrderByDescending(p => p.CreatedAt);
    }

    public async Task<PassengerDto?> GetPassengerByIdAsync(Guid id)
    {
        var p = await _unitOfWork.GetRepository<Passenger>().GetByIdAsync(id);
        if (p == null) return null;
        return new PassengerDto
        {
            Id = p.Id,
            FullName = p.FullName,
            PassportNumber = p.PassportNumber,
            Nationality = p.Nationality,
            DateOfBirth = p.DateOfBirth,
            PhoneNumber = p.PhoneNumber,
            CreatedAt = p.CreatedAt
        };
    }

    public async Task<PassengerDto> CreatePassengerAsync(CreatePassengerDto model)
    {
        _logger.LogInformation("Creating passenger: {FullName}, Passport: {Passport}", model.FullName, model.PassportNumber);
        
        // Check for duplicate passport number using searchable HMAC index (Point 2/v2 & Point 1/v3)
        var hashedPassport = BusinessManagement.Shared.Security.EncryptionHelper.HashSearchableField(model.PassportNumber);
        var existing = await _unitOfWork.GetRepository<Passenger>().FindAsync(p => p.PassportNumberHash == hashedPassport);
        if (existing.Any())
        {
            throw new InvalidOperationException($"A passenger with passport number '{model.PassportNumber}' already exists.");
        }

        var tenantId = _currentUserProvider.GetCurrentTenantId();
        if (tenantId == Guid.Empty)
        {
            var defaultTenant = (await _unitOfWork.GetRepository<Tenant>().GetAllAsync()).FirstOrDefault();
            tenantId = defaultTenant?.Id ?? Guid.Empty;
        }

        var passenger = new Passenger
        {
            TenantId = tenantId,
            FullName = model.FullName.Trim(),
            PassportNumber = model.PassportNumber.Trim().ToUpper(),
            PassportExpiry = model.PassportExpiry ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(10)),
            NationalId = model.NationalId?.Trim() ?? string.Empty,
            DateOfBirth = model.DateOfBirth,
            Nationality = model.Nationality?.Trim() ?? "Egyptian",
            Gender = model.Gender?.Trim() ?? "Male",
            PhoneNumber = model.PhoneNumber?.Trim() ?? string.Empty,
            Email = model.Email?.Trim() ?? string.Empty,
            Address = model.Address?.Trim() ?? string.Empty,
            EmergencyContactName = model.EmergencyContactName?.Trim() ?? string.Empty,
            EmergencyContactPhone = model.EmergencyContactPhone?.Trim() ?? string.Empty,
            PartnerId = model.PartnerId
        };

        await _unitOfWork.GetRepository<Passenger>().AddAsync(passenger);
        await _unitOfWork.CompleteAsync();

        return new PassengerDto
        {
            Id = passenger.Id,
            FullName = passenger.FullName,
            PassportNumber = passenger.PassportNumber,
            Nationality = passenger.Nationality,
            DateOfBirth = passenger.DateOfBirth,
            PhoneNumber = passenger.PhoneNumber,
            PartnerId = passenger.PartnerId,
            CreatedAt = passenger.CreatedAt
        };
    }

    public async Task DeletePassengerAsync(Guid id)
    {
        var passenger = await _unitOfWork.GetRepository<Passenger>().GetByIdAsync(id);
        if (passenger == null)
        {
            throw new InvalidOperationException("Passenger record not found.");
        }

        var bookings = await _unitOfWork.GetRepository<Booking>().FindAsync(b => b.PassengerId == id);
        if (bookings.Any())
        {
            throw new InvalidOperationException("Cannot delete this passenger because they have active bookings associated with them.");
        }

        _unitOfWork.GetRepository<Passenger>().Delete(passenger);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<PassengerImportResultDto> ImportPassengersAsync(IEnumerable<CreatePassengerDto> passengers)
    {
        var passengerModels = passengers?.ToList() ?? new List<CreatePassengerDto>();
        _logger.LogInformation("Importing {RowCount} passenger rows from Excel.", passengerModels.Count);

        var result = new PassengerImportResultDto
        {
            TotalRows = passengerModels.Count
        };

        if (!passengerModels.Any())
        {
            return result;
        }

        var normalizedPassports = passengerModels
            .Where(p => !string.IsNullOrWhiteSpace(p.PassportNumber))
            .Select(p => p.PassportNumber.Trim().ToUpper())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var hashedPassports = normalizedPassports.Select(BusinessManagement.Shared.Security.EncryptionHelper.HashSearchableField).ToList();

        var existingPassengers = hashedPassports.Any()
            ? await _unitOfWork.GetRepository<Passenger>().FindAsync(p => hashedPassports.Contains(p.PassportNumberHash))
            : Enumerable.Empty<Passenger>();

        var tenantId = _currentUserProvider.GetCurrentTenantId();
        if (tenantId == Guid.Empty)
        {
            var defaultTenant = (await _unitOfWork.GetRepository<Tenant>().GetAllAsync()).FirstOrDefault();
            tenantId = defaultTenant?.Id ?? Guid.Empty;
        }

        var existingPassportSet = new HashSet<string>(existingPassengers.Select(p => p.PassportNumber), StringComparer.OrdinalIgnoreCase);
        var importPassportSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var validPassengers = new List<Passenger>();

        foreach (var model in passengerModels)
        {
            var fullName = model.FullName?.Trim() ?? string.Empty;
            var passportNumber = model.PassportNumber?.Trim().ToUpper() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(passportNumber))
            {
                result.ImportErrors.Add(new PassengerImportErrorDto
                {
                    Message = "Full name and passport number are required."
                });
                continue;
            }

            if (existingPassportSet.Contains(passportNumber) || !importPassportSet.Add(passportNumber))
            {
                result.DuplicateCount++;
                continue;
            }

            validPassengers.Add(new Passenger
            {
                TenantId = tenantId,
                FullName = fullName,
                PassportNumber = passportNumber,
                PassportExpiry = model.PassportExpiry ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(10)),
                NationalId = model.NationalId?.Trim() ?? string.Empty,
                DateOfBirth = model.DateOfBirth,
                Nationality = model.Nationality?.Trim() ?? "Egyptian",
                Gender = model.Gender?.Trim() ?? "Male",
                PhoneNumber = model.PhoneNumber?.Trim() ?? string.Empty,
                Email = model.Email?.Trim() ?? string.Empty,
                Address = model.Address?.Trim() ?? string.Empty,
                EmergencyContactName = model.EmergencyContactName?.Trim() ?? string.Empty,
                EmergencyContactPhone = model.EmergencyContactPhone?.Trim() ?? string.Empty,
                PartnerId = model.PartnerId
            });
        }

        if (validPassengers.Any())
        {
            foreach (var passenger in validPassengers)
            {
                await _unitOfWork.GetRepository<Passenger>().AddAsync(passenger);
            }

            await _unitOfWork.CompleteAsync();
            result.ImportedCount = validPassengers.Count;
        }

        return result;
    }
    #endregion

    #region Booking Operations
    public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync(string? partnerUsername = null)
    {
        _logger.LogInformation("Fetching bookings list. Partner filter: {Partner}", partnerUsername ?? "All");
        
        // Eager load Passenger and Partner
        var bookings = await _unitOfWork.GetRepository<Booking>().GetAllAsync();
        var passengers = await _unitOfWork.GetRepository<Passenger>().GetAllAsync();
        var partners = await _unitOfWork.GetRepository<Partner>().GetAllAsync();

        var query = from b in bookings
                    join p in passengers on b.PassengerId equals p.Id
                    join pt in partners on b.PartnerId equals pt.Id
                    select new { b, p, pt };

        // Apply partner-specific data isolation if user is a Partner
        if (!string.IsNullOrWhiteSpace(partnerUsername))
        {
            query = query.Where(x => x.pt.Username.Equals(partnerUsername, StringComparison.OrdinalIgnoreCase));
        }

        return query.Select(x => new BookingDto
        {
            Id = x.b.Id,
            BookingNumber = x.b.BookingNumber,
            PassengerId = x.b.PassengerId,
            PassengerName = x.p.FullName,
            PassengerPassport = x.p.PassportNumber,
            PartnerId = x.b.PartnerId,
            PartnerName = x.pt.Name,
            TravelDate = x.b.TravelDate,
            NetCost = x.b.NetCost,
            BarcodeCost = x.b.BarcodeCost,
            CompanyCost = x.b.CompanyCost,
            AirportCost = x.b.AirportCost,
            ProgramCost = x.b.ProgramCost,
            TicketCost = x.b.TicketCost,
            BusCost = x.b.BusCost,
            TotalCost = x.b.TotalCost,
            SellingPrice = x.b.SellingPrice,
            NetProfit = x.b.NetProfit,
            PaymentsCollected = x.b.PaymentsCollected,
            RemainingBalance = x.b.RemainingBalance,
            HasQrCode = x.b.HasQrCode,
            Notes = x.b.Notes,
            CreatedAt = x.b.CreatedAt,
            GroupNumber = x.b.GroupNumber
        }).OrderByDescending(x => x.CreatedAt);
    }

    public async Task<BookingDto?> GetBookingByIdAsync(Guid id)
    {
        var b = await _unitOfWork.GetRepository<Booking>().GetByIdAsync(id);
        if (b == null) return null;

        var passenger = await _unitOfWork.GetRepository<Passenger>().GetByIdAsync(b.PassengerId);
        var partner = await _unitOfWork.GetRepository<Partner>().GetByIdAsync(b.PartnerId);

        return new BookingDto
        {
            Id = b.Id,
            BookingNumber = b.BookingNumber,
            PassengerId = b.PassengerId,
            PassengerName = passenger?.FullName ?? "-",
            PassengerPassport = passenger?.PassportNumber ?? "-",
            PartnerId = b.PartnerId,
            PartnerName = partner?.Name ?? "-",
            TravelDate = b.TravelDate,
            NetCost = b.NetCost,
            BarcodeCost = b.BarcodeCost,
            CompanyCost = b.CompanyCost,
            AirportCost = b.AirportCost,
            ProgramCost = b.ProgramCost,
            TicketCost = b.TicketCost,
            BusCost = b.BusCost,
            TotalCost = b.TotalCost,
            SellingPrice = b.SellingPrice,
            NetProfit = b.NetProfit,
            PaymentsCollected = b.PaymentsCollected,
            RemainingBalance = b.RemainingBalance,
            HasQrCode = b.HasQrCode,
            Notes = b.Notes,
            CreatedAt = b.CreatedAt,
            GroupNumber = b.GroupNumber
        };
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingDto model)
    {
        _logger.LogInformation("Creating booking file for Partner: {PartnerId}, Passenger: {PassengerId}", model.PartnerId, model.PassengerId);

        var partner = await _unitOfWork.GetRepository<Partner>().GetByIdAsync(model.PartnerId);
        if (partner == null)
        {
            throw new InvalidOperationException("Attributed business partner not found.");
        }

        if (!model.PassengerId.HasValue || model.PassengerId.Value == Guid.Empty)
        {
            throw new InvalidOperationException("Passenger profile is required.");
        }

        var passenger = await _unitOfWork.GetRepository<Passenger>().GetByIdAsync(model.PassengerId.Value);
        if (passenger == null)
        {
            throw new InvalidOperationException("Passenger record not found.");
        }

        // Quota safety validation lock (الكوتة check bypassed)
        /* 
        if (model.HasQrCode)
        {
            var existingBookings = await _unitOfWork.GetRepository<Booking>().FindAsync(b => b.PartnerId == model.PartnerId && b.HasQrCode);
            int qrCount = existingBookings.Count();
            if (qrCount >= partner.QuotaLimit)
            {
                throw new InvalidOperationException($"Booking failed. Quota limit of {partner.QuotaLimit} slots exceeded for partner '{partner.Name}'.");
            }
        }
        */

        // Sum calculations
        decimal netCost = model.NetCost ?? 0;
        decimal barcodeCost = model.BarcodeCost ?? 0;
        decimal companyCost = model.CompanyCost ?? 0;
        decimal airportCost = model.AirportCost ?? 0;
        decimal programCost = model.ProgramCost ?? 0;
        decimal ticketCost = model.TicketCost ?? 0;
        decimal busCost = model.BusCost ?? 0;
        decimal sellingPrice = model.SellingPrice ?? 0;
        decimal paymentsCollected = model.PaymentsCollected ?? 0;

        decimal totalCost = netCost + barcodeCost + companyCost + airportCost + programCost + ticketCost + busCost;
        decimal netProfit = sellingPrice - totalCost;
        decimal remaining = sellingPrice - paymentsCollected;

        // Auto-generation of unique Booking Number
        var allBookings = await _unitOfWork.GetRepository<Booking>().FindAsync(b => b.BookingNumber.StartsWith("B-"), ignoreQueryFilters: true);
        var numericBookings = allBookings
            .Select(b => b.BookingNumber)
            .Where(n => n.Length == 8 && int.TryParse(n.Substring(2), out _))
            .Select(n => int.Parse(n.Substring(2)))
            .ToList();

        int maxNumber = numericBookings.Any() ? numericBookings.Max() : 1000;
        string bookingNumber = $"B-{(maxNumber + 1):D6}";

        // Resolve Tenant and Branch IDs
        var tenantId = _currentUserProvider.GetCurrentTenantId();
        if (tenantId == Guid.Empty)
        {
            var defaultTenant = (await _unitOfWork.GetRepository<Tenant>().GetAllAsync()).FirstOrDefault();
            tenantId = defaultTenant?.Id ?? Guid.Empty;
        }

        var defaultBranch = (await _unitOfWork.GetRepository<Branch>().GetAllAsync()).FirstOrDefault();
        var branchId = defaultBranch?.Id ?? Guid.Empty;

        var booking = new Booking
        {
            TenantId = tenantId,
            BranchId = branchId,
            BookingNumber = bookingNumber,
            PassengerId = model.PassengerId.Value,
            PartnerId = model.PartnerId,
            PackageId = new Guid("99999999-9999-9999-9999-999999999999"),
            TravelDate = model.TravelDate,
            NetCost = netCost,
            BarcodeCost = barcodeCost,
            CompanyCost = companyCost,
            AirportCost = airportCost,
            ProgramCost = programCost,
            TicketCost = ticketCost,
            BusCost = busCost,
            TotalCost = totalCost,
            SellingPrice = sellingPrice,
            NetProfit = netProfit,
            PaymentsCollected = paymentsCollected,
            RemainingBalance = remaining,
            HasQrCode = model.HasQrCode,
            Notes = model.Notes ?? string.Empty,
            GroupNumber = model.GroupNumber
        };

        await _unitOfWork.GetRepository<Booking>().AddAsync(booking);
        await _unitOfWork.CompleteAsync();

        if (paymentsCollected > 0)
        {
            var safeTxId = Guid.NewGuid();
            var payment = new BookingPayment
            {
                BookingId = booking.Id,
                Amount = paymentsCollected,
                Currency = "EGP",
                PaymentMethod = model.PaymentMethod ?? "Cash",
                ReceiptNumber = $"{(model.ReceiptNumber ?? string.Empty)}#ST-{safeTxId}",
                PaymentDate = model.PaymentDate ?? DateOnly.FromDateTime(DateTime.UtcNow)
            };
            await _unitOfWork.GetRepository<BookingPayment>().AddAsync(payment);

            var safeTx = new SafeTransaction
            {
                Id = safeTxId,
                TenantId = tenantId,
                TransactionDate = model.PaymentDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                Description = $"دفعة حجز أولية ملف {booking.BookingNumber} للمسافر {passenger.FullName} {(string.IsNullOrEmpty(model.ReceiptNumber) ? "" : $"[{model.ReceiptNumber}]")}",
                Amount = paymentsCollected,
                Currency = BusinessManagement.Domain.Enums.Currency.EGP,
                TransactionType = BusinessManagement.Domain.Enums.TransactionType.Deposit,
                AssociatedPartnerId = booking.PartnerId
            };
            await _unitOfWork.GetRepository<SafeTransaction>().AddAsync(safeTx);
            await _unitOfWork.CompleteAsync();
        }

        return new BookingDto
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            PassengerId = booking.PassengerId,
            PassengerName = passenger.FullName,
            PassengerPassport = passenger.PassportNumber,
            PartnerId = booking.PartnerId,
            PartnerName = partner.Name,
            TravelDate = booking.TravelDate,
            NetCost = booking.NetCost,
            BarcodeCost = booking.BarcodeCost,
            CompanyCost = booking.CompanyCost,
            AirportCost = booking.AirportCost,
            ProgramCost = booking.ProgramCost,
            TicketCost = booking.TicketCost,
            BusCost = booking.BusCost,
            TotalCost = booking.TotalCost,
            SellingPrice = booking.SellingPrice,
            NetProfit = booking.NetProfit,
            PaymentsCollected = booking.PaymentsCollected,
            RemainingBalance = booking.RemainingBalance,
            HasQrCode = booking.HasQrCode,
            Notes = booking.Notes,
            CreatedAt = booking.CreatedAt,
            GroupNumber = booking.GroupNumber
        };
    }

    public async Task<BookingDto> UpdateBookingAsync(Guid id, CreateBookingDto model)
    {
        _logger.LogInformation("Updating booking file ID: {BookingId}", id);

        var booking = await _unitOfWork.GetRepository<Booking>().GetByIdAsync(id);
        if (booking == null)
        {
            throw new KeyNotFoundException("Booking record not found.");
        }

        var partner = await _unitOfWork.GetRepository<Partner>().GetByIdAsync(model.PartnerId);
        if (partner == null)
        {
            throw new InvalidOperationException("Attributed business partner not found.");
        }

        if (!model.PassengerId.HasValue || model.PassengerId.Value == Guid.Empty)
        {
            throw new InvalidOperationException("Passenger profile is required.");
        }

        var passenger = await _unitOfWork.GetRepository<Passenger>().GetByIdAsync(model.PassengerId.Value);
        if (passenger == null)
        {
            throw new InvalidOperationException("Passenger record not found.");
        }

        // Quota safety checks on update (bypassed)
        /*
        if (model.HasQrCode && (!booking.HasQrCode || booking.PartnerId != model.PartnerId))
        {
            var existingBookings = await _unitOfWork.GetRepository<Booking>().FindAsync(b => b.PartnerId == model.PartnerId && b.HasQrCode);
            int qrCount = existingBookings.Count();
            if (qrCount >= partner.QuotaLimit)
            {
                throw new InvalidOperationException($"Booking update failed. Quota limit of {partner.QuotaLimit} slots exceeded for partner '{partner.Name}'.");
            }
        }
        */

        // Calculations
        decimal netCost = model.NetCost ?? 0;
        decimal barcodeCost = model.BarcodeCost ?? 0;
        decimal companyCost = model.CompanyCost ?? 0;
        decimal airportCost = model.AirportCost ?? 0;
        decimal programCost = model.ProgramCost ?? 0;
        decimal ticketCost = model.TicketCost ?? 0;
        decimal busCost = model.BusCost ?? 0;
        decimal sellingPrice = model.SellingPrice ?? 0;
        decimal paymentsCollected = model.PaymentsCollected ?? 0;

        decimal totalCost = netCost + barcodeCost + companyCost + airportCost + programCost + ticketCost + busCost;
        decimal netProfit = sellingPrice - totalCost;
        decimal remaining = sellingPrice - booking.PaymentsCollected;

        booking.PassengerId = model.PassengerId.Value;
        booking.PartnerId = model.PartnerId;
        booking.TravelDate = model.TravelDate;
        booking.NetCost = netCost;
        booking.BarcodeCost = barcodeCost;
        booking.CompanyCost = companyCost;
        booking.AirportCost = airportCost;
        booking.ProgramCost = programCost;
        booking.TicketCost = ticketCost;
        booking.BusCost = busCost;
        booking.TotalCost = totalCost;
        booking.SellingPrice = sellingPrice;
        booking.NetProfit = netProfit;
        booking.RemainingBalance = remaining;
        booking.HasQrCode = model.HasQrCode;
        booking.Notes = model.Notes ?? string.Empty;
        booking.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.GetRepository<Booking>().Update(booking);
        await _unitOfWork.CompleteAsync();

        return new BookingDto
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            PassengerId = booking.PassengerId,
            PassengerName = passenger.FullName,
            PassengerPassport = passenger.PassportNumber,
            PartnerId = booking.PartnerId,
            PartnerName = partner.Name,
            TravelDate = booking.TravelDate,
            NetCost = booking.NetCost,
            BarcodeCost = booking.BarcodeCost,
            CompanyCost = booking.CompanyCost,
            AirportCost = booking.AirportCost,
            ProgramCost = booking.ProgramCost,
            TicketCost = booking.TicketCost,
            BusCost = booking.BusCost,
            TotalCost = booking.TotalCost,
            SellingPrice = booking.SellingPrice,
            NetProfit = booking.NetProfit,
            PaymentsCollected = booking.PaymentsCollected,
            RemainingBalance = booking.RemainingBalance,
            HasQrCode = booking.HasQrCode,
            Notes = booking.Notes,
            CreatedAt = booking.CreatedAt,
            GroupNumber = booking.GroupNumber
        };
    }

    public async Task DeleteBookingAsync(Guid id)
    {
        _logger.LogWarning("Deleting booking file ID: {BookingId}", id);
        var booking = await _unitOfWork.GetRepository<Booking>().GetByIdAsync(id);
        if (booking == null)
        {
            throw new KeyNotFoundException("Booking record not found.");
        }

        _unitOfWork.GetRepository<Booking>().Delete(booking);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<IEnumerable<PartnerQuotaMetric>> GetPartnersLookupAsync()
    {
        var partners = await _unitOfWork.GetRepository<Partner>().GetAllAsync();
        var bookings = await _unitOfWork.GetRepository<Booking>().GetAllAsync();

        return partners.Select(p =>
        {
            var pBookings = bookings.Where(b => b.PartnerId == p.Id).ToList();
            return new PartnerQuotaMetric
            {
                PartnerId = p.Id,
                PartnerName = p.Name,
                BookingsCount = pBookings.Count,
                QrCount = pBookings.Count(b => b.HasQrCode),
                NonQrCount = pBookings.Count(b => !b.HasQrCode),
                QuotaLimit = p.QuotaLimit,
                TotalProfit = pBookings.Sum(b => b.NetProfit),
                CollectedPayments = pBookings.Sum(b => b.PaymentsCollected),
                RemainingBalance = pBookings.Sum(b => b.RemainingBalance)
            };
        });
    }

    public async Task<IEnumerable<BookingPaymentDto>> GetBookingPaymentsAsync(Guid bookingId)
    {
        var payments = await _unitOfWork.GetRepository<BookingPayment>().FindAsync(p => p.BookingId == bookingId);
        if (!payments.Any()) return Enumerable.Empty<BookingPaymentDto>();

        // Find distinct months/years of the payments
        var months = payments.Select(p => new { p.PaymentDate.Year, p.PaymentDate.Month }).Distinct().ToList();

        var monthPaymentsMap = new Dictionary<(int Year, int Month), List<BookingPayment>>();
        foreach (var m in months)
        {
            var startOfMonth = new DateOnly(m.Year, m.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var allInMonth = await _unitOfWork.GetRepository<BookingPayment>()
                .FindAsync(p => p.PaymentDate >= startOfMonth && p.PaymentDate <= endOfMonth);

            monthPaymentsMap[(m.Year, m.Month)] = allInMonth
                .OrderBy(p => p.CreatedAt)
                .ThenBy(p => p.Id)
                .ToList();
        }

        return payments.Select(x =>
        {
            var key = (x.PaymentDate.Year, x.PaymentDate.Month);
            int seqIndex = 1;
            if (monthPaymentsMap.TryGetValue(key, out var list))
            {
                seqIndex = list.FindIndex(p => p.Id == x.Id) + 1;
                if (seqIndex <= 0) seqIndex = 1;
            }

            string txId = $"TX-{x.PaymentDate:yyyyMM}-{seqIndex:D4}";

            string rawReceipt = x.ReceiptNumber ?? "";
            string cleanReceipt = rawReceipt;
            if (rawReceipt.Contains("#ST-"))
            {
                cleanReceipt = rawReceipt.Split("#ST-")[0];
            }
            else if (rawReceipt.StartsWith("ST-"))
            {
                cleanReceipt = "";
            }

            return new BookingPaymentDto
            {
                Id = x.Id,
                BookingId = x.BookingId,
                Amount = x.Amount,
                Currency = x.Currency,
                PaymentMethod = x.PaymentMethod,
                ReceiptNumber = cleanReceipt,
                TransactionId = txId,
                PaymentDate = x.PaymentDate
            };
        }).ToList();
    }

    public async Task AddBookingPaymentAsync(Guid bookingId, decimal amount, string paymentMethod, string receiptNumber, DateOnly paymentDate)
    {
        var booking = await _unitOfWork.GetRepository<Booking>().GetByIdAsync(bookingId);
        if (booking == null)
        {
            throw new KeyNotFoundException("Booking record not found.");
        }

        if (amount > booking.RemainingBalance)
        {
            throw new InvalidOperationException($"المبلغ المدخل ({amount:N2} ج.م) أكبر من الرصيد المتبقي للملف ({booking.RemainingBalance:N2} ج.م).");
        }

        var safeTxId = Guid.NewGuid();
        var payment = new BookingPayment
        {
            BookingId = bookingId,
            Amount = amount,
            Currency = "EGP",
            PaymentMethod = paymentMethod,
            ReceiptNumber = $"{(receiptNumber ?? string.Empty)}#ST-{safeTxId}",
            PaymentDate = paymentDate
        };

        await _unitOfWork.GetRepository<BookingPayment>().AddAsync(payment);

        // Update booking denormalized fields
        var allPayments = await _unitOfWork.GetRepository<BookingPayment>().FindAsync(p => p.BookingId == bookingId);
        decimal totalPaid = allPayments.Sum(p => p.Amount) + amount; // Include the new one
        booking.PaymentsCollected = totalPaid;
        booking.RemainingBalance = booking.SellingPrice - totalPaid;

        _unitOfWork.GetRepository<Booking>().Update(booking);

        // Also add deposit to safe!
        var passenger = await _unitOfWork.GetRepository<Passenger>().GetByIdAsync(booking.PassengerId);
        var safeTx = new SafeTransaction
        {
            Id = safeTxId,
            TenantId = booking.TenantId,
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Description = $"دفعة حجز ملف {booking.BookingNumber} للمسافر {passenger?.FullName ?? "غير معروف"} {(string.IsNullOrEmpty(receiptNumber) ? "" : $"[{receiptNumber}]")}",
            Amount = amount,
            Currency = BusinessManagement.Domain.Enums.Currency.EGP,
            TransactionType = BusinessManagement.Domain.Enums.TransactionType.Deposit,
            AssociatedPartnerId = booking.PartnerId
        };
        await _unitOfWork.GetRepository<SafeTransaction>().AddAsync(safeTx);

        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteBookingPaymentAsync(Guid paymentId)
    {
        var payment = await _unitOfWork.GetRepository<BookingPayment>().GetByIdAsync(paymentId);
        if (payment == null)
        {
            throw new KeyNotFoundException("Payment record not found.");
        }

        var bookingId = payment.BookingId;
        var booking = await _unitOfWork.GetRepository<Booking>().GetByIdAsync(bookingId);

        _unitOfWork.GetRepository<BookingPayment>().Delete(payment);

        // Update booking denormalized fields
        if (booking != null)
        {
            var allPayments = await _unitOfWork.GetRepository<BookingPayment>().FindAsync(p => p.BookingId == bookingId);
            decimal totalPaid = allPayments.Where(p => p.Id != paymentId).Sum(p => p.Amount);
            booking.PaymentsCollected = totalPaid;
            booking.RemainingBalance = booking.SellingPrice - totalPaid;
            _unitOfWork.GetRepository<Booking>().Update(booking);

            // Also create a withdrawal to correct the safe balance (offset the payment delete)!
            var passenger = await _unitOfWork.GetRepository<Passenger>().GetByIdAsync(booking.PassengerId);
            var safeTx = new SafeTransaction
            {
                TenantId = booking.TenantId,
                TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Description = $"تعديل/إلغاء دفعة حجز ملف {booking.BookingNumber} للمسافر {passenger?.FullName ?? "غير معروف"}",
                Amount = payment.Amount,
                Currency = BusinessManagement.Domain.Enums.Currency.EGP,
                TransactionType = BusinessManagement.Domain.Enums.TransactionType.Withdrawal,
                AssociatedPartnerId = booking.PartnerId
            };
            await _unitOfWork.GetRepository<SafeTransaction>().AddAsync(safeTx);
        }

        await _unitOfWork.CompleteAsync();
    }
    #endregion

    #region Full business Excel import
    public async Task<ExcelImportResultDto> ImportFullBusinessExcelAsync(System.IO.Stream fileStream)
    {
        _logger.LogInformation("Starting full business Excel import...");
        var result = new ExcelImportResultDto();
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        using var package = new OfficeOpenXml.ExcelPackage(fileStream);

        // Fetch defaults to map FKs
        var defaultTenant = (await _unitOfWork.GetRepository<Tenant>().GetAllAsync()).FirstOrDefault();
        var defaultBranch = (await _unitOfWork.GetRepository<Branch>().GetAllAsync()).FirstOrDefault();
        var defaultPackage = (await _unitOfWork.GetRepository<Package>().GetAllAsync()).FirstOrDefault();
        var defaultTenantId = defaultTenant?.Id ?? Guid.Empty;
        var defaultBranchId = defaultBranch?.Id ?? Guid.Empty;
        var defaultPackageId = defaultPackage?.Id ?? Guid.Empty;

        // Fetch all partners
        var partners = (await _unitOfWork.GetRepository<Partner>().GetAllAsync()).ToList();

        // 1. Parse 'الضمان' Sheet
        var capitalSheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("الضمان"));
        if (capitalSheet != null)
        {
            try
            {
                var capitalRepo = _unitOfWork.GetRepository<PartnershipCapital>();
                var safeRepo = _unitOfWork.GetRepository<SafeTransaction>();

                // Clear existing
                var existingCapitals = await capitalRepo.GetAllAsync();
                foreach (var ec in existingCapitals) capitalRepo.Delete(ec);

                var rows = capitalSheet.Dimension?.Rows ?? 0;
                for (int row = 3; row <= rows; row++)
                {
                    var name = capitalSheet.Cells[row, 1].Text?.Trim();
                    if (string.IsNullOrEmpty(name) || name == "0" || name.Contains("سعر") || name.Contains("المجموع") || name.Contains("نسبة")) continue;

                    decimal.TryParse(capitalSheet.Cells[row, 2].Text, out decimal amountSar);
                    decimal.TryParse(capitalSheet.Cells[row, 3].Text, out decimal amountEgp);
                    decimal.TryParse(capitalSheet.Cells[row, 4].Text, out decimal rate);
                    decimal.TryParse(capitalSheet.Cells[row, 5].Text, out decimal shareRatio);
                    decimal.TryParse(capitalSheet.Cells[row, 6].Text, out decimal profitShareRatio);
                    var notes = capitalSheet.Cells[row, 12].Text?.Trim() ?? "";

                    if (rate == 0) rate = 13.0m;

                    var capital = new PartnershipCapital
                    {
                        TenantId = defaultTenantId,
                        ShareholderName = name,
                        AmountSar = amountSar,
                        AmountEgp = amountEgp,
                        HistoricalRate = rate,
                        ShareRatio = shareRatio,
                        ProfitShareRatio = profitShareRatio,
                        Notes = notes
                    };
                    await capitalRepo.AddAsync(capital);
                    result.CapitalImportedCount++;

                    // Log initial safe deposits for these investments
                    if (amountEgp > 0)
                    {
                        await safeRepo.AddAsync(new SafeTransaction
                        {
                            TenantId = defaultTenantId,
                            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                            Amount = amountEgp,
                            TransactionType = Domain.Enums.TransactionType.Deposit,
                            Currency = Domain.Enums.Currency.EGP,
                            ExchangeRate = 1.0m,
                            DepositorOrWithdrawerName = name,
                            Description = "إيداع رأسمالي (تأسيسي) بالجنيه",
                            BankName = "الخزينة الرئيسية"
                        });
                    }
                    if (amountSar > 0)
                    {
                        await safeRepo.AddAsync(new SafeTransaction
                        {
                            TenantId = defaultTenantId,
                            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                            Amount = amountSar,
                            TransactionType = Domain.Enums.TransactionType.Deposit,
                            Currency = Domain.Enums.Currency.SAR,
                            ExchangeRate = rate,
                            DepositorOrWithdrawerName = name,
                            Description = "إيداع رأسمالي (تأسيسي) بالريال",
                            BankName = "الخزينة الرئيسية"
                        });
                    }
                }
                result.Messages.Add($"تم استيراد {result.CapitalImportedCount} سجلات من هيكل رأس المال والضمان.");
            }
            catch (Exception ex)
            {
                result.Messages.Add($"خطأ أثناء استيراد شيت الضمان: {ex.Message}");
            }
        }

        // 2. Parse 'درفت' Sheet (Bookings and Passengers)
        var draftSheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("درفت"));
        if (draftSheet != null)
        {
            try
            {
                var passengerRepo = _unitOfWork.GetRepository<Passenger>();
                var bookingRepo = _unitOfWork.GetRepository<Booking>();
                var paymentRepo = _unitOfWork.GetRepository<BookingPayment>();
                var safeRepo = _unitOfWork.GetRepository<SafeTransaction>();

                var rows = draftSheet.Dimension?.Rows ?? 0;
                for (int row = 3; row <= rows; row++)
                {
                    var name = draftSheet.Cells[row, 4].Text?.Trim();
                    if (string.IsNullOrEmpty(name) || name == "0" || name.Contains("الاسم")) continue;

                    var partnerCode = draftSheet.Cells[row, 2].Text?.Trim()?.ToUpper();
                    // Map partner code
                    var partner = partners.FirstOrDefault(p => p.Username.Equals(partnerCode, StringComparison.OrdinalIgnoreCase));
                    if (partner == null)
                    {
                        if (partnerCode == "O") partner = partners.FirstOrDefault(p => p.Username == "omar");
                        else if (partnerCode == "A") partner = partners.FirstOrDefault(p => p.Username == "alaa");
                        else if (partnerCode == "K") partner = partners.FirstOrDefault(p => p.Username == "khaled");
                        else if (partnerCode == "W") partner = partners.FirstOrDefault(p => p.Username == "waheed");
                        else partner = partners.FirstOrDefault(p => p.Username == "admin");
                    }

                    // Parse costs
                    decimal.TryParse(draftSheet.Cells[row, 5].Text, out decimal netCost);
                    decimal.TryParse(draftSheet.Cells[row, 6].Text, out decimal barcodeCost);
                    decimal.TryParse(draftSheet.Cells[row, 7].Text, out decimal companyCost);
                    decimal.TryParse(draftSheet.Cells[row, 9].Text, out decimal sellingPrice);
                    decimal.TryParse(draftSheet.Cells[row, 11].Text, out decimal paymentsCollected);

                    // Travel date
                    DateOnly travelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
                    var dateCell = draftSheet.Cells[row, 3].Value;
                    if (dateCell is DateTime dt) travelDate = DateOnly.FromDateTime(dt);
                    else if (dateCell != null && DateOnly.TryParse(draftSheet.Cells[row, 3].Text, out var parsedDate)) travelDate = parsedDate;

                    // Find or create passenger
                    var searchHash = BusinessManagement.Shared.Security.EncryptionHelper.HashSearchableField(name);
                    var existingPassengers = await passengerRepo.FindAsync(p => p.PassportNumberHash == searchHash || p.FullName == name);
                    var passenger = existingPassengers.FirstOrDefault();
                    if (passenger == null)
                    {
                        passenger = new Passenger
                        {
                            TenantId = defaultTenantId,
                            FullName = name,
                            PassportNumber = Guid.NewGuid().ToString("N").Substring(0, 9).ToUpper(), // Mock passport
                            PassportNumberHash = searchHash,
                            PassportExpiry = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5)),
                            Nationality = "Egyptian",
                            Gender = "Male",
                            CreatedBy = "system",
                            UpdatedBy = "system"
                        };
                        await passengerRepo.AddAsync(passenger);
                        await _unitOfWork.CompleteAsync(); // Save to get Id
                    }

                    // Create booking
                    var booking = new Booking
                    {
                        TenantId = defaultTenantId,
                        BranchId = defaultBranchId,
                        BookingNumber = $"B-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                        PassengerId = passenger.Id,
                        PartnerId = partner?.Id ?? Guid.Empty,
                        PackageId = defaultPackageId,
                        TravelDate = travelDate,
                        CurrentStatus = paymentsCollected >= sellingPrice && sellingPrice > 0 ? "Confirmed" : "Draft",
                        SellingPrice = sellingPrice,
                        PaymentsCollected = paymentsCollected,
                        RemainingBalance = sellingPrice - paymentsCollected,
                        HasQrCode = barcodeCost > 0
                    };
                    booking.BookingCost = new BookingCost
                    {
                        NetCost = netCost,
                        BarcodeCost = barcodeCost,
                        CompanyMarkup = companyCost
                    };

                    await bookingRepo.AddAsync(booking);
                    await _unitOfWork.CompleteAsync(); // Save to get Booking Id

                    // Create payment if collected
                    if (paymentsCollected > 0)
                    {
                        var payment = new BookingPayment
                        {
                            BookingId = booking.Id,
                            Amount = paymentsCollected,
                            Currency = "EGP",
                            PaymentMethod = "Cash",
                            ReceiptNumber = $"R-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
                            PaymentDate = travelDate
                        };
                        await paymentRepo.AddAsync(payment);

                        // Log safe deposit
                        await safeRepo.AddAsync(new SafeTransaction
                        {
                            TenantId = defaultTenantId,
                            TransactionDate = travelDate,
                            Amount = paymentsCollected,
                            TransactionType = Domain.Enums.TransactionType.Deposit,
                            Currency = Domain.Enums.Currency.EGP,
                            ExchangeRate = 1.0m,
                            DepositorOrWithdrawerName = name,
                            Description = $"تحصيل دفعة حجز مسافر {name} لشريك {partner?.Name}",
                            AssociatedPartnerId = partner?.Id
                        });
                    }

                    result.BookingsImportedCount++;
                }
                result.Messages.Add($"تم استيراد {result.BookingsImportedCount} حجوزات ومسافرين.");
            }
            catch (Exception ex)
            {
                result.Messages.Add($"خطأ أثناء استيراد شيت درفت: {ex.Message}");
            }
        }

        // 3. Parse 'تاشيرات خارجي' Sheet
        var visaSheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("تاشيرات") || w.Name.Contains("خارجي"));
        if (visaSheet != null)
        {
            try
            {
                var visaRepo = _unitOfWork.GetRepository<ExternalVisa>();
                var rows = visaSheet.Dimension?.Rows ?? 0;
                for (int row = 3; row <= rows; row++)
                {
                    var name = visaSheet.Cells[row, 1].Text?.Trim();
                    if (string.IsNullOrEmpty(name) || name == "0" || name.Contains("الاسم")) continue;

                    var partnerCode = visaSheet.Cells[row, 2].Text?.Trim()?.ToUpper();
                    var partner = partners.FirstOrDefault(p => p.Username.Equals(partnerCode, StringComparison.OrdinalIgnoreCase));
                    if (partner == null)
                    {
                        if (partnerCode == "O") partner = partners.FirstOrDefault(p => p.Username == "omar");
                        else if (partnerCode == "A") partner = partners.FirstOrDefault(p => p.Username == "alaa");
                        else if (partnerCode == "K") partner = partners.FirstOrDefault(p => p.Username == "khaled");
                        else if (partnerCode == "W") partner = partners.FirstOrDefault(p => p.Username == "waheed");
                        else partner = partners.FirstOrDefault(p => p.Username == "admin");
                    }

                    decimal.TryParse(visaSheet.Cells[row, 3].Text, out decimal netCost);
                    decimal.TryParse(visaSheet.Cells[row, 4].Text, out decimal barcodeCost);
                    decimal.TryParse(visaSheet.Cells[row, 5].Text, out decimal agentCommission);
                    decimal.TryParse(visaSheet.Cells[row, 6].Text, out decimal agreementCost);
                    decimal.TryParse(visaSheet.Cells[row, 7].Text, out decimal ticketCost);
                    decimal.TryParse(visaSheet.Cells[row, 8].Text, out decimal airportCost);
                    decimal.TryParse(visaSheet.Cells[row, 9].Text, out decimal busCost);
                    decimal.TryParse(visaSheet.Cells[row, 10].Text, out decimal totalCost);
                    decimal.TryParse(visaSheet.Cells[row, 11].Text, out decimal sellingPrice);
                    decimal.TryParse(visaSheet.Cells[row, 12].Text, out decimal netProfit);
                    decimal.TryParse(visaSheet.Cells[row, 13].Text, out decimal amountPaid);
                    decimal.TryParse(visaSheet.Cells[row, 14].Text, out decimal remainingBalance);

                    var visa = new ExternalVisa
                    {
                        TenantId = defaultTenantId,
                        PassengerName = name,
                        Affiliation = partnerCode ?? "system",
                        NetCost = netCost,
                        BarcodeCost = barcodeCost,
                        AgentCommission = agentCommission,
                        AgreementCost = agreementCost,
                        TicketCost = ticketCost,
                        AirportCost = airportCost,
                        BusCost = busCost,
                        TotalCost = totalCost,
                        SellingPrice = sellingPrice,
                        NetProfit = netProfit,
                        AmountPaid = amountPaid,
                        RemainingBalance = remainingBalance
                    };
                    await visaRepo.AddAsync(visa);
                    result.ExternalVisasImportedCount++;
                }
                result.Messages.Add($"تم استيراد {result.ExternalVisasImportedCount} تأشيرات خارجية.");
            }
            catch (Exception ex)
            {
                result.Messages.Add($"خطأ أثناء استيراد شيت تأشيرات خارجي: {ex.Message}");
            }
        }

        // 4. Parse 'Khz' Sheet
        var khzSheet = package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("Khz") || w.Name.Contains("خز"));
        if (khzSheet != null)
        {
            try
            {
                var safeRepo = _unitOfWork.GetRepository<SafeTransaction>();
                var rows = khzSheet.Dimension?.Rows ?? 0;
                for (int row = 3; row <= rows; row++)
                {
                    var details = khzSheet.Cells[row, 8].Text?.Trim();
                    if (string.IsNullOrEmpty(details) || details == "0" || details.Contains("بيان")) continue;

                    var partnerCode = khzSheet.Cells[row, 2].Text?.Trim()?.ToUpper();
                    var partner = partners.FirstOrDefault(p => p.Username.Equals(partnerCode, StringComparison.OrdinalIgnoreCase));
                    if (partner == null)
                    {
                        if (partnerCode == "O") partner = partners.FirstOrDefault(p => p.Username == "omar");
                        else if (partnerCode == "A") partner = partners.FirstOrDefault(p => p.Username == "alaa");
                        else if (partnerCode == "K") partner = partners.FirstOrDefault(p => p.Username == "khaled");
                        else if (partnerCode == "W") partner = partners.FirstOrDefault(p => p.Username == "waheed");
                    }

                    decimal.TryParse(khzSheet.Cells[row, 4].Text, out decimal deposit);
                    decimal.TryParse(khzSheet.Cells[row, 5].Text, out decimal withdraw);
                    var person = khzSheet.Cells[row, 7].Text?.Trim() ?? "";

                    DateOnly txDate = DateOnly.FromDateTime(DateTime.UtcNow);
                    var dateCell = khzSheet.Cells[row, 3].Value;
                    if (dateCell is DateTime dt) txDate = DateOnly.FromDateTime(dt);
                    else if (dateCell != null && DateOnly.TryParse(khzSheet.Cells[row, 3].Text, out var parsedDate)) txDate = parsedDate;

                    if (deposit > 0)
                    {
                        await safeRepo.AddAsync(new SafeTransaction
                        {
                            TenantId = defaultTenantId,
                            TransactionDate = txDate,
                            Amount = deposit,
                            TransactionType = Domain.Enums.TransactionType.Deposit,
                            Currency = Domain.Enums.Currency.EGP,
                            ExchangeRate = 1.0m,
                            DepositorOrWithdrawerName = person,
                            Description = details,
                            AssociatedPartnerId = partner?.Id
                        });
                        result.SafeTransactionsImportedCount++;
                    }
                    if (withdraw > 0)
                    {
                        await safeRepo.AddAsync(new SafeTransaction
                        {
                            TenantId = defaultTenantId,
                            TransactionDate = txDate,
                            Amount = withdraw,
                            TransactionType = Domain.Enums.TransactionType.Withdrawal,
                            Currency = Domain.Enums.Currency.EGP,
                            ExchangeRate = 1.0m,
                            DepositorOrWithdrawerName = person,
                            Description = details,
                            AssociatedPartnerId = partner?.Id
                        });
                        result.SafeTransactionsImportedCount++;
                    }
                }
                result.Messages.Add($"تم استيراد {result.SafeTransactionsImportedCount} حركات خزينة إضافية.");
            }
            catch (Exception ex)
            {
                result.Messages.Add($"خطأ أثناء استيراد شيت حركة الخزينة: {ex.Message}");
            }
        }

        await _unitOfWork.CompleteAsync();
        return result;
    }
    #endregion
}





