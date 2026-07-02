using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Bms.Application.DTOs;
using Bms.Application.Interfaces;
using Bms.Domain.Entities;
using Bms.Domain.Enums;
using Bms.Domain.Repositories;

namespace Bms.Application.Services;

public class SafeService : ISafeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SafeService> _logger;

    public SafeService(IUnitOfWork unitOfWork, ILogger<SafeService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<SafeTransactionDto>> GetAllTransactionsAsync()
    {
        _logger.LogInformation("Retrieving all safe cash flow transactions.");
        
        var transactions = await _unitOfWork.GetRepository<SafeTransaction>().GetAllAsync();
        var partners = await _unitOfWork.GetRepository<Partner>().GetAllAsync();

        var query = from t in transactions
                    join p in partners on t.AssociatedPartnerId equals p.Id into joined
                    from partner in joined.DefaultIfEmpty()
                    select new { t, partner };

        return query.Select(x => new SafeTransactionDto
        {
            Id = x.t.Id,
            TransactionDate = x.t.TransactionDate,
            Amount = x.t.Amount,
            TransactionType = x.t.TransactionType.ToString(),
            Currency = x.t.Currency.ToString(),
            ExchangeRate = x.t.ExchangeRate,
            AssociatedPartnerId = x.t.AssociatedPartnerId,
            AssociatedPartnerName = x.partner?.Name ?? "Corporate / Treasury",
            DepositorOrWithdrawerName = x.t.DepositorOrWithdrawerName,
            Description = x.t.Description,
            BankName = x.t.BankName,
            CreatedAt = x.t.CreatedAt
        }).OrderByDescending(x => x.TransactionDate);
    }

    public async Task<SafeTransactionDto?> GetTransactionByIdAsync(Guid id)
    {
        var t = await _unitOfWork.GetRepository<SafeTransaction>().GetByIdAsync(id);
        if (t == null) return null;

        var partner = t.AssociatedPartnerId.HasValue 
            ? await _unitOfWork.GetRepository<Partner>().GetByIdAsync(t.AssociatedPartnerId.Value) 
            : null;

        return new SafeTransactionDto
        {
            Id = t.Id,
            TransactionDate = t.TransactionDate,
            Amount = t.Amount,
            TransactionType = t.TransactionType.ToString(),
            Currency = t.Currency.ToString(),
            ExchangeRate = t.ExchangeRate,
            AssociatedPartnerId = t.AssociatedPartnerId,
            AssociatedPartnerName = partner?.Name ?? "Corporate / Treasury",
            DepositorOrWithdrawerName = t.DepositorOrWithdrawerName,
            Description = t.Description,
            BankName = t.BankName,
            CreatedAt = t.CreatedAt
        };
    }

    public async Task<SafeTransactionDto> CreateTransactionAsync(CreateSafeTransactionDto model)
    {
        _logger.LogInformation("Logging cash safe transaction: Type={Type}, Currency={Currency}, Amount={Amount}", 
            model.TransactionType, model.Currency, model.Amount);

        var transaction = new SafeTransaction
        {
            TransactionDate = model.TransactionDate,
            Amount = model.Amount,
            TransactionType = model.TransactionType,
            Currency = model.Currency,
            ExchangeRate = model.ExchangeRate,
            AssociatedPartnerId = model.AssociatedPartnerId,
            DepositorOrWithdrawerName = model.DepositorOrWithdrawerName.Trim(),
            Description = model.Description?.Trim() ?? string.Empty,
            BankName = model.BankName?.Trim() ?? string.Empty
        };

        await _unitOfWork.GetRepository<SafeTransaction>().AddAsync(transaction);
        await _unitOfWork.CompleteAsync();

        var partner = model.AssociatedPartnerId.HasValue 
            ? await _unitOfWork.GetRepository<Partner>().GetByIdAsync(model.AssociatedPartnerId.Value) 
            : null;

        return new SafeTransactionDto
        {
            Id = transaction.Id,
            TransactionDate = transaction.TransactionDate,
            Amount = transaction.Amount,
            TransactionType = transaction.TransactionType.ToString(),
            Currency = transaction.Currency.ToString(),
            ExchangeRate = transaction.ExchangeRate,
            AssociatedPartnerId = transaction.AssociatedPartnerId,
            AssociatedPartnerName = partner?.Name ?? "Corporate / Treasury",
            DepositorOrWithdrawerName = transaction.DepositorOrWithdrawerName,
            Description = transaction.Description,
            BankName = transaction.BankName,
            CreatedAt = transaction.CreatedAt
        };
    }

    public async Task DeleteTransactionAsync(Guid id)
    {
        _logger.LogWarning("Deleting cash safe transaction record: {TransactionId}", id);
        
        var transaction = await _unitOfWork.GetRepository<SafeTransaction>().GetByIdAsync(id);
        if (transaction == null)
        {
            throw new KeyNotFoundException("Safe transaction record not found.");
        }

        _unitOfWork.GetRepository<SafeTransaction>().Delete(transaction);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<(decimal Egp, decimal Sar)> GetSafeBalancesAsync()
    {
        var transactions = await _unitOfWork.GetRepository<SafeTransaction>().GetAllAsync();
        
        decimal egpDeposits = transactions.Where(t => t.Currency == Currency.EGP && t.TransactionType == TransactionType.Deposit).Sum(t => t.Amount);
        decimal egpWithdrawals = transactions.Where(t => t.Currency == Currency.EGP && t.TransactionType == TransactionType.Withdrawal).Sum(t => t.Amount);
        
        decimal sarDeposits = transactions.Where(t => t.Currency == Currency.SAR && t.TransactionType == TransactionType.Deposit).Sum(t => t.Amount);
        decimal sarWithdrawals = transactions.Where(t => t.Currency == Currency.SAR && t.TransactionType == TransactionType.Withdrawal).Sum(t => t.Amount);

        return (Egp: egpDeposits - egpWithdrawals, Sar: sarDeposits - sarWithdrawals);
    }
}
