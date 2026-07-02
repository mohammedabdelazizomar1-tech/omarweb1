using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bms.Application.DTOs;

namespace Bms.Application.Interfaces;

public interface ISafeService
{
    Task<IEnumerable<SafeTransactionDto>> GetAllTransactionsAsync();
    Task<SafeTransactionDto?> GetTransactionByIdAsync(Guid id);
    Task<SafeTransactionDto> CreateTransactionAsync(CreateSafeTransactionDto model);
    Task DeleteTransactionAsync(Guid id);
    Task<(decimal Egp, decimal Sar)> GetSafeBalancesAsync();
}
