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
        var externalVisas = await _unitOfWork.GetRepository<ExternalVisa>().GetAllAsync();
        var activityLogs = await _unitOfWork.GetRepository<ActivityLog>().GetAllAsync();
        var capitals = await _unitOfWork.GetRepository<PartnershipCapital>().GetAllAsync();

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
        var filteredBookings = bookings.ToList();
        if (!string.IsNullOrWhiteSpace(partnerUsername))
        {
            var userPartner = partners.FirstOrDefault(p => p.Username.Equals(partnerUsername, StringComparison.OrdinalIgnoreCase));
            if (userPartner != null)
            {
                filteredBookings = bookings.Where(b => b.PartnerId == userPartner.Id).ToList();
            }
        }

        // Financial KPIs
        decimal totalSalesInvoiced = filteredBookings.Sum(b => b.SellingPrice);
        decimal totalExpenses = filteredBookings.Sum(b => b.TotalCost);
        decimal totalPaymentsCollected = filteredBookings.Sum(b => b.PaymentsCollected);
        decimal totalRemainingReceivable = filteredBookings.Sum(b => b.RemainingBalance);
        decimal totalAccumulatedProfit = filteredBookings.Sum(b => b.NetProfit);

        // Detailed expenses calculations
        decimal gatewayExpenses = bookings.Sum(b => b.NetCost + b.BarcodeCost) + externalVisas.Sum(v => v.NetCost + v.BarcodeCost);
        decimal agencyExpenses = bookings.Sum(b => b.CompanyCost) + externalVisas.Sum(v => v.AgentCommission + v.AgreementCost);
        decimal aviationExpenses = bookings.Sum(b => b.TicketCost) + externalVisas.Sum(v => v.TicketCost);
        decimal otherExpenses = bookings.Sum(b => b.AirportCost + b.ProgramCost + b.BusCost) + externalVisas.Sum(v => v.AirportCost + v.BusCost);

        // Barcode linking
        decimal gatewayBarcodeSpent = gatewayPurchases.Where(p => p.ServiceType == PortalServiceType.QrCode).Sum(p => p.Amount);
        decimal bookingsBarcodeAllocated = bookings.Sum(b => b.BarcodeCost) + externalVisas.Sum(v => v.BarcodeCost);

        // Counts
        int pendingPaymentsCount = filteredBookings.Count(b => b.RemainingBalance > 0);
        int completedPaymentsCount = filteredBookings.Count(b => b.RemainingBalance <= 0);
        
        var today = DateOnly.FromDateTime(DateTime.Today);
        int upcomingTripsCount = filteredBookings.Count(b => b.TravelDate >= today);
        
        int totalPassengersCount = passengers.Count();
        int totalBookingsCount = bookings.Count();
        int totalExternalVisasCount = externalVisas.Count();
        int activeGatewaysCount = gatewayPurchases.Count();

        // Calculate dynamic partner ledgers and quotas
        decimal totalCompanyProfit = bookings.Sum(b => b.NetProfit);
        var partnerLedgers = new List<PartnerLedgerMetric>();
        
        var partnerMetrics = partners.Select(p =>
        {
            var pBookings = bookings.Where(b => b.PartnerId == p.Id).ToList();
            var normName = p.Name.Trim().ToLower();
            
            var cap = capitals.FirstOrDefault(c => 
                c.ShareholderName.Trim().ToLower() == normName || 
                c.ShareholderName.Trim().ToLower() == p.Username.Trim().ToLower());

            decimal profitShareRatio = cap?.ProfitShareRatio ?? 0;
            decimal profitShare = totalCompanyProfit * profitShareRatio;
            
            decimal withdrawals = safeTransactions
                .Where(t => (t.AssociatedPartnerId == p.Id || t.DepositorOrWithdrawerName.Trim().ToLower() == normName) && t.TransactionType == TransactionType.Withdrawal)
                .Sum(t => t.Amount * t.ExchangeRate);

            decimal bookingsProfit = pBookings.Sum(b => b.NetProfit);
            decimal remainingBalance = pBookings.Sum(b => b.RemainingBalance);
            decimal netLedgerBalance = profitShare - withdrawals;

            int clientsCount = pBookings.Count;
            int qrCount = pBookings.Count(b => b.HasQrCode || b.BarcodeCost > 0);
            int nonQrCount = pBookings.Count(b => !b.HasQrCode && b.BarcodeCost == 0);
            decimal totalAccount = pBookings.Sum(b => b.SellingPrice);
            decimal paidAmount = pBookings.Sum(b => b.PaymentsCollected);
            decimal averageProfitPerClient = clientsCount > 0 ? (bookingsProfit / clientsCount) : 0;

            partnerLedgers.Add(new PartnerLedgerMetric
            {
                PartnerId = p.Id,
                PartnerName = p.Name,
                ClientsCount = clientsCount,
                QrCount = qrCount,
                NonQrCount = nonQrCount,
                TotalAccount = totalAccount,
                PaidAmount = paidAmount,
                RemainingBalance = remainingBalance,
                BookingsProfit = bookingsProfit,
                AverageProfitPerClient = averageProfitPerClient,
                ProfitShare = profitShare,
                Withdrawals = withdrawals,
                NetLedgerBalance = netLedgerBalance
            });

            return new PartnerQuotaMetric
            {
                PartnerId = p.Id,
                PartnerName = p.Name,
                BookingsCount = pBookings.Count,
                QrCount = pBookings.Count(b => b.HasQrCode || b.BarcodeCost > 0),
                NonQrCount = pBookings.Count(b => !b.HasQrCode && b.BarcodeCost == 0),
                QuotaLimit = p.QuotaLimit,
                TotalProfit = bookingsProfit,
                CollectedPayments = pBookings.Sum(b => b.PaymentsCollected),
                RemainingBalance = remainingBalance,
                ProfitShare = profitShare,
                Withdrawals = withdrawals
            };
        }).ToList();

        // Compile Recent Bookings
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

        // Compile Recent Activities Feed
        var recentActivities = activityLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(8)
            .Select(a => new ActivityLogDto
            {
                UserId = a.UserId,
                Action = a.Action,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent.Contains("Windows") ? "Windows PC" : a.UserAgent.Contains("Android") || a.UserAgent.Contains("iPhone") ? "Mobile" : "Web browser",
                CreatedAt = a.CreatedAt
            })
            .ToList();

        // Compile Monthly Revenue & Profit Chart Datasets
        var monthlyData = filteredBookings
            .GroupBy(b => new { b.TravelDate.Year, b.TravelDate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .TakeLast(6)
            .ToList();

        var monthlyRevenueLabels = monthlyData.Select(g => $"{g.Key.Year}-{g.Key.Month:D2}").ToList();
        var monthlyRevenueData = monthlyData.Select(g => g.Sum(b => b.SellingPrice)).ToList();
        var monthlyProfitData = monthlyData.Select(g => g.Sum(b => b.NetProfit)).ToList();

        // Compile Cash Flow Chart Datasets
        var cashFlowData = safeTransactions
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .TakeLast(6)
            .ToList();

        var cashFlowLabels = cashFlowData.Select(g => $"{g.Key.Year}-{g.Key.Month:D2}").ToList();
        var cashFlowDeposits = cashFlowData.Select(g => g.Where(t => t.TransactionType == TransactionType.Deposit).Sum(t => t.Amount)).ToList();
        var cashFlowWithdrawals = cashFlowData.Select(g => g.Where(t => t.TransactionType == TransactionType.Withdrawal).Sum(t => t.Amount)).ToList();

        return new TravelDashboardDto
        {
            SafeBalanceEgp = safeBalanceEgp,
            SafeBalanceSar = safeBalanceSar,
            PartnerQuotas = partnerMetrics,
            TotalSalesInvoiced = totalSalesInvoiced,
            TotalPaymentsCollected = totalPaymentsCollected,
            TotalRemainingReceivable = totalRemainingReceivable,
            TotalAccumulatedProfit = totalAccumulatedProfit,
            TotalExpenses = totalExpenses,
            PendingPaymentsCount = pendingPaymentsCount,
            CompletedPaymentsCount = completedPaymentsCount,
            UpcomingTripsCount = upcomingTripsCount,
            TotalPassengersCount = totalPassengersCount,
            TotalBookingsCount = totalBookingsCount,
            TotalExternalVisasCount = totalExternalVisasCount,
            ActiveGatewaysCount = activeGatewaysCount,
            AvailableQrCodes = availableQr,
            AvailableVipSlots = purchasedVip,
            RecentBookings = joinedRecent,
            RecentActivities = recentActivities,
            MonthlyRevenueLabels = monthlyRevenueLabels,
            MonthlyRevenueData = monthlyRevenueData,
            MonthlyProfitData = monthlyProfitData,
            CashFlowLabels = cashFlowLabels,
            CashFlowDeposits = cashFlowDeposits,
            CashFlowWithdrawals = cashFlowWithdrawals,
            
            GatewayExpenses = gatewayExpenses,
            AgencyExpenses = agencyExpenses,
            AviationExpenses = aviationExpenses,
            OtherExpenses = otherExpenses,
            GatewayBarcodeSpent = gatewayBarcodeSpent,
            BookingsBarcodeAllocated = bookingsBarcodeAllocated,
            PartnerLedgers = partnerLedgers
        };
    }
}
