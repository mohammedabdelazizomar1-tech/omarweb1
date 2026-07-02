using System;
using System.ComponentModel.DataAnnotations;
using Bms.Domain.Enums;

namespace Bms.Application.DTOs;

public class SafeTransactionDto
{
    public Guid Id { get; set; }
    public DateOnly TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public Guid? AssociatedPartnerId { get; set; }
    public string AssociatedPartnerName { get; set; } = string.Empty;
    public string DepositorOrWithdrawerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateSafeTransactionDto
{
    [Required(ErrorMessage = "Transaction Date is required.")]
    public DateOnly TransactionDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 10000000, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Transaction Type is required.")]
    public TransactionType TransactionType { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    public Currency Currency { get; set; }

    [Required(ErrorMessage = "Exchange Rate is required.")]
    [Range(0.0001, 1000, ErrorMessage = "Exchange Rate must be greater than zero.")]
    public decimal ExchangeRate { get; set; } = 1.0m;

    public Guid? AssociatedPartnerId { get; set; }

    [Required(ErrorMessage = "Depositor or Withdrawer name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string DepositorOrWithdrawerName { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
    public string Description { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Bank Name cannot exceed 100 characters.")]
    public string BankName { get; set; } = string.Empty;
}
