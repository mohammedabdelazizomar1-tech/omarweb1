using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;

namespace BusinessManagement.Application.Interfaces;

public interface IPartnershipService
{
    Task<IEnumerable<PartnershipCapitalDto>> GetAllCapitalAsync();
    Task<PartnershipCapitalDto> CreateCapitalAsync(CreatePartnershipCapitalDto model);
    Task<IEnumerable<PartnerQuotaMetric>> CalculateProfitSharesAsync();
}





