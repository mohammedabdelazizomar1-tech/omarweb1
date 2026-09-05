using System;

namespace BusinessManagement.Application.DTOs;

public class BookingPaymentDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public string PaymentMethod { get; set; } = "Cash";
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateOnly PaymentDate { get; set; }
}
