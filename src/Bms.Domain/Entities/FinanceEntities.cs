using System;
using System.Collections.Generic;

namespace Bms.Domain.Entities;

public class Account : BaseEntity
{
    public Guid TenantId { get; set; }
    public string AccountCode { get; set; } = string.Empty; // e.g. "110101"
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Asset, Liability, Equity, Revenue, Expense
    public bool IsActive { get; set; } = true;

    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}

public class JournalEntry : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateOnly EntryDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty; // e.g. JE-0001
    public string Narration { get; set; } = string.Empty;
    public bool IsPosted { get; set; } = false;

    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}

public class JournalEntryLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid JournalEntryId { get; set; }
    public virtual JournalEntry JournalEntry { get; set; } = null!;

    public Guid AccountId { get; set; }
    public virtual Account Account { get; set; } = null!;

    public decimal Debit { get; set; } = 0.00m;
    public decimal Credit { get; set; } = 0.00m;

    public Guid? CostCenterId { get; set; }
    public virtual CostCenter? CostCenter { get; set; }
}

public class CostCenter : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty; // e.g. "CC-CAI"
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class Expense : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public Guid AccountId { get; set; } // Cash safe / Bank account cash credit
    public virtual Account Account { get; set; } = null!;

    public Guid? CostCenterId { get; set; }
    public virtual CostCenter? CostCenter { get; set; }
}

public class Revenue : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateOnly RevenueDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public Guid AccountId { get; set; } // Cash safe / Bank account cash debit
    public virtual Account Account { get; set; } = null!;

    public Guid? CostCenterId { get; set; }
    public virtual CostCenter? CostCenter { get; set; }
}
