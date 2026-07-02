using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Bms.Application.DTOs;
using Bms.Application.Interfaces;
using Bms.Domain.Entities;
using Bms.Domain.Enums;
using Bms.Domain.Repositories;

namespace Bms.Application.Services;

public class TravelDashboardService : ITravelDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TravelDashboardService> _logger;

    public TravelDashboardService(IUnitOfWork unitOfWork, ILogger<TravelDashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TravelDashboardDto> GetDashboardDataAsync(string? partnerUsername = null)
    {
        _logger.LogInformation("Compiling dashboard operations cache. User: {User}", partnerUsername ?? "Admin");

        var bookings = await _unitOfWork.GetRepository<Booking>().GetAllAsync();
        var passengers = await _unitOfWork.GetRepository<Passenger>().GetAllAsync();
        var partners = await _unitOfWork.GetRepository<Partner>().GetAllAsync();
        var safeTransactions = await _unitOfWork.GetRepository<SafeTransaction>().GetAllAsync();
        var gatewayPurchases = await _unitOfWork.GetRepository<GatewayPortal>().GetAllAsync();

        // Safe balances EGP/SAR
        decimal egpDeposits = safeTransactions.Where(t => t.Currency == Currency.EGP && t.TransactionType == TransactionType.Deposit).Sum(t => t.Amount);
        decimal egpWithdrawals = safeTransactions.Where(t => t.Currency == Currency.EGP && t.TransactionType == TransactionType.Withdrawal).Sum(t => t.Amount);
        
        decimal sarDeposits = safeTransactions.Where(t => t.Currency == Currency.SAR && t.TransactionType == TransactionType.Deposit).Sum(t => t.Amount);
        decimal sarWithdrawals = safeTransactions.Where(t => t.Currency == Currency.SAR && t.TransactionType == TransactionType.Withdrawal).Sum(t => t.Amount);

        decimal safeBalanceEgp = egpDeposits - egpWithdrawals;
        decimal safeBalanceSar = sarDeposits - sarWithdrawals;

        // Portal stock balance
        int purchasedQr = gatewayPurchases.Where(p => p.ServiceType == PortalServiceType.QrCode).Sum(p => p.Count);
        int consumedQr = bookings.Count(b => b.HasQrCode);
        int availableQr = Math.Max(0, purchasedQr - consumedQr);

        int purchasedVip = gatewayPurchases.Where(p => p.ServiceType == PortalServiceType.Vip).Sum(p => p.Count);

        // Filter bookings if user is restricted as a partner
        var filteredBookings = bookings;
        if (!string.IsNullOrWhiteSpace(partnerUsername))
        {
            var userPartner = partners.FirstOrDefault(p => p.Username.Equals(partnerUsername, StringComparison.OrdinalIgnoreCase));
            if (userPartner != null)
            {
                filteredBookings = bookings.Where(b => b.PartnerId == userPartner.Id).ToList();
            }
        }

        // Calculations
        decimal totalSalesInvoiced = filteredBookings.Sum(b => b.SellingPrice);
        decimal totalPaymentsCollected = filteredBookings.Sum(b => b.PaymentsCollected);
        decimal totalRemainingReceivable = filteredBookings.Sum(b => b.RemainingBalance);
        decimal totalAccumulatedProfit = filteredBookings.Sum(b => b.NetProfit);

        // Compile Partner Quota details
        var partnerMetrics = partners.Select(p =>
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
        }).ToList();

        // Compile Recent Bookings lists
        var joinedRecent = (from b in filteredBookings
                            join p in passengers on b.PassengerId equals p.Id
                            join pt in partners on b.PartnerId equals pt.Id
                            select new RecentBookingSummary
                            {
                                BookingId = b.Id,
                                BookingNumber = b.BookingNumber,
                                PassengerName = p.FullName,
                                PartnerName = pt.Name,
                                TravelDate = b.TravelDate,
                                SellingPrice = b.SellingPrice,
                                Status = b.RemainingBalance <= 0 ? "Paid" : b.PaymentsCollected > 0 ? "Partial" : "Unpaid",
                                HasQrCode = b.HasQrCode
                            })
                            .OrderByDescending(r => r.BookingNumber)
                            .Take(8)
                            .ToList();

        return new TravelDashboardDto
        {
            SafeBalanceEgp = safeBalanceEgp,
            SafeBalanceSar = safeBalanceSar,
            PartnerQuotas = partnerMetrics,
            TotalSalesInvoiced = totalSalesInvoiced,
            TotalPaymentsCollected = totalPaymentsCollected,
            TotalRemainingReceivable = totalRemainingReceivable,
            TotalAccumulatedProfit = totalAccumulatedProfit,
            AvailableQrCodes = availableQr,
            AvailableVipSlots = purchasedVip,
            RecentBookings = joinedRecent
        };
    }
}
