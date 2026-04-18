using EventHub.Core.Models.Ticket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventHub.Core.Contracts
{
    public interface ITicketService
    {
        Task<List<Guid>> PurchaseAsync(Guid eventId, Guid userId, int quantity);
        Task<IEnumerable<TicketListViewModel>> GetUserTicketsAsync(Guid userId);
        Task<TicketDetailViewModel?> GetTicketByIdAsync(Guid ticketId, Guid userId);
    }
}