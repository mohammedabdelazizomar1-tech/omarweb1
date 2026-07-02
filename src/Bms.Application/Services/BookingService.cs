using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Bms.Application.DTOs;
using Bms.Application.Interfaces;
using Bms.Domain.Entities;
using Bms.Domain.Repositories;

namespace Bms.Application.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BookingService> _logger;

    public BookingService(IUnitOfWork unitOfWork, ILogger<BookingService> logger)
    {
        _unitOfWork = unitOfWork;
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
        
        // Check for duplicate passport number
        var existing = await _unitOfWork.GetRepository<Passenger>().FindAsync(p => p.PassportNumber == model.PassportNumber.Trim().ToUpper());
        if (existing.Any())
        {
            throw new InvalidOperationException($"A passenger with passport number '{model.PassportNumber}' already exists.");
        }

        var passenger = new Passenger
        {
            FullName = model.FullName.Trim(),
            PassportNumber = model.PassportNumber.Trim().ToUpper(),
            Nationality = model.Nationality.Trim(),
            DateOfBirth = model.DateOfBirth,
            PhoneNumber = model.PhoneNumber?.Trim() ?? string.Empty
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
            CreatedAt = passenger.CreatedAt
        };
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
            CreatedAt = x.b.CreatedAt
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
            CreatedAt = b.CreatedAt
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

        var passenger = await _unitOfWork.GetRepository<Passenger>().GetByIdAsync(model.PassengerId);
        if (passenger == null)
        {
            throw new InvalidOperationException("Passenger record not found.");
        }

        // Quota safety validation lock (الكوتة check)
        if (model.HasQrCode)
        {
            var existingBookings = await _unitOfWork.GetRepository<Booking>().FindAsync(b => b.PartnerId == model.PartnerId && b.HasQrCode);
            int qrCount = existingBookings.Count();
            if (qrCount >= partner.QuotaLimit)
            {
                throw new InvalidOperationException($"Booking failed. Quota limit of {partner.QuotaLimit} slots exceeded for partner '{partner.Name}'.");
            }
        }

        // Sum calculations
        decimal totalCost = model.NetCost + model.BarcodeCost + model.CompanyCost + model.AirportCost + model.ProgramCost + model.TicketCost + model.BusCost;
        decimal netProfit = model.SellingPrice - totalCost;
        decimal remaining = model.SellingPrice - model.PaymentsCollected;

        // Auto-generation of unique Booking Number
        var bookingsCount = (await _unitOfWork.GetRepository<Booking>().GetAllAsync()).Count();
        string bookingNumber = $"B-{(bookingsCount + 1001):D6}";

        var booking = new Booking
        {
            BookingNumber = bookingNumber,
            PassengerId = model.PassengerId,
            PartnerId = model.PartnerId,
            TravelDate = model.TravelDate,
            NetCost = model.NetCost,
            BarcodeCost = model.BarcodeCost,
            CompanyCost = model.CompanyCost,
            AirportCost = model.AirportCost,
            ProgramCost = model.ProgramCost,
            TicketCost = model.TicketCost,
            BusCost = model.BusCost,
            TotalCost = totalCost,
            SellingPrice = model.SellingPrice,
            NetProfit = netProfit,
            PaymentsCollected = model.PaymentsCollected,
            RemainingBalance = remaining,
            HasQrCode = model.HasQrCode,
            Notes = model.Notes ?? string.Empty
        };

        await _unitOfWork.GetRepository<Booking>().AddAsync(booking);
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
            CreatedAt = booking.CreatedAt
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

        var passenger = await _unitOfWork.GetRepository<Passenger>().GetByIdAsync(model.PassengerId);
        if (passenger == null)
        {
            throw new InvalidOperationException("Passenger record not found.");
        }

        // Quota safety checks on update
        if (model.HasQrCode && (!booking.HasQrCode || booking.PartnerId != model.PartnerId))
        {
            var existingBookings = await _unitOfWork.GetRepository<Booking>().FindAsync(b => b.PartnerId == model.PartnerId && b.HasQrCode);
            int qrCount = existingBookings.Count();
            if (qrCount >= partner.QuotaLimit)
            {
                throw new InvalidOperationException($"Booking update failed. Quota limit of {partner.QuotaLimit} slots exceeded for partner '{partner.Name}'.");
            }
        }

        // Calculations
        decimal totalCost = model.NetCost + model.BarcodeCost + model.CompanyCost + model.AirportCost + model.ProgramCost + model.TicketCost + model.BusCost;
        decimal netProfit = model.SellingPrice - totalCost;
        decimal remaining = model.SellingPrice - model.PaymentsCollected;

        booking.PassengerId = model.PassengerId;
        booking.PartnerId = model.PartnerId;
        booking.TravelDate = model.TravelDate;
        booking.NetCost = model.NetCost;
        booking.BarcodeCost = model.BarcodeCost;
        booking.CompanyCost = model.CompanyCost;
        booking.AirportCost = model.AirportCost;
        booking.ProgramCost = model.ProgramCost;
        booking.TicketCost = model.TicketCost;
        booking.BusCost = model.BusCost;
        booking.TotalCost = totalCost;
        booking.SellingPrice = model.SellingPrice;
        booking.NetProfit = netProfit;
        booking.PaymentsCollected = model.PaymentsCollected;
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
            CreatedAt = booking.CreatedAt
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
    #endregion
}
