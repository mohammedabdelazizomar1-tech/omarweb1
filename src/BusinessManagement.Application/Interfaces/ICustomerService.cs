using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;

namespace BusinessManagement.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerListDto> GetCustomersPagedAsync(string? search, string? gender, string? nationality, string? sortBy, string? sortOrder, int page, int pageSize, Guid? partnerId = null, string? groupNumber = null);
    Task<CustomerDetailsDto?> GetCustomerByIdAsync(Guid id);
    Task<CustomerDetailsDto> CreateCustomerAsync(CustomerDetailsDto customerDto);
    Task<bool> UpdateCustomerAsync(CustomerDetailsDto customerDto);
    Task<bool> DeleteCustomerAsync(Guid id);
    Task<CustomerImportResultDto> ImportCustomersFromExcelAsync(IEnumerable<CustomerDetailsDto> customers, string? groupNumber = null);

    // Notes
    Task<IEnumerable<PassengerNoteDto>> GetNotesForCustomerAsync(Guid customerId);
    Task<PassengerNoteDto> AddNoteToCustomerAsync(Guid customerId, string noteText, string author);
    Task<bool> DeleteNoteAsync(Guid noteId);

    // Attachments
    Task<IEnumerable<AttachmentDto>> GetAttachmentsForCustomerAsync(Guid customerId);
    Task<AttachmentDto> AddAttachmentToCustomerAsync(Guid customerId, string fileName, string filePath, string contentType, long sizeBytes);
    Task<bool> DeleteAttachmentAsync(Guid attachmentId);

    // Customer History
    Task<IEnumerable<CustomerBookingHistoryDto>> GetCustomerBookingHistoryAsync(Guid customerId);
}
