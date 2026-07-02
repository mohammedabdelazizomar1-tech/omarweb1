using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bms.Application.DTOs;

namespace Bms.Application.Interfaces;

public interface IBookingService
{
    // Passenger Profiles
    Task<IEnumerable<PassengerDto>> GetAllPassengersAsync();
    Task<PassengerDto?> GetPassengerByIdAsync(Guid id);
    Task<PassengerDto> CreatePassengerAsync(CreatePassengerDto model);

    // Bookings
    Task<IEnumerable<BookingDto>> GetAllBookingsAsync(string? partnerUsername = null);
    Task<BookingDto?> GetBookingByIdAsync(Guid id);
    Task<BookingDto> CreateBookingAsync(CreateBookingDto model);
    Task<BookingDto> UpdateBookingAsync(Guid id, CreateBookingDto model);
    Task DeleteBookingAsync(Guid id);
    
    // Partners Lookup
    Task<IEnumerable<PartnerQuotaMetric>> GetPartnersLookupAsync();
}
