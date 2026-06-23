using EventHub.Core.Contracts;
using EventHub.Core.Services;
using EventHub.Infrastructure.Data.Common;
using EventHub.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using EventHub.Core.Models.Ticket;
using System.Threading.Tasks;
using Xunit;

using DataEvent = EventHub.Infrastructure.Data.Models.Event;

namespace EventHub.Tests.Unit.TicketsService
{
    public class TicketServiceTests
    {
        private readonly Mock<IRepository> _repoMock;
        private readonly Mock<IQRCodeService> _qrCodeServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly TicketService _ticketService;

        public TicketServiceTests()
        {
            _repoMock = new Mock<IRepository>();
            _qrCodeServiceMock = new Mock<IQRCodeService>();
            
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            context.Request.Scheme = "http";
            context.Request.Host = new HostString("localhost");
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            _ticketService = new TicketService(_repoMock.Object, _qrCodeServiceMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task PurchaseAsync_WithValidData_ReturnsTicketIdsAndUpdatesEvent()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            int quantityToBuy = 2;

            var dataEvent = new DataEvent
            {
                Id = eventId,
                EventName = "Test Event",
                IsActive = true,
                TotalTickets = 100,
                TicketsSold = 50,
                BasePrice = 10.0m
            };

            var events = new List<DataEvent> { dataEvent };
            _repoMock.Setup(r => r.All<DataEvent>()).Returns(events.AsQueryable().BuildMock());

            var tickets = new List<Ticket>();
            _repoMock.Setup(r => r.AllReadonly<Ticket>()).Returns(tickets.AsQueryable().BuildMock());

            // Act
            var result = await _ticketService.PurchaseAsync(eventId, userId, quantityToBuy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(quantityToBuy, result.Count);

            // Verify event updated
            Assert.Equal(52, dataEvent.TicketsSold);
            _repoMock.Verify(r => r.Update(dataEvent), Times.Once);
            
            // Verify tickets added
            _repoMock.Verify(r => r.AddAsync(It.Is<Ticket>(t => t.EventId == eventId && t.UserId == userId)), Times.Exactly(quantityToBuy));
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task PurchaseAsync_EventNotFound_ReturnsEmptyList()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var events = new List<DataEvent>(); // Empty list
            _repoMock.Setup(r => r.All<DataEvent>()).Returns(events.AsQueryable().BuildMock());

            // Act
            var result = await _ticketService.PurchaseAsync(eventId, userId, 1);

            // Assert
            Assert.Empty(result);
            _repoMock.Verify(r => r.Update(It.IsAny<DataEvent>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Ticket>()), Times.Never);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task PurchaseAsync_NotEnoughCapacity_ReturnsEmptyList()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            int quantityToBuy = 5;

            var dataEvent = new DataEvent
            {
                Id = eventId,
                EventName = "Sold Out Event",
                IsActive = true,
                TotalTickets = 10,
                TicketsSold = 8 // Only 2 left
            };

            var events = new List<DataEvent> { dataEvent };
            _repoMock.Setup(r => r.All<DataEvent>()).Returns(events.AsQueryable().BuildMock());

            // Act
            var result = await _ticketService.PurchaseAsync(eventId, userId, quantityToBuy);

            // Assert
            Assert.Empty(result);
            _repoMock.Verify(r => r.Update(It.IsAny<DataEvent>()), Times.Never);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Ticket>()), Times.Never);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task ReserveSeatsAsync_WithAvailableSeats_ReturnsSuccess()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var seat1Id = Guid.NewGuid();
            var seat2Id = Guid.NewGuid();
            var seatIds = new List<Guid> { seat1Id, seat2Id };

            var dataEvent = new DataEvent
            {
                Id = eventId,
                RoomId = roomId,
                BasePrice = 10.0m,
                IsActive = true
            };
            var events = new List<DataEvent> { dataEvent };
            _repoMock.Setup(r => r.AllReadonly<DataEvent>()).Returns(events.AsQueryable().BuildMock());

            var seats = new List<Seat>
            {
                new Seat { Id = seat1Id, RoomId = roomId, SeatNumber = 1, IsActive = true },
                new Seat { Id = seat2Id, RoomId = roomId, SeatNumber = 2, IsActive = true }
            };
            _repoMock.Setup(r => r.AllReadonly<Seat>()).Returns(seats.AsQueryable().BuildMock());

            var tickets = new List<Ticket>(); // No conflicting tickets
            _repoMock.Setup(r => r.AllReadonly<Ticket>()).Returns(tickets.AsQueryable().BuildMock());

            var pricingTiers = new List<EventPricingTier>();
            _repoMock.Setup(r => r.AllReadonly<EventPricingTier>()).Returns(pricingTiers.AsQueryable().BuildMock());

            // Act
            var result = await _ticketService.ReserveSeatsAsync(eventId, userId, seatIds);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.TicketIds.Count);
            Assert.Equal(20.0m, (decimal)result.TotalPrice);

