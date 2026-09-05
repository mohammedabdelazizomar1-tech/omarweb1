using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Enums;
using BusinessManagement.Domain.Repositories;

namespace BusinessManagement.Application.Services;

public class SafeService : ISafeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SafeService> _logger;

    public SafeService(IUnitOfWork unitOfWork, ILogger<SafeService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<SafeTransactionDto>> GetAllTransactionsAsync()
    {
        _logger.LogInformation("Retrieving all safe cash flow transactions.");
        
        var transactions = await _unitOfWork.GetRepository<SafeTransaction>().GetAllAsync();
        var partners = await _unitOfWork.GetRepository<Partner>().GetAllAsync();

        var query = from t in transactions
                    join p in partners on t.AssociatedPartnerId equals p.Id into joined
                    from partner in joined.DefaultIfEmpty()
                    select new { t, partner };

        return query.Select(x => new SafeTransactionDto
        {
            Id = x.t.Id,
            TransactionDate = x.t.TransactionDate,
            Amount = x.t.Amount,
            TransactionType = x.t.TransactionType.ToString(),
            Currency = x.t.Currency.ToString(),
            ExchangeRate = x.t.ExchangeRate,
            AssociatedPartnerId = x.t.AssociatedPartnerId,
            AssociatedPartnerName = x.partner?.Name ?? "Corporate / Treasury",
            DepositorOrWithdrawerName = x.t.DepositorOrWithdrawerName,
            Description = x.t.Description,
            BankName = x.t.BankName,
            CreatedAt = x.t.CreatedAt
        }).OrderByDescending(x => x.TransactionDate);
    }

    public async Task<SafeTransactionDto?> GetTransactionByIdAsync(Guid id)
    {
        var t = await _unitOfWork.GetRepository<SafeTransaction>().GetByIdAsync(id);
        if (t == null) return null;

        var partner = t.AssociatedPartnerId.HasValue 
            ? await _unitOfWork.GetRepository<Partner>().GetByIdAsync(t.AssociatedPartnerId.Value) 
            : null;

        var paymentRepo = _unitOfWork.GetRepository<BookingPayment>();
        var bookingRepo = _unitOfWork.GetRepository<Booking>();
        var passengerRepo = _unitOfWork.GetRepository<Passenger>();

        var payments = await paymentRepo.FindAsync(p => p.ReceiptNumber.Contains($"#ST-{id}") || p.ReceiptNumber == $"ST-{id}");
        var details = new List<SafeTransactionPassengerDetailDto>();

        if (payments != null && payments.Any())
        {
            foreach (var payment in payments)
            {
                var booking = await bookingRepo.GetByIdAsync(payment.BookingId);
                if (booking != null)
                {
                    var passenger = await passengerRepo.GetByIdAsync(booking.PassengerId);
                    details.Add(new SafeTransactionPassengerDetailDto
                    {
                        BookingNumber = booking.BookingNumber,
                        PassengerName = passenger?.FullName ?? "غير معروف",
                        GroupNumber = booking.GroupNumber,
                        AmountPaid = payment.Amount
                    });
                }
            }
        }

        var groupsList = details
            .Where(d => !string.IsNullOrWhiteSpace(d.GroupNumber))
            .OrderBy(d => d.GroupNumber)
            .ThenBy(d => d.PassengerName)
            .ToList();

        var individualsList = details
            .Where(d => string.IsNullOrWhiteSpace(d.GroupNumber))
            .OrderBy(d => d.PassengerName)
            .ToList();

        var sortedDetails = new List<SafeTransactionPassengerDetailDto>();
        sortedDetails.AddRange(groupsList);
        sortedDetails.AddRange(individualsList);

        return new SafeTransactionDto
        {
            Id = t.Id,
            TransactionDate = t.TransactionDate,
            Amount = t.Amount,
            TransactionType = t.TransactionType.ToString(),
            Currency = t.Currency.ToString(),
            ExchangeRate = t.ExchangeRate,
            AssociatedPartnerId = t.AssociatedPartnerId,
            AssociatedPartnerName = partner?.Name ?? "Corporate / Treasury",
            DepositorOrWithdrawerName = t.DepositorOrWithdrawerName,
            Description = t.Description,
            BankName = t.BankName,
            CreatedAt = t.CreatedAt,
            PassengerDetails = sortedDetails
        };
    }

    public async Task<SafeTransactionDto> CreateTransactionAsync(CreateSafeTransactionDto model)
    {
        _logger.LogInformation("Logging cash safe transaction: Type={Type}, Currency={Currency}, Amount={Amount}", 
            model.TransactionType, model.Currency, model.Amount);

        var selectedBookingIds = new List<Guid>();
        if (model.BookingIds != null && model.BookingIds.Any())
        {
            selectedBookingIds.AddRange(model.BookingIds);
        }
        else if (model.BookingId.HasValue)
        {
            selectedBookingIds.Add(model.BookingId.Value);
        }

        SafeTransaction finalTransaction;

        if (selectedBookingIds.Any())
        {
            var bookingRepo = _unitOfWork.GetRepository<Booking>();
            var selectedBookings = new List<Booking>();
            foreach (var id in selectedBookingIds)
            {
                var b = await bookingRepo.GetByIdAsync(id);
                if (b != null)
                {
                    selectedBookings.Add(b);
                }
            }

            if (!selectedBookings.Any())
            {
                throw new KeyNotFoundException("No valid booking records were found.");
            }

            finalTransaction = await ProcessSubBookingTransactionAsync(model, selectedBookings);
            await _unitOfWork.CompleteAsync();
        }
        else
        {
            // Standard general transaction
            var transactionId = Guid.NewGuid();
            finalTransaction = new SafeTransaction
            {
                Id = transactionId,
                TransactionDate = model.TransactionDate,
                Amount = model.Amount,
                TransactionType = model.TransactionType,
                Currency = model.Currency,
                ExchangeRate = model.ExchangeRate,
                AssociatedPartnerId = model.AssociatedPartnerId,
                DepositorOrWithdrawerName = model.DepositorOrWithdrawerName.Trim(),
                Description = model.Description?.Trim() ?? string.Empty,
                BankName = model.BankName?.Trim() ?? string.Empty
            };

            if (model.TransactionType == TransactionType.Withdrawal && !string.IsNullOrWhiteSpace(model.WithdrawalCategory))
            {
                finalTransaction.Description = $"[{model.WithdrawalCategory.Trim()}] {finalTransaction.Description}".Trim();
            }

            await _unitOfWork.GetRepository<SafeTransaction>().AddAsync(finalTransaction);
            await _unitOfWork.CompleteAsync();
        }

        var partner = finalTransaction.AssociatedPartnerId.HasValue 
            ? await _unitOfWork.GetRepository<Partner>().GetByIdAsync(finalTransaction.AssociatedPartnerId.Value) 
            : null;

        return new SafeTransactionDto
        {
            Id = finalTransaction.Id,
            TransactionDate = finalTransaction.TransactionDate,
            Amount = finalTransaction.Amount,
            TransactionType = finalTransaction.TransactionType.ToString(),
            Currency = finalTransaction.Currency.ToString(),
            ExchangeRate = finalTransaction.ExchangeRate,
            AssociatedPartnerId = finalTransaction.AssociatedPartnerId,
            AssociatedPartnerName = partner?.Name ?? "Corporate / Treasury",
            DepositorOrWithdrawerName = finalTransaction.DepositorOrWithdrawerName,
            Description = finalTransaction.Description,
            BankName = finalTransaction.BankName,
            CreatedAt = finalTransaction.CreatedAt
        };
    }

    private async Task<SafeTransaction> ProcessSubBookingTransactionAsync(CreateSafeTransactionDto model, List<Booking> bookings)
    {
        var transactionId = Guid.NewGuid();
        var transaction = new SafeTransaction
        {
            Id = transactionId,
            TransactionDate = model.TransactionDate,
            TransactionType = TransactionType.Deposit,
            Currency = Currency.EGP,
            ExchangeRate = 1.0m,
            BankName = model.BankName?.Trim() ?? string.Empty
        };

        var bookingRepo = _unitOfWork.GetRepository<Booking>();
        var paymentRepo = _unitOfWork.GetRepository<BookingPayment>();
        var passengerRepo = _unitOfWork.GetRepository<Passenger>();

        decimal totalAllocated = 0;
        var bookingNumbers = new List<string>();
        var passengerNames = new List<string>();
        Guid? partnerId = null;

        foreach (var booking in bookings.OrderBy(b => b.BookingNumber))
        {
            decimal allocation = 0;
            if (model.BookingPayments != null && model.BookingPayments.TryGetValue(booking.Id, out decimal explicitAmount))
            {
                allocation = explicitAmount;
            }
            else
            {
                allocation = booking.RemainingBalance;
            }

            if (allocation <= 0) continue;

            if (allocation > booking.RemainingBalance + 0.01m)
            {
                throw new InvalidOperationException($"المبلغ المدخل للمسافر ({allocation:N2} ج.م) أكبر من الرصيد المتبقي للملف ({booking.RemainingBalance:N2} ج.م).");
            }

            // Create Booking Payment linked using the correct ST- prefix
            var payment = new BookingPayment
            {
                BookingId = booking.Id,
                Amount = allocation,
                Currency = "EGP",
                PaymentMethod = "Cash",
                ReceiptNumber = $"ST-{transactionId}",
                PaymentDate = model.TransactionDate
            };
            await paymentRepo.AddAsync(payment);

            // Update Booking Balances
            booking.PaymentsCollected += allocation;
            booking.RemainingBalance = booking.SellingPrice - booking.PaymentsCollected;
            bookingRepo.Update(booking);

            totalAllocated += allocation;
            bookingNumbers.Add(booking.BookingNumber);
            var passenger = await passengerRepo.GetByIdAsync(booking.PassengerId);
            if (passenger != null && !passengerNames.Contains(passenger.FullName))
            {
                passengerNames.Add(passenger.FullName);
            }
            partnerId = booking.PartnerId;
        }

        transaction.Amount = totalAllocated;
        transaction.AssociatedPartnerId = partnerId;
        
        var groupNumbers = bookings
            .Where(b => !string.IsNullOrWhiteSpace(b.GroupNumber))
            .Select(b => b.GroupNumber!)
            .Distinct()
            .OrderBy(g => g)
            .ToList();

        var individualBookingNumbers = bookings
            .Where(b => string.IsNullOrWhiteSpace(b.GroupNumber))
            .Select(b => b.BookingNumber)
            .Distinct()
            .OrderBy(bn => bn)
            .ToList();

        var depositors = new List<string>();
        if (groupNumbers.Any()) depositors.AddRange(groupNumbers);
        if (individualBookingNumbers.Any()) depositors.AddRange(individualBookingNumbers);

        transaction.DepositorOrWithdrawerName = depositors.Any()
            ? string.Join(" / ", depositors)
            : "غير معروف";

        transaction.Description = !string.IsNullOrWhiteSpace(model.Description)
            ? model.Description.Trim()
            : $"دفعة حجز ملفات {string.Join(", ", bookingNumbers)} للمسافرين {string.Join(", ", passengerNames)} (من الخزنة)";

        await _unitOfWork.GetRepository<SafeTransaction>().AddAsync(transaction);
        return transaction;
    }

    public async Task DeleteTransactionAsync(Guid id)
    {
        _logger.LogWarning("Deleting cash safe transaction record: {TransactionId}", id);
        
        var transaction = await _unitOfWork.GetRepository<SafeTransaction>().GetByIdAsync(id);
        if (transaction == null)
        {
            throw new KeyNotFoundException("Safe transaction record not found.");
        }

        // Revert associated BookingPayments if any exist
        var paymentRepo = _unitOfWork.GetRepository<BookingPayment>();
        var bookingRepo = _unitOfWork.GetRepository<Booking>();
        
        var linkedPayments = await paymentRepo.FindAsync(p => p.ReceiptNumber.Contains($"#ST-{id}") || p.ReceiptNumber == $"ST-{id}");
        if (linkedPayments != null && linkedPayments.Any())
        {
            _logger.LogInformation("Found {Count} booking payments associated with safe transaction {TransactionId}. Reverting...", 
                linkedPayments.Count(), id);

            foreach (var payment in linkedPayments)
            {
                var booking = await bookingRepo.GetByIdAsync(payment.BookingId);
                if (booking != null)
                {
                    booking.PaymentsCollected -= payment.Amount;
                    booking.RemainingBalance = booking.SellingPrice - booking.PaymentsCollected;
                    bookingRepo.Update(booking);
                }
            }

            // Perform direct bulk delete for the booking payments to bypass tracked optimistic concurrency issues
            await paymentRepo.ExecuteDeleteAsync(p => p.ReceiptNumber.Contains($"#ST-{id}") || p.ReceiptNumber == $"ST-{id}");
        }

        _unitOfWork.GetRepository<SafeTransaction>().Delete(transaction);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<(decimal Egp, decimal Sar)> GetSafeBalancesAsync()
    {
        var transactions = await _unitOfWork.GetRepository<SafeTransaction>().GetAllAsync();
        
        decimal egpDeposits = transactions.Where(t => t.Currency == Currency.EGP && t.TransactionType == TransactionType.Deposit).Sum(t => t.Amount);
        decimal egpWithdrawals = transactions.Where(t => t.Currency == Currency.EGP && t.TransactionType == TransactionType.Withdrawal).Sum(t => t.Amount);
        
        decimal sarDeposits = transactions.Where(t => t.Currency == Currency.SAR && t.TransactionType == TransactionType.Deposit).Sum(t => t.Amount);
        decimal sarWithdrawals = transactions.Where(t => t.Currency == Currency.SAR && t.TransactionType == TransactionType.Withdrawal).Sum(t => t.Amount);

        return (Egp: egpDeposits - egpWithdrawals, Sar: sarDeposits - sarWithdrawals);
    }
}





