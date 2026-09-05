using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Services;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusinessManagement.Tests.Services;

public class PartnershipServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<Booking>> _mockBookingRepo;
    private readonly Mock<IRepository<PartnershipCapital>> _mockCapitalRepo;
    private readonly Mock<IRepository<Partner>> _mockPartnerRepo;
    private readonly Mock<ILogger<PartnershipService>> _mockLogger;
    private readonly PartnershipService _partnershipService;

    public PartnershipServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockBookingRepo = new Mock<IRepository<Booking>>();
        _mockCapitalRepo = new Mock<IRepository<PartnershipCapital>>();
        _mockPartnerRepo = new Mock<IRepository<Partner>>();
        _mockLogger = new Mock<ILogger<PartnershipService>>();

        _mockUnitOfWork.Setup(u => u.GetRepository<Booking>()).Returns(_mockBookingRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<PartnershipCapital>()).Returns(_mockCapitalRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<Partner>()).Returns(_mockPartnerRepo.Object);

        _partnershipService = new PartnershipService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CalculateProfitSharesAsync_ShouldReturnCalculatedMetrics()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var capitals = new List<PartnershipCapital>
        {
            new PartnershipCapital
            {
                Id = Guid.NewGuid(),
                ShareholderName = "علاء",
                AmountSar = 1000,
                AmountEgp = 13000,
                HistoricalRate = 13.0m,
                ShareRatio = 0.5m,
                ProfitShareRatio = 0.5m
            }
        };

        var partners = new List<Partner>
        {
            new Partner
            {
                Id = partnerId,
                Name = "Alaa",
                Username = "alaa",
                QuotaLimit = 10
            }
        };

        var bookings = new List<Booking>
        {
            new Booking
            {
                Id = Guid.NewGuid(),
                PartnerId = partnerId,
                SellingPrice = 10000,
                HasQrCode = true,
                BookingCost = new BookingCost
                {
                    NetCost = 5000,
                    BarcodeCost = 1000,
                    CompanyMarkup = 500
                }
            }
        };

        _mockCapitalRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(capitals);
        _mockPartnerRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(partners);
        _mockBookingRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings);

        // Act
        var result = (await _partnershipService.CalculateProfitSharesAsync()).ToList();

        // Assert
        Assert.Single(result);
        var metric = result[0];
        Assert.Equal("علاء", metric.PartnerName);
        Assert.Equal(1, metric.BookingsCount);
        Assert.Equal(1, metric.QrCount);
        Assert.Equal(0, metric.NonQrCount);
        Assert.Equal(3500, metric.TotalProfit); // 10000 - (5000 + 1000 + 500) = 3500
    }
}
