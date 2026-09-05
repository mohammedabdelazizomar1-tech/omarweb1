using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessManagement.Application.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Asset, Liability, Equity, Revenue, Expense
    public bool IsActive { get; set; }
    public decimal CurrentBalance { get; set; }
}

public class JournalEntryDto
{
    public Guid Id { get; set; }
    public DateOnly EntryDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public bool IsPosted { get; set; }
    public List<JournalEntryLineDto> Lines { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class JournalEntryLineDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class CreateJournalEntryDto
{
    [Required(ErrorMessage = "تاريخ القيد مطلوب.")]
    public DateOnly EntryDate { get; set; }

    [Required(ErrorMessage = "وصف القيد مطلوب.")]
    public string Narration { get; set; } = string.Empty;

    public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
}

public class CreateJournalEntryLineDto
{
    [Required]
    public Guid AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class LedgerDto
{
    public AccountDto Account { get; set; } = null!;
    public List<LedgerLineDto> Transactions { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class LedgerLineDto
{
    public Guid JournalEntryId { get; set; }
    public DateOnly EntryDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}

public class IncomeStatementDto
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public List<AccountDto> RevenueAccounts { get; set; } = new();
    public List<AccountDto> ExpenseAccounts { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome => TotalRevenue - TotalExpenses;
}

public class BalanceSheetDto
{
    public DateOnly AsOfDate { get; set; }
    public List<AccountDto> AssetAccounts { get; set; } = new();
    public List<AccountDto> LiabilityAccounts { get; set; } = new();
    public List<AccountDto> EquityAccounts { get; set; } = new();
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;
}

public class DistributeProfitDto
{
    [Required(ErrorMessage = "المبلغ الإجمالي للتوزيع مطلوب.")]
    [Range(1, 10000000, ErrorMessage = "المبلغ للتوزيع يجب أن يكون أكبر من الصفر.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "العملة مطلوبة.")]
    public string Currency { get; set; } = "EGP"; // EGP, SAR

    [Required(ErrorMessage = "وصف القيد مطلوب.")]
    public string Description { get; set; } = string.Empty;
}

public class MonthlyClosingDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public bool IsClosed { get; set; }
    public DateTime ClosedAt { get; set; }
    public string ClosedBy { get; set; } = string.Empty;
}
