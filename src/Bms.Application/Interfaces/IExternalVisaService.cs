using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bms.Application.DTOs;

namespace Bms.Application.Interfaces;

public interface IExternalVisaService
{
    Task<IEnumerable<ExternalVisaDto>> GetAllVisasAsync();
    Task<ExternalVisaDto?> GetVisaByIdAsync(Guid id);
    Task<ExternalVisaDto> CreateVisaAsync(CreateExternalVisaDto model);
    Task DeleteVisaAsync(Guid id);
}
