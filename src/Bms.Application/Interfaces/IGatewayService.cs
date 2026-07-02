using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bms.Application.DTOs;

namespace Bms.Application.Interfaces;

public interface IGatewayService
{
    Task<IEnumerable<GatewayPortalDto>> GetAllPortalPurchasesAsync();
    Task<GatewayPortalDto> CreatePortalPurchaseAsync(CreateGatewayPortalDto model);
    Task<(int QrCount, int VipCount)> GetPortalBalancesAsync();
}
