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

public class ExternalVisaService : IExternalVisaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExternalVisaService> _logger;

    public ExternalVisaService(IUnitOfWork unitOfWork, ILogger<ExternalVisaService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<ExternalVisaDto>> GetAllVisasAsync()
    {
        _logger.LogInformation("Retrieving all external visa registries.");
        var visas = await _unitOfWork.GetRepository<ExternalVisa>().GetAllAsync();
        return visas.Select(v => new ExternalVisaDto
        {
            Id = v.Id,
            PassengerName = v.PassengerName,
            Affiliation = v.Affiliation,
            NetCost = v.NetCost,
            BarcodeCost = v.BarcodeCost,
            AgentCommission = v.AgentCommission,
            AgreementCost = v.AgreementCost,
            TicketCost = v.TicketCost,
            AirportCost = v.AirportCost,
            BusCost = v.BusCost,
            TotalCost = v.TotalCost,
            SellingPrice = v.SellingPrice,
            NetProfit = v.NetProfit,
            AmountPaid = v.AmountPaid,
            RemainingBalance = v.RemainingBalance,
            Notes = v.Notes,
            CreatedAt = v.CreatedAt
        }).OrderByDescending(v => v.CreatedAt);
    }

    public async Task<ExternalVisaDto?> GetVisaByIdAsync(Guid id)
    {
        var v = await _unitOfWork.GetRepository<ExternalVisa>().GetByIdAsync(id);
        if (v == null) return null;
        return new ExternalVisaDto
        {
            Id = v.Id,
            PassengerName = v.PassengerName,
            Affiliation = v.Affiliation,
            NetCost = v.NetCost,
            BarcodeCost = v.BarcodeCost,
            AgentCommission = v.AgentCommission,
            AgreementCost = v.AgreementCost,
            TicketCost = v.TicketCost,
            AirportCost = v.AirportCost,
            BusCost = v.BusCost,
            TotalCost = v.TotalCost,
            SellingPrice = v.SellingPrice,
            NetProfit = v.NetProfit,
            AmountPaid = v.AmountPaid,
            RemainingBalance = v.RemainingBalance,
            Notes = v.Notes,
            CreatedAt = v.CreatedAt
        };
    }

    public async Task<ExternalVisaDto> CreateVisaAsync(CreateExternalVisaDto model)
    {
        _logger.LogInformation("Logging external visa deal for passenger: {Name}", model.PassengerName);

        // Sum calculations
        decimal totalCost = model.NetCost + model.BarcodeCost + model.AgentCommission + model.AgreementCost + model.TicketCost + model.AirportCost + model.BusCost;
        decimal netProfit = model.SellingPrice - totalCost;
        decimal remaining = model.SellingPrice - model.AmountPaid;

        var visa = new ExternalVisa
        {
            PassengerName = model.PassengerName.Trim(),
            Affiliation = model.Affiliation.Trim(),
            NetCost = model.NetCost,
            BarcodeCost = model.BarcodeCost,
            AgentCommission = model.AgentCommission,
            AgreementCost = model.AgreementCost,
            TicketCost = model.TicketCost,
            AirportCost = model.AirportCost,
            BusCost = model.BusCost,
            TotalCost = totalCost,
            SellingPrice = model.SellingPrice,
            NetProfit = netProfit,
            AmountPaid = model.AmountPaid,
            RemainingBalance = remaining,
            Notes = model.Notes ?? string.Empty
        };

        await _unitOfWork.GetRepository<ExternalVisa>().AddAsync(visa);
        await _unitOfWork.CompleteAsync();

        return new ExternalVisaDto
        {
            Id = visa.Id,
            PassengerName = visa.PassengerName,
            Affiliation = visa.Affiliation,
            NetCost = visa.NetCost,
            BarcodeCost = visa.BarcodeCost,
            AgentCommission = visa.AgentCommission,
            AgreementCost = visa.AgreementCost,
            TicketCost = visa.TicketCost,
            AirportCost = visa.AirportCost,
            BusCost = visa.BusCost,
            TotalCost = visa.TotalCost,
            SellingPrice = visa.SellingPrice,
            NetProfit = visa.NetProfit,
            AmountPaid = visa.AmountPaid,
            RemainingBalance = visa.RemainingBalance,
            Notes = visa.Notes,
            CreatedAt = visa.CreatedAt
        };
    }

    public async Task DeleteVisaAsync(Guid id)
    {
        _logger.LogWarning("Deleting external visa file ID: {VisaId}", id);
        
        var visa = await _unitOfWork.GetRepository<ExternalVisa>().GetByIdAsync(id);
        if (visa == null)
        {
            throw new KeyNotFoundException("External visa record not found.");
        }

        _unitOfWork.GetRepository<ExternalVisa>().Delete(visa);
        await _unitOfWork.CompleteAsync();
    }
}