            _repoMock.Verify(r => r.AddAsync(It.Is<Ticket>(t => t.EventId == eventId && t.UserId == userId && t.Status == TicketStatus.Reserved)), Times.Exactly(2));
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ReserveSeatsAsync_WithAlreadyReservedSeats_ReturnsError()
        {
            // Arrange
            var eventId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var seat1Id = Guid.NewGuid(); // Available
            var seat2Id = Guid.NewGuid(); // Already reserved
            var seatIds = new List<Guid> { seat1Id, seat2Id };

            var dataEvent = new DataEvent
            {
                Id = eventId,
                RoomId = roomId,
                BasePrice = 10.0m,
                IsActive = true
            };
            var events = new List<DataEvent> { dataEvent };
            _repoMock.Setup(r => r.AllReadonly<DataEvent>()).Returns(events.AsQueryable().BuildMock());

            var seats = new List<Seat>
            {
                new Seat { Id = seat1Id, RoomId = roomId, SeatNumber = 1, IsActive = true },
                new Seat { Id = seat2Id, RoomId = roomId, SeatNumber = 2, IsActive = true }
            };
            _repoMock.Setup(r => r.AllReadonly<Seat>()).Returns(seats.AsQueryable().BuildMock());

            var conflictingTicket = new Ticket
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                SeatId = seat2Id,
                Status = TicketStatus.Reserved,
                ReservationExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
            var tickets = new List<Ticket> { conflictingTicket };
            _repoMock.Setup(r => r.AllReadonly<Ticket>()).Returns(tickets.AsQueryable().BuildMock());

            // Act
            var result = await _ticketService.ReserveSeatsAsync(eventId, userId, seatIds);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("Some of the seats you picked were just taken", result.ErrorMessage);

            _repoMock.Verify(r => r.AddAsync(It.IsAny<Ticket>()), Times.Never);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task AdminRefundTicketAsync_WithValidTicket_ReturnsTrueAndUpdatesStatus()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var processedBy = Guid.NewGuid();
            var pricingTierId = Guid.NewGuid();

            var ticket = new Ticket
            {
                Id = ticketId,
                Status = TicketStatus.Purchased,
                PricingTierId = pricingTierId
            };
            var tickets = new List<Ticket> { ticket };
            _repoMock.Setup(r => r.All<Ticket>()).Returns(tickets.AsQueryable().BuildMock());

            var paymentTickets = new List<PaymentTicket>();
            _repoMock.Setup(r => r.AllReadonly<PaymentTicket>()).Returns(paymentTickets.AsQueryable().BuildMock());

            var payments = new List<Payment>();
            _repoMock.Setup(r => r.All<Payment>()).Returns(payments.AsQueryable().BuildMock());

            var tier = new EventPricingTier
            {
                Id = pricingTierId,
                SoldQuantity = 5
            };
            var tiers = new List<EventPricingTier> { tier };
            _repoMock.Setup(r => r.All<EventPricingTier>()).Returns(tiers.AsQueryable().BuildMock());

            // Act
            var result = await _ticketService.AdminRefundTicketAsync(ticketId, processedBy);

            // Assert
            Assert.True(result);
            Assert.Equal(TicketStatus.Refunded, ticket.Status);
            Assert.Equal(4, tier.SoldQuantity);

            _repoMock.Verify(r => r.Update(ticket), Times.Once);
            _repoMock.Verify(r => r.Update(tier), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AdminRefundTicketAsync_TicketNotFound_ReturnsFalse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var processedBy = Guid.NewGuid();

            var tickets = new List<Ticket>();
            _repoMock.Setup(r => r.All<Ticket>()).Returns(tickets.AsQueryable().BuildMock());

            // Act
            var result = await _ticketService.AdminRefundTicketAsync(ticketId, processedBy);

            // Assert
            Assert.False(result);
            _repoMock.Verify(r => r.Update(It.IsAny<Ticket>()), Times.Never);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task AdminRefundTicketAsync_AlreadyRefunded_ReturnsFalse()
        {
            // Arrange
            var ticketId = Guid.NewGuid();
            var processedBy = Guid.NewGuid();

            var ticket = new Ticket
            {
                Id = ticketId,
                Status = TicketStatus.Refunded
            };
            var tickets = new List<Ticket> { ticket };
            _repoMock.Setup(r => r.All<Ticket>()).Returns(tickets.AsQueryable().BuildMock());

            // Act
            var result = await _ticketService.AdminRefundTicketAsync(ticketId, processedBy);

            // Assert
            Assert.False(result);
            _repoMock.Verify(r => r.Update(It.IsAny<Ticket>()), Times.Never);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetUserTicketsAsync_WithExistingTickets_ReturnsViewModelList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var tickets = new List<Ticket>
            {
                new Ticket { Id = Guid.NewGuid(), UserId = userId, EventId = eventId, TicketNumber = 12345 },
                new Ticket { Id = Guid.NewGuid(), UserId = userId, EventId = eventId, TicketNumber = 12346 }
            };
            _repoMock.Setup(r => r.AllReadonly<Ticket>()).Returns(tickets.AsQueryable().BuildMock());

            var events = new List<DataEvent>
            {
                new DataEvent { Id = eventId, EventName = "My Concert", RoomId = roomId, StartDateTime = DateTime.UtcNow }
            };
            _repoMock.Setup(r => r.AllReadonly<DataEvent>()).Returns(events.AsQueryable().BuildMock());

            var rooms = new List<Room>
            {
                new Room { RoomId = roomId, Name = "Main Hall" }
            };
            _repoMock.Setup(r => r.AllReadonly<Room>()).Returns(rooms.AsQueryable().BuildMock());

            // Act
            var result = await _ticketService.GetUserTicketsAsync(userId);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            
            Assert.Equal("My Concert", resultList[0].EventName);
            Assert.Equal("Main Hall", resultList[0].RoomName);
            Assert.Equal(12345, resultList[0].TicketNumber);
        }
    }
}
