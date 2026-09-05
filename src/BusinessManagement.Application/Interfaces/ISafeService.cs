using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;

namespace BusinessManagement.Application.Interfaces;

public interface ISafeService
{
    Task<IEnumerable<SafeTransactionDto>> GetAllTransactionsAsync();
    Task<SafeTransactionDto?> GetTransactionByIdAsync(Guid id);
    Task<SafeTransactionDto> CreateTransactionAsync(CreateSafeTransactionDto model);
    Task DeleteTransactionAsync(Guid id);
    Task<(decimal Egp, decimal Sar)> GetSafeBalancesAsync();
}





