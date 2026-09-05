using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Domain;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;

namespace BusinessManagement.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<CustomerService> _logger;

    public class CustomerServiceBuilder { } // Empty helper for potential extension if needed later, keeping simple

    public CustomerService(
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ICurrentUserProvider currentUserProvider,
        ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<CustomerListDto> GetCustomersPagedAsync(string? search, string? gender, string? nationality, string? sortBy, string? sortOrder, int page, int pageSize, Guid? partnerId = null, string? groupNumber = null)
    {
        _logger.LogInformation("Querying paged customers. Search: '{Search}', PartnerId: '{PartnerId}', GroupNumber: '{GroupNumber}', Page: {Page}", search, partnerId, groupNumber, page);
        
        var repo = _unitOfWork.GetRepository<Passenger>();
        var query = await repo.GetAllAsync();

        if (partnerId.HasValue)
        {
            query = query.Where(p => p.PartnerId == partnerId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(groupNumber))
        {
            if (groupNumber == "-")
            {
                query = query.Where(p => string.IsNullOrWhiteSpace(p.GroupNumber)).ToList();
            }
            else
            {
                string cleanGroup = groupNumber.Trim().ToLower();
                query = query.Where(p => p.GroupNumber != null && p.GroupNumber.ToLower().Contains(cleanGroup)).ToList();
            }
        }

        // 1. Filtering
        if (!string.IsNullOrWhiteSpace(search))
        {
            string cleanSearch = search.Trim().ToLower();
            string hashedSearch = BusinessManagement.Shared.Security.EncryptionHelper.HashSearchableField(cleanSearch);
            query = query.Where(p => 
                p.FullName.ToLower().Contains(cleanSearch) || 
                p.PassportNumber.ToLower().Contains(cleanSearch) || 
                p.PassportNumberHash == hashedSearch ||
                p.NationalIdHash == hashedSearch ||
                p.NationalId.Contains(cleanSearch) || 
                p.PhoneNumber.Contains(cleanSearch) || 
                (p.GroupNumber != null && p.GroupNumber.ToLower().Contains(cleanSearch)) ||
                p.Email.ToLower().Contains(cleanSearch)
            ).ToList();
        }

        if (!string.IsNullOrWhiteSpace(gender))
        {
            query = query.Where(p => p.Gender.Equals(gender, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(nationality))
        {
            query = query.Where(p => p.Nationality.Equals(nationality, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // 2. Sorting
        var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        switch (sortBy?.ToLower())
        {
            case "name":
                query = isDesc ? query.OrderByDescending(p => p.FullName).ToList() : query.OrderBy(p => p.FullName).ToList();
                break;
            case "passport":
                query = isDesc ? query.OrderByDescending(p => p.PassportNumber).ToList() : query.OrderBy(p => p.PassportNumber).ToList();
                break;
            case "nationality":
                query = isDesc ? query.OrderByDescending(p => p.Nationality).ToList() : query.OrderBy(p => p.Nationality).ToList();
                break;
            case "gender":
                query = isDesc ? query.OrderByDescending(p => p.Gender).ToList() : query.OrderBy(p => p.Gender).ToList();
                break;
            case "createdat":
            default:
                query = isDesc ? query.OrderByDescending(p => p.CreatedAt).ToList() : query.OrderBy(p => p.CreatedAt).ToList();
                break;
        }

        int totalCount = query.Count();

        // 3. Paging
        var partnersMap = (await _unitOfWork.GetRepository<Partner>().GetAllAsync())
            .ToDictionary(pt => pt.Id, pt => pt.Name);

        var customers = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new CustomerDetailsDto
            {
                Id = p.Id,
                FullName = p.FullName,
                PassportNumber = p.PassportNumber,
                PassportExpiry = p.PassportExpiry,
                NationalId = p.NationalId,
                DateOfBirth = p.DateOfBirth,
                Nationality = p.Nationality,
                Gender = p.Gender,
                PhoneNumber = p.PhoneNumber,
                Email = p.Email,
                Address = p.Address,
                EmergencyContactName = p.EmergencyContactName,
                EmergencyContactPhone = p.EmergencyContactPhone,
                PartnerId = p.PartnerId,
                PartnerName = p.PartnerId.HasValue && partnersMap.TryGetValue(p.PartnerId.Value, out var pName) ? pName : "عميل مباشر",
                GroupNumber = p.GroupNumber,
                CreatedAt = p.CreatedAt
            })
            .ToList();

        return new CustomerListDto
        {
            Customers = customers,
            TotalCount = totalCount,
            PageIndex = page,
            PageSize = pageSize
        };
    }

    public async Task<CustomerDetailsDto?> GetCustomerByIdAsync(Guid id)
    {
        var repo = _unitOfWork.GetRepository<Passenger>();
        var p = await repo.GetByIdAsync(id);
        if (p == null) return null;

        var partner = p.PartnerId.HasValue 
            ? await _unitOfWork.GetRepository<Partner>().GetByIdAsync(p.PartnerId.Value)
            : null;

        return new CustomerDetailsDto
        {
            Id = p.Id,
            FullName = p.FullName,
            PassportNumber = p.PassportNumber,
            PassportExpiry = p.PassportExpiry,
            NationalId = p.NationalId,
            DateOfBirth = p.DateOfBirth,
            Nationality = p.Nationality,
            Gender = p.Gender,
            PhoneNumber = p.PhoneNumber,
            Email = p.Email,
            Address = p.Address,
            EmergencyContactName = p.EmergencyContactName,
            EmergencyContactPhone = p.EmergencyContactPhone,
            PartnerId = p.PartnerId,
            PartnerName = partner?.Name ?? "عميل مباشر",
            GroupNumber = p.GroupNumber,
            CreatedAt = p.CreatedAt
        };
    }

    public async Task<CustomerDetailsDto> CreateCustomerAsync(CustomerDetailsDto dto)
    {
        var tenantId = _currentUserProvider.GetCurrentTenantId();
        if (tenantId == Guid.Empty)
        {
            var defaultTenant = (await _unitOfWork.GetRepository<Tenant>().GetAllAsync()).FirstOrDefault();
            tenantId = defaultTenant?.Id ?? Guid.Empty;
        }

        var repo = _unitOfWork.GetRepository<Passenger>();
        var customer = new Passenger
        {
            TenantId = tenantId,
            FullName = dto.FullName,
            PassportNumber = dto.PassportNumber,
            PassportExpiry = dto.PassportExpiry ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(10)),
            NationalId = dto.NationalId ?? string.Empty,
            DateOfBirth = dto.DateOfBirth,
            Nationality = dto.Nationality ?? "Egyptian",
            Gender = dto.Gender ?? string.Empty,
            PhoneNumber = dto.PhoneNumber ?? string.Empty,
            Email = dto.Email ?? string.Empty,
            Address = dto.Address ?? string.Empty,
            EmergencyContactName = dto.EmergencyContactName ?? string.Empty,
            EmergencyContactPhone = dto.EmergencyContactPhone ?? string.Empty,
            PartnerId = dto.PartnerId,
            GroupNumber = dto.GroupNumber,
            CreatedBy = "system",
            UpdatedBy = "system"
        };

        await repo.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        dto.Id = customer.Id;
        dto.CreatedAt = customer.CreatedAt;
        return dto;
    }

    public async Task<bool> UpdateCustomerAsync(CustomerDetailsDto dto)
    {
        var repo = _unitOfWork.GetRepository<Passenger>();
        var customer = await repo.GetByIdAsync(dto.Id);
        if (customer == null) return false;

        customer.FullName = dto.FullName;
        customer.PassportNumber = dto.PassportNumber;
        customer.PassportExpiry = dto.PassportExpiry ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(10));
        customer.NationalId = dto.NationalId ?? string.Empty;
        customer.DateOfBirth = dto.DateOfBirth;
        customer.Nationality = dto.Nationality ?? "Egyptian";
        customer.Gender = dto.Gender ?? string.Empty;
        customer.PhoneNumber = dto.PhoneNumber ?? string.Empty;
        customer.Email = dto.Email ?? string.Empty;
        customer.Address = dto.Address ?? string.Empty;
        customer.EmergencyContactName = dto.EmergencyContactName ?? string.Empty;
        customer.EmergencyContactPhone = dto.EmergencyContactPhone ?? string.Empty;
        customer.PartnerId = dto.PartnerId;
        customer.GroupNumber = dto.GroupNumber;
        customer.UpdatedAt = DateTime.UtcNow;

        repo.Update(customer);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCustomerAsync(Guid id)
    {
        var repo = _unitOfWork.GetRepository<Passenger>();
        var customer = await repo.GetByIdAsync(id);
        if (customer == null) return false;

        repo.Delete(customer);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PassengerNoteDto>> GetNotesForCustomerAsync(Guid customerId)
    {
        var repo = _unitOfWork.GetRepository<PassengerNote>();
        var notes = await repo.FindAsync(n => n.PassengerId == customerId);
        return notes
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new PassengerNoteDto
            {
                Id = n.Id,
                PassengerId = n.PassengerId,
                NoteText = n.NoteText,
                CreatedBy = n.CreatedBy,
                CreatedAt = n.CreatedAt
            })
            .ToList();
    }

    public async Task<PassengerNoteDto> AddNoteToCustomerAsync(Guid customerId, string noteText, string author)
    {
        var repo = _unitOfWork.GetRepository<PassengerNote>();
        var note = new PassengerNote
        {
            PassengerId = customerId,
            NoteText = noteText,
            CreatedBy = author,
            UpdatedBy = author
        };

        await repo.AddAsync(note);
        await _unitOfWork.SaveChangesAsync();

        return new PassengerNoteDto
        {
            Id = note.Id,
            PassengerId = note.PassengerId,
            NoteText = note.NoteText,
            CreatedBy = note.CreatedBy,
            CreatedAt = note.CreatedAt
        };
    }

    public async Task<bool> DeleteNoteAsync(Guid noteId)
    {
        var repo = _unitOfWork.GetRepository<PassengerNote>();
        var note = await repo.GetByIdAsync(noteId);
        if (note == null) return false;

        repo.Delete(note);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<AttachmentDto>> GetAttachmentsForCustomerAsync(Guid customerId)
    {
        var linksRepo = _unitOfWork.GetRepository<AttachmentLink>();
        var attRepo = _unitOfWork.GetRepository<Attachment>();

        var links = await linksRepo.FindAsync(l => l.TargetEntityName == "Passenger" && l.TargetEntityId == customerId);
        var attachmentIds = links.Select(l => l.AttachmentId).ToList();

        var attachments = await attRepo.FindAsync(a => attachmentIds.Contains(a.Id));
        return attachments
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FilePath = a.FilePath,
                ContentType = a.ContentType,
                FileSizeBytes = a.FileSizeBytes,
                CreatedAt = a.CreatedAt
            })
            .ToList();
    }

    public async Task<AttachmentDto> AddAttachmentToCustomerAsync(Guid customerId, string fileName, string filePath, string contentType, long sizeBytes)
    {
        var att = new Attachment
        {
            FileName = fileName,
            FilePath = filePath,
            ContentType = contentType,
            FileSizeBytes = sizeBytes,
            CreatedBy = "system",
            UpdatedBy = "system"
        };
        await _unitOfWork.GetRepository<Attachment>().AddAsync(att);

        var link = new AttachmentLink
        {
            AttachmentId = att.Id,
            TargetEntityName = "Passenger",
            TargetEntityId = customerId
        };
        await _unitOfWork.GetRepository<AttachmentLink>().AddAsync(link);

        await _unitOfWork.SaveChangesAsync();

        return new AttachmentDto
        {
            Id = att.Id,
            FileName = att.FileName,
            FilePath = att.FilePath,
            ContentType = att.ContentType,
            FileSizeBytes = att.FileSizeBytes,
            CreatedAt = att.CreatedAt
        };
    }

    public async Task<bool> DeleteAttachmentAsync(Guid attachmentId)
    {
        var linksRepo = _unitOfWork.GetRepository<AttachmentLink>();
        var attRepo = _unitOfWork.GetRepository<Attachment>();

        var links = await linksRepo.FindAsync(l => l.AttachmentId == attachmentId && l.TargetEntityName == "Passenger");
        foreach (var link in links)
        {
            linksRepo.Delete(link);
        }

        var att = await attRepo.GetByIdAsync(attachmentId);
        if (att != null)
        {
            attRepo.Delete(att);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<CustomerBookingHistoryDto>> GetCustomerBookingHistoryAsync(Guid customerId)
    {
        var bookings = await _unitOfWork.GetRepository<Booking>().FindAsync(b => b.PassengerId == customerId);
        var partners = await _unitOfWork.GetRepository<Partner>().GetAllAsync();

        return (from b in bookings
                join p in partners on b.PartnerId equals p.Id
                select new CustomerBookingHistoryDto
                {
                    BookingId = b.Id,
                    BookingNumber = b.BookingNumber,
                    PartnerName = p.Name,
                    QrStatus = b.HasQrCode ? "نعم" : "لا",
                    TravelDate = b.TravelDate,
                    SellingPrice = b.SellingPrice,
                    AmountPaid = b.PaymentsCollected,
                    RemainingBalance = b.RemainingBalance,
                    Status = b.RemainingBalance <= 0 ? "مسدد بالكامل" : b.PaymentsCollected > 0 ? "جزئي" : "غير مسدد"
                })
                .OrderByDescending(b => b.TravelDate)
                .ToList();
    }

    public async Task<CustomerImportResultDto> ImportCustomersFromExcelAsync(IEnumerable<CustomerDetailsDto> customers, string? groupNumber = null)
    {
        var customerDtos = customers?.ToList() ?? new List<CustomerDetailsDto>();
        _logger.LogInformation("Importing {RowCount} customer rows from Excel.", customerDtos.Count);

        var result = new CustomerImportResultDto
        {
            TotalRows = customerDtos.Count
        };

        if (!customerDtos.Any())
        {
            return result;
        }

        var tenantId = _currentUserProvider.GetCurrentTenantId();
        if (tenantId == Guid.Empty)
        {
            var defaultTenant = (await _unitOfWork.GetRepository<Tenant>().GetAllAsync()).FirstOrDefault();
            tenantId = defaultTenant?.Id ?? Guid.Empty;
        }

        var passengerRepo = _unitOfWork.GetRepository<Passenger>();
        var partners = await _unitOfWork.GetRepository<Partner>().GetAllAsync();

        var normalizedPassports = customerDtos
            .Where(c => !string.IsNullOrWhiteSpace(c.PassportNumber))
            .Select(c => c.PassportNumber.Trim().ToUpper())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var normalizedNationalIds = customerDtos
            .Where(c => !string.IsNullOrWhiteSpace(c.NationalId))
            .Select(c => c.NationalId!.Trim())
            .Distinct()
            .ToList();

        var hashedPassports = normalizedPassports.Select(BusinessManagement.Shared.Security.EncryptionHelper.HashSearchableField).ToList();
        var hashedNationalIds = normalizedNationalIds.Select(BusinessManagement.Shared.Security.EncryptionHelper.HashSearchableField).ToList();

        var existingPassengers = new List<Passenger>();
        if (hashedPassports.Any() || hashedNationalIds.Any())
        {
            var existingByPassport = hashedPassports.Any()
                ? await passengerRepo.FindAsync(p => hashedPassports.Contains(p.PassportNumberHash), ignoreQueryFilters: true)
                : Enumerable.Empty<Passenger>();

            var existingByNationalId = hashedNationalIds.Any()
                ? await passengerRepo.FindAsync(p => hashedNationalIds.Contains(p.NationalIdHash), ignoreQueryFilters: true)
                : Enumerable.Empty<Passenger>();

            existingPassengers = existingByPassport.Concat(existingByNationalId).DistinctBy(p => p.Id).ToList();
        }

        var existingPassportSet = new HashSet<string>(existingPassengers.Select(p => p.PassportNumber), StringComparer.OrdinalIgnoreCase);
        var existingNationalIdSet = new HashSet<string>(existingPassengers.Select(p => p.NationalId), StringComparer.OrdinalIgnoreCase);

        var importPassportSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var importNationalIdSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var validPassengers = new List<Passenger>();

        int rowIndex = 1;
        foreach (var dto in customerDtos)
        {
            rowIndex++;
            var fullName = dto.FullName?.Trim() ?? string.Empty;
            var passportNumber = dto.PassportNumber?.Trim().ToUpper() ?? string.Empty;
            var nationalId = dto.NationalId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName))
            {
                result.ImportErrors.Add(new CustomerImportErrorDto { RowIndex = rowIndex, Message = "اسم المسافر مطلوب." });
                continue;
            }

            if (string.IsNullOrWhiteSpace(passportNumber))
            {
                result.ImportErrors.Add(new CustomerImportErrorDto { RowIndex = rowIndex, Message = "رقم جواز السفر مطلوب." });
                continue;
            }

            // Regex validation: Must start with A/a followed by exactly 7 digits
            if (!System.Text.RegularExpressions.Regex.IsMatch(passportNumber, @"^[aA]\d{7}$"))
            {
                result.ImportErrors.Add(new CustomerImportErrorDto { RowIndex = rowIndex, Message = "رقم جواز السفر غير صحيح. يجب أن يبدأ بحرف A ويليه 7 أرقام." });
                continue;
            }

            if (!string.IsNullOrWhiteSpace(nationalId) && (nationalId.Length != 14 || !nationalId.All(char.IsDigit)))
            {
                result.ImportErrors.Add(new CustomerImportErrorDto { RowIndex = rowIndex, Message = "الرقم القومي يجب أن يتكون من 14 رقماً." });
                continue;
            }

            // Resolve PartnerId from dto.PartnerName (affiliation column)
            Guid? partnerId = null;
            if (!string.IsNullOrWhiteSpace(dto.PartnerName))
            {
                var normAff = dto.PartnerName.Trim().ToLower();
                if (normAff == "mohammed") normAff = "mohamed"; // normalize mohammed to mohamed
                
                var partner = partners.FirstOrDefault(p => p.Username.Equals(normAff, StringComparison.OrdinalIgnoreCase));
                if (partner != null)
                {
                    partnerId = partner.Id;
                }
            }

            bool isDuplicate = false;
            if (existingPassportSet.Contains(passportNumber) || !importPassportSet.Add(passportNumber))
            {
                result.DuplicateCount++;
                isDuplicate = true;
            }
            else if (!string.IsNullOrWhiteSpace(nationalId) && (existingNationalIdSet.Contains(nationalId) || !importNationalIdSet.Add(nationalId)))
            {
                result.DuplicateCount++;
                isDuplicate = true;
            }

            if (isDuplicate)
            {
                continue;
            }

            validPassengers.Add(new Passenger
            {
                TenantId = tenantId,
                FullName = fullName,
                PassportNumber = passportNumber,
                PassportExpiry = dto.PassportExpiry ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(10)),
                NationalId = nationalId,
                DateOfBirth = dto.DateOfBirth,
                Nationality = string.IsNullOrWhiteSpace(dto.Nationality) ? "Egyptian" : dto.Nationality,
                Gender = string.IsNullOrWhiteSpace(dto.Gender) ? "Male" : dto.Gender,
                PhoneNumber = dto.PhoneNumber?.Trim() ?? string.Empty,
                Email = dto.Email?.Trim() ?? string.Empty,
                Address = dto.Address?.Trim() ?? string.Empty,
                EmergencyContactName = dto.EmergencyContactName?.Trim() ?? string.Empty,
                EmergencyContactPhone = dto.EmergencyContactPhone?.Trim() ?? string.Empty,
                PartnerId = partnerId,
                GroupNumber = groupNumber ?? dto.GroupNumber,
                CreatedBy = "system",
                UpdatedBy = "system"
            });
        }

        if (validPassengers.Any())
        {
            foreach (var passenger in validPassengers)
            {
                await passengerRepo.AddAsync(passenger);
            }

            await _unitOfWork.SaveChangesAsync();
            result.ImportedCount = validPassengers.Count;
        }

        return result;
    }
}
