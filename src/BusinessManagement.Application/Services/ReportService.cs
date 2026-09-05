using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;

namespace BusinessManagement.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerReportDto> GetCustomerReportAsync(DateTime? fromDate, DateTime? toDate)
    {
        var passengerRepo = _unitOfWork.GetRepository<Passenger>();
        var bookingRepo = _unitOfWork.GetRepository<Booking>();

        var passengers = await passengerRepo.GetAllAsync();
        var bookings = await bookingRepo.GetAllAsync();

        // Apply filters on passengers if applicable
        var filteredPassengers = passengers.ToList();
        
        var dto = new CustomerReportDto
        {
            TotalCustomers = filteredPassengers.Count,
            TotalGroups = bookings.Select(b => b.GroupNumber).Distinct().Count(g => !string.IsNullOrEmpty(g))
        };

        // Nationalities Breakdown
        dto.CustomersByNationality = filteredPassengers
            .GroupBy(p => string.IsNullOrWhiteSpace(p.Nationality) ? "غير محدد" : p.Nationality)
            .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
            .OrderByDescending(kv => kv.Value)
            .ToList();

        // Gender Breakdown
        dto.CustomersByGender = filteredPassengers
            .GroupBy(p => string.IsNullOrWhiteSpace(p.Gender) ? "غير محدد" : p.Gender)
            .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
            .ToList();

        // Most Frequent customers
        dto.MostFrequentCustomers = filteredPassengers
            .Select(p => new CustomerFrequencyDto
            {
                Name = p.FullName,
                PassportNumber = p.PassportNumber,
                BookingsCount = bookings.Count(b => b.PassengerId == p.Id)
            })
            .OrderByDescending(c => c.BookingsCount)
            .Take(10)
            .ToList();

        return dto;
    }

    public async Task<BookingReportDto> GetBookingReportAsync(DateTime? fromDate, DateTime? toDate)
    {
        var bookingRepo = _unitOfWork.GetRepository<Booking>();
        var passengerRepo = _unitOfWork.GetRepository<Passenger>();
        var partnerRepo = _unitOfWork.GetRepository<Partner>();

        var bookings = await bookingRepo.GetAllAsync();
        var passengers = await passengerRepo.GetAllAsync();
        var partners = await partnerRepo.GetAllAsync();

        var query = bookings.AsQueryable();
        if (fromDate.HasValue)
        {
            var fromOnly = DateOnly.FromDateTime(fromDate.Value);
            query = query.Where(b => b.TravelDate >= fromOnly);
        }
        if (toDate.HasValue)
        {
            var toOnly = DateOnly.FromDateTime(toDate.Value);
            query = query.Where(b => b.TravelDate <= toOnly);
        }

        var filtered = query.ToList();

        var dto = new BookingReportDto
        {
            TotalBookingsCount = filtered.Count,
            TotalSellingPrice = filtered.Sum(b => b.SellingPrice),
            TotalCost = filtered.Sum(b => b.TotalCost),
            TotalNetProfit = filtered.Sum(b => b.NetProfit)
        };

        // Detailed list
        dto.DetailedBookings = filtered.Select(b => new BookingDetailReportDto
        {
            BookingId = b.Id,
            PassengerName = passengers.FirstOrDefault(p => p.Id == b.PassengerId)?.FullName ?? "غير معروف",
            PartnerName = partners.FirstOrDefault(p => p.Id == b.PartnerId)?.Name ?? "مكتب رئيسي",
            TravelDate = b.TravelDate,
            SellingPrice = b.SellingPrice,
            NetCost = b.TotalCost,
            Profit = b.NetProfit
        })
        .OrderByDescending(b => b.TravelDate)
        .Take(50)
        .ToList();

        return dto;
    }

    public async Task<VisaReportDto> GetVisaReportAsync(DateTime? fromDate, DateTime? toDate)
    {
        var bookingRepo = _unitOfWork.GetRepository<Booking>();
        var passengerRepo = _unitOfWork.GetRepository<Passenger>();
        var extRepo = _unitOfWork.GetRepository<ExternalVisa>();

        var bookings = await bookingRepo.GetAllAsync();
        var passengers = await passengerRepo.GetAllAsync();
        var externalVisas = await extRepo.GetAllAsync();

        var query = bookings.AsQueryable();
        if (fromDate.HasValue)
        {
            var fromOnly = DateOnly.FromDateTime(fromDate.Value);
            query = query.Where(b => b.TravelDate >= fromOnly);
        }
        if (toDate.HasValue)
        {
            var toOnly = DateOnly.FromDateTime(toDate.Value);
            query = query.Where(b => b.TravelDate <= toOnly);
        }

        var filtered = query.ToList();

        var dto = new VisaReportDto
        {
            TotalVisas = filtered.Count + externalVisas.Count(),
            QrCodeVisasCount = filtered.Count(b => b.HasQrCode),
            VipSlotsCount = filtered.Count(b => b.AirportCost > 0),
            ExternalVisasCount = externalVisas.Count()
        };

        // Compile visas list
        dto.VisasList = filtered.Select(b => new VisaDetailDto
        {
            PassengerName = passengers.FirstOrDefault(p => p.Id == b.PassengerId)?.FullName ?? "غير معروف",
            PassportNumber = passengers.FirstOrDefault(p => p.Id == b.PassengerId)?.PassportNumber ?? "",
            VisaType = b.AirportCost > 0 ? "VIP سلفت" : "تأشيرة B2C / أفراد",
            HasQrCode = b.HasQrCode,
            Status = b.TravelDate >= DateOnly.FromDateTime(DateTime.Today) ? "سارية" : "منتهية الصلاحية"
        })
        .Concat(externalVisas.Select(ev => new VisaDetailDto
        {
            PassengerName = ev.PassengerName,
            PassportNumber = "N/A",
            VisaType = "تأشيرة خارجية",
            HasQrCode = false,
            Status = "سارية (خارجي)"
        }))
        .Take(50)
        .ToList();

        return dto;
    }

    public async Task<CashReportDto> GetCashReportAsync(DateTime? fromDate, DateTime? toDate)
    {
        var safeRepo = _unitOfWork.GetRepository<SafeTransaction>();
        var txs = await safeRepo.GetAllAsync();

        var filtered = txs.AsQueryable();
        if (fromDate.HasValue)
        {
            var fromOnly = DateOnly.FromDateTime(fromDate.Value);
            filtered = filtered.Where(t => t.TransactionDate >= fromOnly);
        }
        if (toDate.HasValue)
        {
            var toOnly = DateOnly.FromDateTime(toDate.Value);
            filtered = filtered.Where(t => t.TransactionDate <= toOnly);
        }

        var list = filtered.OrderByDescending(t => t.TransactionDate).ToList();

        var dto = new CashReportDto
        {
            CurrentSafeEgpBalance = txs.Where(t => t.Currency == Domain.Enums.Currency.EGP).Sum(t => t.TransactionType == Domain.Enums.TransactionType.Deposit ? t.Amount : -t.Amount),
            CurrentSafeSarBalance = txs.Where(t => t.Currency == Domain.Enums.Currency.SAR).Sum(t => t.TransactionType == Domain.Enums.TransactionType.Deposit ? t.Amount : -t.Amount),
            TotalEgpDeposits = list.Where(t => t.Currency == Domain.Enums.Currency.EGP && t.TransactionType == Domain.Enums.TransactionType.Deposit).Sum(t => t.Amount),
            TotalEgpWithdrawals = list.Where(t => t.Currency == Domain.Enums.Currency.EGP && t.TransactionType == Domain.Enums.TransactionType.Withdrawal).Sum(t => t.Amount),
            TotalSarDeposits = list.Where(t => t.Currency == Domain.Enums.Currency.SAR && t.TransactionType == Domain.Enums.TransactionType.Deposit).Sum(t => t.Amount),
            TotalSarWithdrawals = list.Where(t => t.Currency == Domain.Enums.Currency.SAR && t.TransactionType == Domain.Enums.TransactionType.Withdrawal).Sum(t => t.Amount)
        };

        dto.TransactionsList = list.Select(t => new CashTransactionDetailDto
        {
            Date = t.TransactionDate.ToDateTime(TimeOnly.MinValue),
            ReferenceNumber = t.AssociatedPartnerId?.ToString().Substring(0, 8) ?? "خزينة",
            Type = t.TransactionType == Domain.Enums.TransactionType.Deposit ? "إيداع" : "سحب / صرف",
            Amount = t.Amount,
            Currency = t.Currency.ToString(),
            Description = t.Description
        })
        .Take(50)
        .ToList();

        return dto;
    }

    public async Task<ExpenseReportDto> GetExpenseReportAsync(DateTime? fromDate, DateTime? toDate)
    {
        var lineRepo = _unitOfWork.GetRepository<JournalEntryLine>();
        var entryRepo = _unitOfWork.GetRepository<JournalEntry>();
        var accountRepo = _unitOfWork.GetRepository<Account>();

        var entries = await entryRepo.GetAllAsync();
        var lines = await lineRepo.GetAllAsync();
        var accounts = await accountRepo.GetAllAsync();

        // Filter lines belonging to entries in date range, and with Expense accounts (code starts with 5)
        var filteredEntries = entries.AsQueryable();
        if (fromDate.HasValue)
        {
            var fromOnly = DateOnly.FromDateTime(fromDate.Value);
            filteredEntries = filteredEntries.Where(e => e.EntryDate >= fromOnly);
        }
        if (toDate.HasValue)
        {
            var toOnly = DateOnly.FromDateTime(toDate.Value);
            filteredEntries = filteredEntries.Where(e => e.EntryDate <= toOnly);
        }

        var entryIds = filteredEntries.Select(e => e.Id).ToList();
        var expenseAccounts = accounts.Where(a => a.AccountCode.StartsWith("5")).ToList();
        var expenseAccountIds = expenseAccounts.Select(a => a.Id).ToList();

        var expenseLines = lines.Where(l => entryIds.Contains(l.JournalEntryId) && expenseAccountIds.Contains(l.AccountId)).ToList();

        decimal totalExpenses = expenseLines.Sum(l => l.Debit - l.Credit);

        var dto = new ExpenseReportDto
        {
            TotalExpenses = totalExpenses,
            ExpensesByCategory = expenseLines
                .GroupBy(l => l.AccountId)
                .Select(g =>
                {
                    var acc = expenseAccounts.FirstOrDefault(a => a.Id == g.Key);
                    var sum = g.Sum(l => l.Debit - l.Credit);
                    return new CategorySummaryDto
                    {
                        AccountCode = acc?.AccountCode ?? "",
                        CategoryName = acc?.Name ?? "مصروفات متنوعة",
                        Amount = sum,
                        Percentage = totalExpenses > 0 ? (double)(sum / totalExpenses * 100) : 0
                    };
                })
                .OrderByDescending(c => c.Amount)
                .ToList()
        };

        return dto;
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateTime? fromDate, DateTime? toDate)
    {
        var lineRepo = _unitOfWork.GetRepository<JournalEntryLine>();
        var entryRepo = _unitOfWork.GetRepository<JournalEntry>();
        var accountRepo = _unitOfWork.GetRepository<Account>();

        var entries = await entryRepo.GetAllAsync();
        var lines = await lineRepo.GetAllAsync();
        var accounts = await accountRepo.GetAllAsync();

        var filteredEntries = entries.AsQueryable();
        if (fromDate.HasValue)
        {
            var fromOnly = DateOnly.FromDateTime(fromDate.Value);
            filteredEntries = filteredEntries.Where(e => e.EntryDate >= fromOnly);
        }
        if (toDate.HasValue)
        {
            var toOnly = DateOnly.FromDateTime(toDate.Value);
            filteredEntries = filteredEntries.Where(e => e.EntryDate <= toOnly);
        }

        var entryIds = filteredEntries.Select(e => e.Id).ToList();
        var revAccounts = accounts.Where(a => a.AccountCode.StartsWith("4")).ToList();
        var revAccountIds = revAccounts.Select(a => a.Id).ToList();

        var revLines = lines.Where(l => entryIds.Contains(l.JournalEntryId) && revAccountIds.Contains(l.AccountId)).ToList();

        decimal totalRevenues = revLines.Sum(l => l.Credit - l.Debit);

        var dto = new RevenueReportDto
        {
            TotalRevenues = totalRevenues,
            RevenuesByCategory = revLines
                .GroupBy(l => l.AccountId)
                .Select(g =>
                {
                    var acc = revAccounts.FirstOrDefault(a => a.Id == g.Key);
                    var sum = g.Sum(l => l.Credit - l.Debit);
                    return new CategorySummaryDto
                    {
                        AccountCode = acc?.AccountCode ?? "",
                        CategoryName = acc?.Name ?? "إيرادات حجز",
                        Amount = sum,
                        Percentage = totalRevenues > 0 ? (double)(sum / totalRevenues * 100) : 0
                    };
                })
                .OrderByDescending(c => c.Amount)
                .ToList()
        };

        return dto;
    }

    public async Task<ProfitReportDto> GetProfitReportAsync(DateTime? fromDate, DateTime? toDate)
    {
        var revReport = await GetRevenueReportAsync(fromDate, toDate);
        var expReport = await GetExpenseReportAsync(fromDate, toDate);

        var dto = new ProfitReportDto
        {
            TotalRevenues = revReport.TotalRevenues,
            TotalExpenses = expReport.TotalExpenses,
            NetProfit = revReport.TotalRevenues - expReport.TotalExpenses,
            ProfitMarginPercentage = revReport.TotalRevenues > 0 
                ? (double)((revReport.TotalRevenues - expReport.TotalExpenses) / revReport.TotalRevenues * 100) 
                : 0
        };

        // Timeline: Group by month for past 6 months
        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.Today.AddMonths(-i);
            var start = new DateTime(date.Year, date.Month, 1);
            var end = start.AddMonths(1).AddSeconds(-1);

            var rMonth = await GetRevenueReportAsync(start, end);
            var eMonth = await GetExpenseReportAsync(start, end);

            dto.ProfitTimeline.Add(new MonthlyProfitDetailDto
            {
                MonthName = date.ToString("yyyy-MM"),
                Revenue = rMonth.TotalRevenues,
                Expense = eMonth.TotalExpenses,
                Profit = rMonth.TotalRevenues - eMonth.TotalExpenses
            });
        }

        return dto;
    }

    public async Task<IEnumerable<PartnerReportDto>> GetPartnerReportAsync()
    {
        var capitalRepo = _unitOfWork.GetRepository<PartnershipCapital>();
        var capitals = await capitalRepo.GetAllAsync();

        // Calculate total retained earnings from account 3201
        var lineRepo = _unitOfWork.GetRepository<JournalEntryLine>();
        var accountRepo = _unitOfWork.GetRepository<Account>();
        var accounts = await accountRepo.GetAllAsync();
        var lines = await lineRepo.GetAllAsync();

        var retainedAcc = accounts.FirstOrDefault(a => a.AccountCode == "3201");
        decimal totalRetained = 0;
        if (retainedAcc != null)
        {
            totalRetained = lines.Where(l => l.AccountId == retainedAcc.Id).Sum(l => l.Credit - l.Debit);
        }

        var totalContributions = capitals.Sum(c => c.AmountEgp + (c.AmountSar * c.HistoricalRate));

        return capitals.Select(c =>
        {
            decimal share = 0;
            if (c.ProfitShareRatio > 0)
            {
                share = totalRetained * c.ProfitShareRatio;
            }
            else
            {
                var val = c.AmountEgp + (c.AmountSar * c.HistoricalRate);
                share = totalContributions > 0 ? totalRetained * (val / totalContributions) : 0;
            }

            return new PartnerReportDto
            {
                ShareholderName = c.ShareholderName,
                AmountSar = c.AmountSar,
                AmountEgp = c.AmountEgp,
                ShareRatio = c.ShareRatio,
                ProfitShareRatio = c.ProfitShareRatio,
                EstimatedProfitQuota = share
            };
        }).ToList();
    }

    public async Task<IEnumerable<PeriodicReportDto>> GetPeriodicReportsAsync(string rangeType, int year)
    {
        var bookingRepo = _unitOfWork.GetRepository<Booking>();
        var bookings = await bookingRepo.GetAllAsync();

        var list = new List<PeriodicReportDto>();

        if (rangeType.Equals("monthly", StringComparison.OrdinalIgnoreCase))
        {
            // Monthly periods
            for (int m = 1; m <= 12; m++)
            {
                var start = new DateTime(year, m, 1);
                var end = start.AddMonths(1).AddSeconds(-1);

                var startOnly = DateOnly.FromDateTime(start);
                var endOnly = DateOnly.FromDateTime(end);

                var count = bookings.Count(b => b.TravelDate >= startOnly && b.TravelDate <= endOnly);
                var revenues = await GetRevenueReportAsync(start, end);
                var expenses = await GetExpenseReportAsync(start, end);

                list.Add(new PeriodicReportDto
                {
                    PeriodType = "شهرية",
                    PeriodLabel = start.ToString("yyyy-MM MMMM"),
                    BookingsCount = count,
                    Revenues = revenues.TotalRevenues,
                    Expenses = expenses.TotalExpenses,
                    NetProfit = revenues.TotalRevenues - expenses.TotalExpenses
                });
            }
        }
        else if (rangeType.Equals("annual", StringComparison.OrdinalIgnoreCase))
        {
            // Past 3 years
            for (int y = year - 2; y <= year; y++)
            {
                var start = new DateTime(y, 1, 1);
                var end = new DateTime(y, 12, 31, 23, 59, 59);

                var startOnly = DateOnly.FromDateTime(start);
                var endOnly = DateOnly.FromDateTime(end);

                var count = bookings.Count(b => b.TravelDate >= startOnly && b.TravelDate <= endOnly);
                var revenues = await GetRevenueReportAsync(start, end);
                var expenses = await GetExpenseReportAsync(start, end);

                list.Add(new PeriodicReportDto
                {
                    PeriodType = "سنوية",
                    PeriodLabel = $"السنة المالية {y}",
                    BookingsCount = count,
                    Revenues = revenues.TotalRevenues,
                    Expenses = expenses.TotalExpenses,
                    NetProfit = revenues.TotalRevenues - expenses.TotalExpenses
                });
            }
        }
        else
        {
            // Daily for past 15 days
            for (int d = 14; d >= 0; d--)
            {
                var day = DateTime.Today.AddDays(-d);
                var start = day.Date;
                var end = start.AddDays(1).AddSeconds(-1);

                var dayOnly = DateOnly.FromDateTime(day);

                var count = bookings.Count(b => b.TravelDate == dayOnly);
                var revenues = await GetRevenueReportAsync(start, end);
                var expenses = await GetExpenseReportAsync(start, end);

                list.Add(new PeriodicReportDto
                {
                    PeriodType = "يومية",
                    PeriodLabel = day.ToString("yyyy-MM-dd dddd"),
                    BookingsCount = count,
                    Revenues = revenues.TotalRevenues,
                    Expenses = expenses.TotalExpenses,
                    NetProfit = revenues.TotalRevenues - expenses.TotalExpenses
                });
            }
        }

        return list;
    }
}
