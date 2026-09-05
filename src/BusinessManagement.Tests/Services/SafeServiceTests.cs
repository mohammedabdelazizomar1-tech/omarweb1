using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessManagement.Application.DTOs;
using BusinessManagement.Application.Services;
using BusinessManagement.Domain;
using BusinessManagement.Domain.Entities;
using BusinessManagement.Domain.Enums;
using BusinessManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BusinessManagement.Tests.Services;

public class SafeServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<SafeService>> _mockLogger;
    private readonly Mock<IRepository<SafeTransaction>> _mockSafeRepo;
    private readonly Mock<IRepository<Booking>> _mockBookingRepo;
    private readonly Mock<IRepository<BookingPayment>> _mockPaymentRepo;
    private readonly Mock<IRepository<Passenger>> _mockPassengerRepo;
    private readonly Mock<IRepository<Partner>> _mockPartnerRepo;
    private readonly SafeService _safeService;

    public SafeServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<SafeService>>();
        _mockSafeRepo = new Mock<IRepository<SafeTransaction>>();
        _mockBookingRepo = new Mock<IRepository<Booking>>();
        _mockPaymentRepo = new Mock<IRepository<BookingPayment>>();
        _mockPassengerRepo = new Mock<IRepository<Passenger>>();
        _mockPartnerRepo = new Mock<IRepository<Partner>>();

        _mockUnitOfWork.Setup(u => u.GetRepository<SafeTransaction>()).Returns(_mockSafeRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<Booking>()).Returns(_mockBookingRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<BookingPayment>()).Returns(_mockPaymentRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<Passenger>()).Returns(_mockPassengerRepo.Object);
        _mockUnitOfWork.Setup(u => u.GetRepository<Partner>()).Returns(_mockPartnerRepo.Object);

        _safeService = new SafeService(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateTransactionAsync_WithBookingId_ShouldRegisterPaymentAndUpdateBooking()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var passengerId = Guid.NewGuid();
        var partnerId = Guid.NewGuid();

        var booking = new Booking
        {
            Id = bookingId,
            PassengerId = passengerId,
            PartnerId = partnerId,
            BookingNumber = "B-001001",
            SellingPrice = 10000m,
            PaymentsCollected = 4000m,
            RemainingBalance = 6000m
        };

        var passenger = new Passenger
        {
            Id = passengerId,
            FullName = "احمد محمد علي"
        };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId)).ReturnsAsync(passenger);
        _mockPaymentRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<BookingPayment, bool>>>()))
            .ReturnsAsync(new List<BookingPayment>
            {
                new BookingPayment { BookingId = bookingId, Amount = 4000m }
            });

        var createDto = new CreateSafeTransactionDto
        {
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = 3000m,
            TransactionType = TransactionType.Deposit,
            Currency = Currency.EGP,
            ExchangeRate = 1.0m,
            DepositorOrWithdrawerName = "احمد محمد علي",
            BookingId = bookingId,
            BookingPayments = new Dictionary<Guid, decimal> { { bookingId, 3000m } },
            Description = "دفع حجز اختبار"
        };

        // Act
        var result = await _safeService.CreateTransactionAsync(createDto);

        // Assert
        Assert.NotNull(result);
        _mockPaymentRepo.Verify(r => r.AddAsync(It.Is<BookingPayment>(p => 
            p.BookingId == bookingId && 
            p.Amount == 3000m && 
            p.PaymentMethod == "Cash")), Times.Once);

        _mockBookingRepo.Verify(r => r.Update(It.Is<Booking>(b => 
            b.PaymentsCollected == 7000m && 
            b.RemainingBalance == 3000m)), Times.Once);

        _mockSafeRepo.Verify(r => r.AddAsync(It.Is<SafeTransaction>(t => 
            t.Amount == 3000m && 
            t.Currency == Currency.EGP && 
            t.TransactionType == TransactionType.Deposit && 
            t.AssociatedPartnerId == partnerId && 
            t.DepositorOrWithdrawerName == "B-001001" &&
            t.Description == "دفع حجز اختبار")), Times.Once);

        _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateTransactionAsync_WithGroupBooking_ShouldUseGroupNumberAsDepositorName()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var passengerId = Guid.NewGuid();
        var partnerId = Guid.NewGuid();

        var booking = new Booking
        {
            Id = bookingId,
            PassengerId = passengerId,
            PartnerId = partnerId,
            BookingNumber = "B-001002",
            GroupNumber = "GRP-999",
            SellingPrice = 10000m,
            PaymentsCollected = 4000m,
            RemainingBalance = 6000m
        };

        var passenger = new Passenger
        {
            Id = passengerId,
            FullName = "مصطفى محمود"
        };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(bookingId)).ReturnsAsync(booking);
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId)).ReturnsAsync(passenger);

        var createDto = new CreateSafeTransactionDto
        {
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = 4000m,
            TransactionType = TransactionType.Deposit,
            Currency = Currency.EGP,
            ExchangeRate = 1.0m,
            DepositorOrWithdrawerName = "وسيط الدفع",
            BookingIds = new List<Guid> { bookingId },
            BookingPayments = new Dictionary<Guid, decimal> { { bookingId, 4000m } },
            Description = "سداد دفعة للمجموعة"
        };

        // Act
        var result = await _safeService.CreateTransactionAsync(createDto);

        // Assert
        Assert.NotNull(result);
        _mockSafeRepo.Verify(r => r.AddAsync(It.Is<SafeTransaction>(t => 
            t.Amount == 4000m && 
            t.Currency == Currency.EGP && 
            t.TransactionType == TransactionType.Deposit && 
            t.AssociatedPartnerId == partnerId && 
            t.DepositorOrWithdrawerName == "GRP-999" &&
            t.Description == "سداد دفعة للمجموعة")), Times.Once);
    }

    [Fact]
    public async Task CreateTransactionAsync_WithMixedGroupAndIndividualBookings_ShouldCombineGroupNumberAndBookingNumber()
    {
        // Arrange
        var bookingId1 = Guid.NewGuid();
        var bookingId2 = Guid.NewGuid();
        var passengerId1 = Guid.NewGuid();
        var passengerId2 = Guid.NewGuid();
        var partnerId = Guid.NewGuid();

        var booking1 = new Booking
        {
            Id = bookingId1,
            PassengerId = passengerId1,
            PartnerId = partnerId,
            BookingNumber = "B-001001",
            GroupNumber = "GRP-123",
            SellingPrice = 10000m,
            PaymentsCollected = 4000m,
            RemainingBalance = 6000m
        };

        var booking2 = new Booking
        {
            Id = bookingId2,
            PassengerId = passengerId2,
            PartnerId = partnerId,
            BookingNumber = "B-001002",
            GroupNumber = null,
            SellingPrice = 5000m,
            PaymentsCollected = 1000m,
            RemainingBalance = 4000m
        };

        var passenger1 = new Passenger { Id = passengerId1, FullName = "علي حسن" };
        var passenger2 = new Passenger { Id = passengerId2, FullName = "سارة محمد" };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(bookingId1)).ReturnsAsync(booking1);
        _mockBookingRepo.Setup(r => r.GetByIdAsync(bookingId2)).ReturnsAsync(booking2);
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId1)).ReturnsAsync(passenger1);
        _mockPassengerRepo.Setup(r => r.GetByIdAsync(passengerId2)).ReturnsAsync(passenger2);

        var createDto = new CreateSafeTransactionDto
        {
            TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = 5000m,
            TransactionType = TransactionType.Deposit,
            Currency = Currency.EGP,
            ExchangeRate = 1.0m,
            DepositorOrWithdrawerName = "وسيط الدفع المشترك",
            BookingIds = new List<Guid> { bookingId1, bookingId2 },
            BookingPayments = new Dictionary<Guid, decimal>
            {
                { bookingId1, 3000m },
                { bookingId2, 2000m }
            },
            Description = "سداد دفعة مشتركة للمجموعة والفرد"
        };

        // Act
        var result = await _safeService.CreateTransactionAsync(createDto);

        // Assert
        Assert.NotNull(result);
        _mockSafeRepo.Verify(r => r.AddAsync(It.Is<SafeTransaction>(t => 
            t.Amount == 5000m && 
            t.Currency == Currency.EGP && 
            t.TransactionType == TransactionType.Deposit && 
            t.AssociatedPartnerId == partnerId && 
            t.DepositorOrWithdrawerName == "GRP-123 / B-001002" &&
            t.Description == "سداد دفعة مشتركة للمجموعة والفرد")), Times.Once);
    }
}
