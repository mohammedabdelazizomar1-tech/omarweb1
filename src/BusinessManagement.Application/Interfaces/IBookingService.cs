using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;

namespace BusinessManagement.Application.Interfaces;

public interface IBookingService
{
    // Passenger Profiles
    Task<IEnumerable<PassengerDto>> GetAllPassengersAsync();
    Task<PassengerDto?> GetPassengerByIdAsync(Guid id);
    Task<PassengerDto> CreatePassengerAsync(CreatePassengerDto model);
    Task<PassengerImportResultDto> ImportPassengersAsync(IEnumerable<CreatePassengerDto> passengers);
    Task DeletePassengerAsync(Guid id);

    // Bookings
    Task<IEnumerable<BookingDto>> GetAllBookingsAsync(string? partnerUsername = null);
    Task<BookingDto?> GetBookingByIdAsync(Guid id);
    Task<BookingDto> CreateBookingAsync(CreateBookingDto model);
    Task<BookingDto> UpdateBookingAsync(Guid id, CreateBookingDto model);
    Task DeleteBookingAsync(Guid id);
    
    // Partners Lookup
    Task<IEnumerable<PartnerQuotaMetric>> GetPartnersLookupAsync();

    // Payments Management
    Task<IEnumerable<BookingPaymentDto>> GetBookingPaymentsAsync(Guid bookingId);
    Task AddBookingPaymentAsync(Guid bookingId, decimal amount, string paymentMethod, string receiptNumber, DateOnly paymentDate);
    Task DeleteBookingPaymentAsync(Guid paymentId);

    // Full business Excel import
    Task<ExcelImportResultDto> ImportFullBusinessExcelAsync(System.IO.Stream fileStream);
}





