using System;
using Bms.Domain.Enums;

namespace Bms.Domain.Entities;

public class SafeTransaction : BaseEntity
{
    public Guid TenantId { get; set; }
    public DateOnly TransactionDate { get; set; }
    public TransactionType TransactionType { get; set; }
    public Currency Currency { get; set; }
    public decimal Amount { get; set; }
    public decimal ExchangeRate { get; set; } = 1.0m;
    public string DepositorOrWithdrawerName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    
    public Guid? AssociatedPartnerId { get; set; }
    public virtual Partner? AssociatedPartner { get; set; }

    public string Description { get; set; } = string.Empty;
}
