using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Interfaces;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Enums;
using BusinessManagement.Domain.Repositories;

namespace BusinessManagement.Application.Services;

public class GatewayService : IGatewayService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GatewayService> _logger;

    public GatewayService(IUnitOfWork unitOfWork, ILogger<GatewayService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<GatewayPortalDto>> GetAllPortalPurchasesAsync()
    {
        _logger.LogInformation("Retrieving all portal visa slot purchases.");
        
        var purchases = await _unitOfWork.GetRepository<GatewayPortal>().GetAllAsync();
        var partners = await _unitOfWork.GetRepository<Partner>().GetAllAsync();

        var query = from pr in purchases
                    join p in partners on pr.PartnerId equals p.Id into joined
                    from partner in joined.DefaultIfEmpty()
                    select new { pr, partner };

        return query.Select(x => new GatewayPortalDto
        {
            Id = x.pr.Id,
            TransactionDate = x.pr.TransactionDate,
            Amount = x.pr.Amount,
            Count = x.pr.Count,
            ServiceType = x.pr.ServiceType.ToString(),
            PartnerId = x.pr.PartnerId,
            PartnerName = x.partner?.Name ?? "General Corporate Quota",
            Notes = x.pr.Notes,
            CreatedAt = x.pr.CreatedAt
        }).OrderByDescending(x => x.TransactionDate);
    }

    public async Task<GatewayPortalDto> CreatePortalPurchaseAsync(CreateGatewayPortalDto model)
    {
        _logger.LogInformation("Logging portal quota purchase: Type={Type}, Count={Count}, Cost={Amount}", 
            model.ServiceType, model.Count, model.Amount);

        var purchase = new GatewayPortal
        {
            TransactionDate = model.TransactionDate,
            Amount = model.Amount,
            Count = model.Count,
            ServiceType = model.ServiceType,
            PartnerId = model.PartnerId,
            Notes = model.Notes?.Trim() ?? string.Empty
        };

        await _unitOfWork.GetRepository<GatewayPortal>().AddAsync(purchase);
        
        // Log this purchase as an cash outflow in EGP safe if Amount > 0!
        if (model.Amount > 0)
        {
            var egpOutflow = new SafeTransaction
            {
                TransactionDate = model.TransactionDate,
                Amount = model.Amount,
                TransactionType = TransactionType.Withdrawal,
                Currency = Currency.EGP,
                ExchangeRate = 1.0m,
                AssociatedPartnerId = model.PartnerId,
                DepositorOrWithdrawerName = "Saudi Portal Vendor",
                Description = $"Visa Gateway slots replenishment ({model.Count} x {model.ServiceType})",
                BankName = "Direct safe withdrawal"
            };
            await _unitOfWork.GetRepository<SafeTransaction>().AddAsync(egpOutflow);
        }

        await _unitOfWork.CompleteAsync();

        var partner = model.PartnerId.HasValue 
            ? await _unitOfWork.GetRepository<Partner>().GetByIdAsync(model.PartnerId.Value) 
            : null;

        return new GatewayPortalDto
        {
            Id = purchase.Id,
            TransactionDate = purchase.TransactionDate,
            Amount = purchase.Amount,
            Count = purchase.Count,
            ServiceType = purchase.ServiceType.ToString(),
            PartnerId = purchase.PartnerId,
            PartnerName = partner?.Name ?? "General Corporate Quota",
            Notes = purchase.Notes,
            CreatedAt = purchase.CreatedAt
        };
    }

    public async Task<(int QrCount, int VipCount)> GetPortalBalancesAsync()
    {
        var purchases = await _unitOfWork.GetRepository<GatewayPortal>().GetAllAsync();
        var bookings = await _unitOfWork.GetRepository<Booking>().GetAllAsync();

        int purchasedQr = purchases.Where(p => p.ServiceType == PortalServiceType.QrCode).Sum(p => p.Count);
        int consumedQr = bookings.Count(b => b.HasQrCode);

        int purchasedVip = purchases.Where(p => p.ServiceType == PortalServiceType.Vip).Sum(p => p.Count);
        // Note: VIP consumption mapping could be expanded if needed. For now it acts as an inventory count.

        return (QrCount: Math.Max(0, purchasedQr - consumedQr), VipCount: purchasedVip);
    }
}





