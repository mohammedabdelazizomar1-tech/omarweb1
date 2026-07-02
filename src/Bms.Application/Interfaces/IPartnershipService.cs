using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bms.Application.DTOs;

namespace Bms.Application.Interfaces;

public interface IPartnershipService
{
    Task<IEnumerable<PartnershipCapitalDto>> GetAllCapitalAsync();
    Task<PartnershipCapitalDto> CreateCapitalAsync(CreatePartnershipCapitalDto model);
    Task<IEnumerable<PartnerQuotaMetric>> CalculateProfitSharesAsync();
}
