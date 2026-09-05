using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;

namespace BusinessManagement.Application.Interfaces;

public interface ITravelDashboardService
{
    Task<TravelDashboardDto> GetDashboardDataAsync(string? partnerUsername = null);
}





