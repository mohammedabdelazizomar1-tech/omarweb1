using System.Threading.Tasks;
using Bms.Application.DTOs;

namespace Bms.Application.Interfaces;

public interface ITravelDashboardService
{
    Task<TravelDashboardDto> GetDashboardDataAsync(string? partnerUsername = null);
}
