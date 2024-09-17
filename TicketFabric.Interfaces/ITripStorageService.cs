using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using TicketFabric.Entities;

namespace TicketFabric.Interfaces
{
    public interface ITripStorageService : IService
    { 
        Task<Trip> GetTrip(string tripId);
        Task<IEnumerable<Trip>> GetTrips();
        Task AddTrip(Trip trip);
        Task DeleteTrip(string tripId);
    }
}
