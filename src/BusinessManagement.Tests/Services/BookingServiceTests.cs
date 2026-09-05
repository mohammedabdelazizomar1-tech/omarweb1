using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Services;
using BusinessManagement.Domain;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusinessManagement.Tests.Services;

public class BookingServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICurrentUserProvider> _mockCurrentUserProvider;
    private readonly Mock<ILogger<BookingService>> _mockLogger;
    private readonly Mock<IRepository<Partner>> _mockPartnerRepo;
    private readonly Mock<IRepository<Passenger>> _mockPassengerRepo;
    private readonly Mock<IRepository<Booking>> _mockBookingRepo;
    private readonly Mock<IRepository<BookingPayment>> _mockPaymentRepo;
    private readonly Mock<IRepository<SafeTransaction>> _mockSafeRepo;
    private readonly Mock<IRepository<Tenant>> _mockTenantRepo;
    private readonly Mock<IRepository<Branch>> _mockBranchRepo;
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCurrentUserProvider = new Mock<ICurrentUserProvider>();
        _mockLogger = new Mock<ILogger<BookingService>>();
        
        _mockPartnerRepo = new Mock<IRepository<Partner>>();
        _mockPassengerRepo = new Mock<IRepository<Passenger>>();
        _mockBookingRepo = new Mock<IRepository<Booking>>();
        _mockPaymentRepo = new Mock<IRepository<BookingPayment>>();
        _mockSafeRepo = new Mock<IRepository<SafeTransaction>>();
        _mockTenantRepo = new Mock<IRepository<Tenant>>();
        _mockBranchRepo = new Mock<IRepository<Branch>>();

        _mockUnitOfWork.Setup(u => u.GetRepository<Partner>()).Returns(_mockPartnerRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<Passenger>()).Returns(_mockPassengerRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<Booking>()).Returns(_mockBookingRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<BookingPayment>()).Returns(_mockPaymentRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<SafeTransaction>()).Returns(_mockSafeRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<Tenant>()).Returns(_mockTenantRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<Branch>()).Returns(_mockBranchRepo.Object);

        _bookingService = new BookingService(_mockUnitOfWork.Object, _mockCurrentUserProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldGenerateB001001_WhenNoBookingsExist()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var passengerId = Guid.NewGuid();
        
        _mockPartnerRepo.Setup(r => r.GetByIdAsync(partnerId)).ReturnsAsync(new Partner { Id = partnerId, QuotaLimit = 10, Name = "Test Partner" });
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId)).ReturnsAsync(new Passenger { Id = passengerId, FullName = "John Doe" });
        
        // Return empty list of bookings
        _mockBookingRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Booking, bool>>>(), true))
            .ReturnsAsync(new List<Booking>());

        var model = new CreateBookingDto
        {
            PartnerId = partnerId,
            PassengerId = passengerId,
            TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
        };

        // Act
        var result = await _bookingService.CreateBookingAsync(model);

        // Assert
        Assert.Equal("B-001001", result.BookingNumber);
        _mockBookingRepo.Verify(r => r.AddAsync(It.Is<Booking>(b => b.BookingNumber == "B-001001")), Times.Once);
        _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldIncrementMaxBookingNumber_IncludingSoftDeletedAndOtherTenants()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var passengerId = Guid.NewGuid();
        
        _mockPartnerRepo.Setup(r => r.GetByIdAsync(partnerId)).ReturnsAsync(new Partner { Id = partnerId, QuotaLimit = 10, Name = "Test Partner" });
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId)).ReturnsAsync(new Passenger { Id = passengerId, FullName = "John Doe" });
        
        // Mock existing bookings: some active sequential, one soft-deleted sequential, one Excel imported (random)
        var existingBookings = new List<Booking>
        {
            new Booking { BookingNumber = "B-001001" },
            new Booking { BookingNumber = "B-001002" },
            new Booking { BookingNumber = "B-001005", IsDeleted = true }, // Soft deleted max sequential
            new Booking { BookingNumber = "B-EF93A2D1" } // Excel imported random pattern
        };

        _mockBookingRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Booking, bool>>>(), true))
            .ReturnsAsync(existingBookings);

        var model = new CreateBookingDto
        {
            PartnerId = partnerId,
            PassengerId = passengerId,
            TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
        };

        // Act
        var result = await _bookingService.CreateBookingAsync(model);

        // Assert
        // Highest numeric sequence was B-001005 (even though it's soft-deleted). The next should be B-001006.
        // B-EF93A2D1 should be ignored.
        Assert.Equal("B-001006", result.BookingNumber);
        _mockBookingRepo.Verify(r => r.AddAsync(It.Is<Booking>(b => b.BookingNumber == "B-001006")), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldSetTenantAndBranchIds_WhenCreatingBooking()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var passengerId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        
        _mockCurrentUserProvider.Setup(p => p.GetCurrentTenantId()).Returns(tenantId);
        _mockPartnerRepo.Setup(r => r.GetByIdAsync(partnerId)).ReturnsAsync(new Partner { Id = partnerId, QuotaLimit = 10, Name = "Test Partner" });
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId)).ReturnsAsync(new Passenger { Id = passengerId, FullName = "John Doe" });
        _mockBranchRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Branch> { new Branch { Id = branchId, Name = "Main Branch" } });
        
        _mockBookingRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Booking, bool>>>(), true))
            .ReturnsAsync(new List<Booking>());

        var model = new CreateBookingDto
        {
            PartnerId = partnerId,
            PassengerId = passengerId,
            TravelDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            PaymentsCollected = 100
        };

        // Act
        await _bookingService.CreateBookingAsync(model);

        // Assert
        _mockBookingRepo.Verify(r => r.AddAsync(It.Is<Booking>(b => b.TenantId == tenantId && b.BranchId == branchId)), Times.Once);
        _mockSafeRepo.Verify(r => r.AddAsync(It.Is<SafeTransaction>(tx => tx.TenantId == tenantId)), Times.Once);
    }
}
