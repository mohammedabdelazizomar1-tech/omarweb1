using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Bms.Application.DTOs;
using Bms.Application.Interfaces;
using Bms.Domain.Entities;
using Bms.Domain.Repositories;

namespace Bms.Application.Services;

public class PartnershipService : IPartnershipService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PartnershipService> _logger;

    public PartnershipService(IUnitOfWork unitOfWork, ILogger<PartnershipService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<PartnershipCapitalDto>> GetAllCapitalAsync()
    {
        _logger.LogInformation("Retrieving partner capital structure.");
        var capitals = await _unitOfWork.GetRepository<PartnershipCapital>().GetAllAsync();
        return capitals.Select(c => new PartnershipCapitalDto
        {
            Id = c.Id,
            ShareholderName = c.ShareholderName,
            AmountSar = c.AmountSar,
            AmountEgp = c.AmountEgp,
            HistoricalRate = c.HistoricalRate,
            ShareRatio = c.ShareRatio,
            ProfitShareRatio = c.ProfitShareRatio,
            Notes = c.Notes,
            CreatedAt = c.CreatedAt
        }).OrderByDescending(c => c.AmountEgp);
    }

    public async Task<PartnershipCapitalDto> CreateCapitalAsync(CreatePartnershipCapitalDto model)
    {
        _logger.LogInformation("Registering capital investment from: {ShareholderName}", model.ShareholderName);
        
        var capital = new PartnershipCapital
        {
            ShareholderName = model.ShareholderName.Trim(),
            AmountSar = model.AmountSar,
            AmountEgp = model.AmountEgp,
            HistoricalRate = model.HistoricalRate,
            ShareRatio = model.ShareRatio,
            ProfitShareRatio = model.ProfitShareRatio,
            Notes = model.Notes?.Trim() ?? string.Empty
        };

        await _unitOfWork.GetRepository<PartnershipCapital>().AddAsync(capital);

        // Also record EGP amount as safe deposit!
        if (model.AmountEgp > 0)
        {
            var egpDeposit = new SafeTransaction
            {
                TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Amount = model.AmountEgp,
                TransactionType = Domain.Enums.TransactionType.Deposit,
                Currency = Domain.Enums.Currency.EGP,
                ExchangeRate = 1.0m,
                DepositorOrWithdrawerName = model.ShareholderName,
                Description = $"Partnership Capital Contribution (EGP)",
                BankName = "Safe Cash box"
            };
            await _unitOfWork.GetRepository<SafeTransaction>().AddAsync(egpDeposit);
        }

        // Also record SAR amount in safe if it exists!
        if (model.AmountSar > 0)
        {
            var sarDeposit = new SafeTransaction
            {
                TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Amount = model.AmountSar,
                TransactionType = Domain.Enums.TransactionType.Deposit,
                Currency = Domain.Enums.Currency.SAR,
                ExchangeRate = model.HistoricalRate,
                DepositorOrWithdrawerName = model.ShareholderName,
                Description = $"Partnership Capital Contribution (SAR)",
                BankName = "Safe Cash box"
            };
            await _unitOfWork.GetRepository<SafeTransaction>().AddAsync(sarDeposit);
        }

        await _unitOfWork.CompleteAsync();

        return new PartnershipCapitalDto
        {
            Id = capital.Id,
            ShareholderName = capital.ShareholderName,
            AmountSar = capital.AmountSar,
            AmountEgp = capital.AmountEgp,
            HistoricalRate = capital.HistoricalRate,
            ShareRatio = capital.ShareRatio,
            ProfitShareRatio = capital.ProfitShareRatio,
            Notes = capital.Notes,
            CreatedAt = capital.CreatedAt
        };
    }

    public async Task<IEnumerable<PartnerQuotaMetric>> CalculateProfitSharesAsync()
    {
        var bookings = await _unitOfWork.GetRepository<Booking>().GetAllAsync();
        var capitals = await _unitOfWork.GetRepository<PartnershipCapital>().GetAllAsync();
        
        decimal totalProfit = bookings.Sum(b => b.NetProfit);
        
        return capitals.Select(c =>
        {
            // Compute share based on their registered ratio
            decimal partnerProfitShare = totalProfit * c.ProfitShareRatio;
            return new PartnerQuotaMetric
            {
                PartnerId = c.Id,
                PartnerName = c.ShareholderName,
                BookingsCount = 0, // Lookup for metadata
                QrCount = 0,
                NonQrCount = 0,
                QuotaLimit = 0,
                TotalProfit = partnerProfitShare,
                CollectedPayments = 0,
                RemainingBalance = 0
            };
        });
    }
}
