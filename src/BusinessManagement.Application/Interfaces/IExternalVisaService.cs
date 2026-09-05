using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;

namespace BusinessManagement.Application.Interfaces;

public interface IExternalVisaService
{
    Task<IEnumerable<ExternalVisaDto>> GetAllVisasAsync();
    Task<ExternalVisaDto?> GetVisaByIdAsync(Guid id);
    Task<ExternalVisaDto> CreateVisaAsync(CreateExternalVisaDto model);
    Task DeleteVisaAsync(Guid id);
}





