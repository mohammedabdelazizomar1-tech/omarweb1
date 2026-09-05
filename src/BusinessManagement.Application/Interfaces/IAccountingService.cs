using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;

namespace BusinessManagement.Application.Interfaces;

public interface IAccountingService
{
    // Chart of Accounts
    Task<IEnumerable<AccountDto>> GetChartOfAccountsAsync();

    // Journal Entries
    Task<IEnumerable<JournalEntryDto>> GetJournalEntriesAsync();
    Task<JournalEntryDto?> GetJournalEntryByIdAsync(Guid id);
    Task<JournalEntryDto> CreateJournalEntryAsync(CreateJournalEntryDto entryDto, string user);
    Task<bool> PostJournalEntryAsync(Guid id);
    Task<bool> DeleteJournalEntryAsync(Guid id);

    // Ledger
    Task<LedgerDto> GetGeneralLedgerAsync(Guid accountId, DateOnly? fromDate, DateOnly? toDate);

    // Reports
    Task<IncomeStatementDto> GetIncomeStatementAsync(DateOnly fromDate, DateOnly toDate);
    Task<BalanceSheetDto> GetBalanceSheetAsync(DateOnly asOfDate);

    // Profit Distribution
    Task<bool> DistributePartnersProfitAsync(DistributeProfitDto dto, string user);

    // Monthly Closing
    Task<IEnumerable<MonthlyClosingDto>> GetMonthlyClosingsAsync();
    Task<bool> CloseMonthAsync(int year, int month, string user);
    Task<bool> ReopenMonthAsync(int year, int month);
    Task<bool> IsMonthClosedAsync(DateOnly date);
}
