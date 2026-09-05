using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;

namespace BusinessManagement.Application.Interfaces;

public interface IGatewayService
{
    Task<IEnumerable<GatewayPortalDto>> GetAllPortalPurchasesAsync();
    Task<GatewayPortalDto> CreatePortalPurchaseAsync(CreateGatewayPortalDto model);
    Task<(int QrCount, int VipCount)> GetPortalBalancesAsync();
}





