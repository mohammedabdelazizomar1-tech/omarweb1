using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;

namespace BusinessManagement.Application.Services;

public class AccountingService : IAccountingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountingService> _logger;

    public AccountingService(IUnitOfWork unitOfWork, ILogger<AccountingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<AccountDto>> GetChartOfAccountsAsync()
    {
        var accRepo = _unitOfWork.GetRepository<Account>();
        var lineRepo = _unitOfWork.GetRepository<JournalEntryLine>();

        var accounts = await accRepo.GetAllAsync();
        var lines = await lineRepo.GetAllAsync();
        var entryRepo = _unitOfWork.GetRepository<JournalEntry>();
        var postedEntryIds = (await entryRepo.GetAllAsync())
            .Where(e => e.IsPosted)
            .Select(e => e.Id)
            .ToHashSet();

        // Calculate balances only from POSTED journal entries
        var postedLines = lines.Where(l => postedEntryIds.Contains(l.JournalEntryId)).ToList();

        var result = new List<AccountDto>();
        foreach (var acc in accounts)
        {
            var accLines = postedLines.Where(l => l.AccountId == acc.Id).ToList();
            decimal debitSum = accLines.Sum(l => l.Debit);
            decimal creditSum = accLines.Sum(l => l.Credit);

            decimal balance = 0;
            // Asset & Expense: Debit - Credit
            if (acc.Type == "Asset" || acc.Type == "Expense")
            {
                balance = debitSum - creditSum;
            }
            // Liability, Equity, Revenue: Credit - Debit
            else
            {
                balance = creditSum - debitSum;
            }

            result.Add(new AccountDto
            {
                Id = acc.Id,
                AccountCode = acc.AccountCode,
                Name = acc.Name,
                Type = acc.Type,
                IsActive = acc.IsActive,
                CurrentBalance = balance
            });
        }

        return result.OrderBy(a => a.AccountCode);
    }

    public async Task<IEnumerable<JournalEntryDto>> GetJournalEntriesAsync()
    {
        var entryRepo = _unitOfWork.GetRepository<JournalEntry>();
        var entries = await entryRepo.GetAllAsync();

        return entries
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.CreatedAt)
            .Select(e => new JournalEntryDto
            {
                Id = e.Id,
                EntryDate = e.EntryDate,
                ReferenceNumber = e.ReferenceNumber,
                Narration = e.Narration,
                IsPosted = e.IsPosted,
                CreatedAt = e.CreatedAt,
                TotalDebit = e.JournalEntryLines.Sum(l => l.Debit),
                TotalCredit = e.JournalEntryLines.Sum(l => l.Credit),
                Lines = e.JournalEntryLines.Select(l => new JournalEntryLineDto
                {
                    Id = l.Id,
                    AccountId = l.AccountId,
                    AccountCode = l.Account.AccountCode,
                    AccountName = l.Account.Name,
                    Debit = l.Debit,
                    Credit = l.Credit
                }).ToList()
            }).ToList();
    }

    public async Task<JournalEntryDto?> GetJournalEntryByIdAsync(Guid id)
    {
        var entryRepo = _unitOfWork.GetRepository<JournalEntry>();
        var e = await entryRepo.GetByIdAsync(id);
        if (e == null) return null;

        return new JournalEntryDto
        {
            Id = e.Id,
            EntryDate = e.EntryDate,
            ReferenceNumber = e.ReferenceNumber,
            Narration = e.Narration,
            IsPosted = e.IsPosted,
            CreatedAt = e.CreatedAt,
            TotalDebit = e.JournalEntryLines.Sum(l => l.Debit),
            TotalCredit = e.JournalEntryLines.Sum(l => l.Credit),
            Lines = e.JournalEntryLines.Select(l => new JournalEntryLineDto
            {
                Id = l.Id,
                AccountId = l.AccountId,
                AccountCode = l.Account.AccountCode,
                AccountName = l.Account.Name,
                Debit = l.Debit,
                Credit = l.Credit
            }).ToList()
        };
    }

    public async Task<JournalEntryDto> CreateJournalEntryAsync(CreateJournalEntryDto dto, string user)
    {
        if (await IsMonthClosedAsync(dto.EntryDate))
        {
            throw new InvalidOperationException("عذراً، هذا الشهر مغلق محاسبياً ولا يمكن إضافة أو تعديل قيود فيه.");
        }

        decimal totalDebit = dto.Lines.Sum(l => l.Debit);
        decimal totalCredit = dto.Lines.Sum(l => l.Credit);

        if (totalDebit != totalCredit)
        {
            throw new InvalidOperationException("القيد غير متزن! إجمالي المدين يجب أن يساوي إجمالي الدائن.");
        }

        var entryRepo = _unitOfWork.GetRepository<JournalEntry>();
        
        // Generate reference number JE-YYYYMM-XXXX
        var datePrefix = $"JE-{dto.EntryDate:yyyyMM}";
        var existingCount = (await entryRepo.GetAllAsync())
            .Count(e => e.ReferenceNumber.StartsWith(datePrefix));
        var referenceNumber = $"{datePrefix}-{(existingCount + 1):D4}";

        var entry = new JournalEntry
        {
            EntryDate = dto.EntryDate,
            ReferenceNumber = referenceNumber,
            Narration = dto.Narration,
            IsPosted = false,
            CreatedBy = user,
            UpdatedBy = user
        };

        foreach (var line in dto.Lines)
        {
            entry.JournalEntryLines.Add(new JournalEntryLine
            {
                AccountId = line.AccountId,
                Debit = line.Debit,
                Credit = line.Credit
            });
        }

        await entryRepo.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        return new JournalEntryDto
        {
            Id = entry.Id,
            EntryDate = entry.EntryDate,
            ReferenceNumber = entry.ReferenceNumber,
            Narration = entry.Narration,
            IsPosted = entry.IsPosted
        };
    }

    public async Task<bool> PostJournalEntryAsync(Guid id)
    {
        var repo = _unitOfWork.GetRepository<JournalEntry>();
        var entry = await repo.GetByIdAsync(id);
        if (entry == null || entry.IsPosted) return false;

        if (await IsMonthClosedAsync(entry.EntryDate))
        {
            throw new InvalidOperationException("عذراً، لا يمكن ترحيل قيود لشهر مغلق محاسبياً.");
        }

        entry.IsPosted = true;
        entry.UpdatedAt = DateTime.UtcNow;
        repo.Update(entry);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteJournalEntryAsync(Guid id)
    {
        var repo = _unitOfWork.GetRepository<JournalEntry>();
        var entry = await repo.GetByIdAsync(id);
        if (entry == null) return false;

        if (await IsMonthClosedAsync(entry.EntryDate))
        {
            throw new InvalidOperationException("عذراً، هذا الشهر مغلق محاسبياً ولا يمكن تعديل أو حذف أي قيود فيه.");
        }

        repo.Delete(entry);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<LedgerDto> GetGeneralLedgerAsync(Guid accountId, DateOnly? fromDate, DateOnly? toDate)
    {
        var accRepo = _unitOfWork.GetRepository<Account>();
        var acc = await accRepo.GetByIdAsync(accountId);
        if (acc == null) throw new KeyNotFoundException("Account not found");

        var lineRepo = _unitOfWork.GetRepository<JournalEntryLine>();
        var allLines = await lineRepo.GetAllAsync();

        // Filter only posted lines for this account
        var postedLines = allLines
            .Where(l => l.AccountId == accountId && l.JournalEntry.IsPosted)
            .OrderBy(l => l.JournalEntry.EntryDate)
            .ThenBy(l => l.JournalEntry.CreatedAt)
            .ToList();

        // 1. Calculate opening balance (balances before fromDate)
        decimal openingBalance = 0;
        decimal openingDebit = 0;
        decimal openingCredit = 0;

        if (fromDate.HasValue)
        {
            var beforeLines = postedLines.Where(l => l.JournalEntry.EntryDate < fromDate.Value).ToList();
            openingDebit = beforeLines.Sum(l => l.Debit);
            openingCredit = beforeLines.Sum(l => l.Credit);

            if (acc.Type == "Asset" || acc.Type == "Expense")
            {
                openingBalance = openingDebit - openingCredit;
            }
            else
            {
                openingBalance = openingCredit - openingDebit;
            }
        }

        // 2. Filter transactions in date range
        var activeLines = postedLines;
        if (fromDate.HasValue)
        {
            activeLines = activeLines.Where(l => l.JournalEntry.EntryDate >= fromDate.Value).ToList();
        }
        if (toDate.HasValue)
        {
            activeLines = activeLines.Where(l => l.JournalEntry.EntryDate <= toDate.Value).ToList();
        }

        // 3. Compile ledger lines with running balance
        var transactions = new List<LedgerLineDto>();
        decimal currentRunning = openingBalance;

        foreach (var l in activeLines)
        {
            if (acc.Type == "Asset" || acc.Type == "Expense")
            {
                currentRunning += (l.Debit - l.Credit);
            }
            else
            {
                currentRunning += (l.Credit - l.Debit);
            }

            transactions.Add(new LedgerLineDto
            {
                JournalEntryId = l.JournalEntryId,
                EntryDate = l.JournalEntry.EntryDate,
                ReferenceNumber = l.JournalEntry.ReferenceNumber,
                Narration = l.JournalEntry.Narration,
                Debit = l.Debit,
                Credit = l.Credit,
                RunningBalance = currentRunning
            });
        }

        return new LedgerDto
        {
            Account = new AccountDto
            {
                Id = acc.Id,
                AccountCode = acc.AccountCode,
                Name = acc.Name,
                Type = acc.Type
            },
            Transactions = transactions,
            OpeningBalance = openingBalance,
            ClosingBalance = currentRunning,
            TotalDebit = activeLines.Sum(l => l.Debit),
            TotalCredit = activeLines.Sum(l => l.Credit)
        };
    }

    public async Task<IncomeStatementDto> GetIncomeStatementAsync(DateOnly fromDate, DateOnly toDate)
    {
        var accounts = await GetChartOfAccountsAsync();

        var revenueAccounts = accounts.Where(a => a.Type == "Revenue").ToList();
        var expenseAccounts = accounts.Where(a => a.Type == "Expense").ToList();

        return new IncomeStatementDto
        {
            StartDate = fromDate,
            EndDate = toDate,
            RevenueAccounts = revenueAccounts,
            ExpenseAccounts = expenseAccounts,
            TotalRevenue = revenueAccounts.Sum(a => a.CurrentBalance),
            TotalExpenses = expenseAccounts.Sum(a => a.CurrentBalance)
        };
    }

    public async Task<BalanceSheetDto> GetBalanceSheetAsync(DateOnly asOfDate)
    {
        var accounts = await GetChartOfAccountsAsync();

        var assetAccounts = accounts.Where(a => a.Type == "Asset").ToList();
        var liabilityAccounts = accounts.Where(a => a.Type == "Liability").ToList();
        var equityAccounts = accounts.Where(a => a.Type == "Equity").ToList();

        // Calculate Net Profit up to asOfDate to add to Retained Earnings
        var totalRev = accounts.Where(a => a.Type == "Revenue").Sum(a => a.CurrentBalance);
        var totalExp = accounts.Where(a => a.Type == "Expense").Sum(a => a.CurrentBalance);
        var netProfit = totalRev - totalExp;

        // Add current net profit to Retained Earnings in presentation
        var retainedEarningsAcc = equityAccounts.FirstOrDefault(a => a.AccountCode == "3201");
        if (retainedEarningsAcc != null)
        {
            retainedEarningsAcc.CurrentBalance += netProfit;
        }

        return new BalanceSheetDto
        {
            AsOfDate = asOfDate,
            AssetAccounts = assetAccounts,
            LiabilityAccounts = liabilityAccounts,
            EquityAccounts = equityAccounts,
            TotalAssets = assetAccounts.Sum(a => a.CurrentBalance),
            TotalLiabilities = liabilityAccounts.Sum(a => a.CurrentBalance),
            TotalEquity = equityAccounts.Sum(a => a.CurrentBalance)
        };
    }

    public async Task<bool> DistributePartnersProfitAsync(DistributeProfitDto dto, string user)
    {
        // Determine ownership percentage by partner capital share
        var capitalRepo = _unitOfWork.GetRepository<PartnershipCapital>();
        var capitalRecords = await capitalRepo.GetAllAsync();

        if (!capitalRecords.Any())
        {
            throw new InvalidOperationException("لا يمكن توزيع الأرباح لعدم وجود رأس مال مسجل في الشراكة.");
        }

        // Calculate total capital contributions pool
        var totalContributions = capitalRecords.Sum(c => c.AmountEgp + (c.AmountSar * c.HistoricalRate));
        if (totalContributions <= 0)
        {
            throw new InvalidOperationException("إجمالي قيمة رأس مال الشركاء تساوي صفر، يرجى تمويل الحساب أولاً.");
        }

        // 1. Prepare journal entry
        var accRepo = _unitOfWork.GetRepository<Account>();
        var accounts = await accRepo.GetAllAsync();

        var accRetained = accounts.FirstOrDefault(a => a.AccountCode == "3201"); // Equity Debit
        var accPayable = accounts.FirstOrDefault(a => a.AccountCode == "2101");  // Liability Credit

        if (accRetained == null || accPayable == null)
        {
            throw new InvalidOperationException("أكواد الحسابات المالية (3201 أو 2101) غير معرفة في شجرة الحسابات.");
        }

        var entryDto = new CreateJournalEntryDto
        {
            EntryDate = DateOnly.FromDateTime(DateTime.Today),
            Narration = $"توزيع الأرباح السنوية/الشهرية للشركاء: {dto.Description}",
            Lines = new List<CreateJournalEntryLineDto>()
        };

        // Debit Retained Earnings
        entryDto.Lines.Add(new CreateJournalEntryLineDto
        {
            AccountId = accRetained.Id,
            Debit = dto.Amount,
            Credit = 0
        });

        // Credit each Shareholder's Account Payable
        foreach (var cap in capitalRecords)
        {
            decimal partnerShare = 0;
            if (cap.ProfitShareRatio > 0)
            {
                partnerShare = dto.Amount * cap.ProfitShareRatio;
            }
            else
            {
                var capValue = cap.AmountEgp + (cap.AmountSar * cap.HistoricalRate);
                var ownershipPercentage = capValue / totalContributions;
                partnerShare = dto.Amount * ownershipPercentage;
            }

            entryDto.Lines.Add(new CreateJournalEntryLineDto
            {
                AccountId = accPayable.Id,
                Debit = 0,
                Credit = partnerShare
            });
        }

        // Save and Post the entry
        var je = await CreateJournalEntryAsync(entryDto, user);
        await PostJournalEntryAsync(je.Id);
        return true;
    }

    public async Task<IEnumerable<MonthlyClosingDto>> GetMonthlyClosingsAsync()
    {
        var repo = _unitOfWork.GetRepository<MonthlyClosing>();
        var closings = await repo.GetAllAsync();

        return closings
            .OrderByDescending(c => c.Year)
            .ThenByDescending(c => c.Month)
            .Select(c => new MonthlyClosingDto
            {
                Id = c.Id,
                Year = c.Year,
                Month = c.Month,
                IsClosed = c.IsClosed,
                ClosedAt = c.ClosedAt,
                ClosedBy = c.ClosedBy
            }).ToList();
    }

    public async Task<bool> CloseMonthAsync(int year, int month, string user)
    {
        var repo = _unitOfWork.GetRepository<MonthlyClosing>();
        var existing = (await repo.FindAsync(c => c.Year == year && c.Month == month)).FirstOrDefault();

        if (existing != null)
        {
            existing.IsClosed = true;
            existing.ClosedAt = DateTime.UtcNow;
            existing.ClosedBy = user;
            repo.Update(existing);
        }
        else
        {
            var closing = new MonthlyClosing
            {
                Year = year,
                Month = month,
                IsClosed = true,
                ClosedAt = DateTime.UtcNow,
                ClosedBy = user,
                CreatedBy = user,
                UpdatedBy = user
            };
            await repo.AddAsync(closing);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReopenMonthAsync(int year, int month)
    {
        var repo = _unitOfWork.GetRepository<MonthlyClosing>();
        var existing = (await repo.FindAsync(c => c.Year == year && c.Month == month)).FirstOrDefault();

        if (existing == null) return false;

        existing.IsClosed = false;
        existing.UpdatedAt = DateTime.UtcNow;
        repo.Update(existing);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsMonthClosedAsync(DateOnly date)
    {
        var repo = _unitOfWork.GetRepository<MonthlyClosing>();
        var closing = (await repo.FindAsync(c => c.Year == date.Year && c.Month == date.Month)).FirstOrDefault();
        return closing?.IsClosed ?? false;
    }
}
