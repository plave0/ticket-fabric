using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

using TicketFabric.Entities;
using TicketFabric.Interfaces;
using AvailabilityTracker.Interfaces;

namespace TripAPI.Controllers
{
    [ApiController]
    public class TripController : ControllerBase
    {

        [HttpGet]
        [Route("/trips/search")]
        public async Task<IActionResult> Search()
        {
            var tripStorage = ServiceProxy.Create<ITripStorageService>(
                new Uri("fabric:/TicketFabric/TripStorage"),
                new ServicePartitionKey(1));

            IEnumerable<Trip> trips = await tripStorage.GetTrips();

            List<TripLiveInfo> tripInfos = new List<TripLiveInfo>();

            foreach (Trip trip in trips)
            {

                var availabilityTracket = ActorProxy.Create<IAvailabilityTracker>(
                    new ActorId(trip.GetKey()),
                    new Uri("fabric:/TicketFabric/AvailabilityTrackerActorService"));

                tripInfos.Add(new TripLiveInfo(
                    trip.GetKey(),
                    trip, 
                    await availabilityTracket.GetCount()
                    ));
            }

            return Ok(tripInfos);
        }
        
        [HttpPost]
        [Route("/trips/add")]
        public async Task<IActionResult> Add([FromBody] Trip trip)
        {

            var tripStorage = ServiceProxy.Create<ITripStorageService>(
                new Uri("fabric:/TicketFabric/TripStorage"),
                new ServicePartitionKey(1));

            await tripStorage.AddTrip(trip);

            return Ok("add");
        }

        [HttpDelete]
        [Route("/trips/delete")]
        public async Task<IActionResult> Delete([FromBody] string tripId)
        {
            var tripStorage = ServiceProxy.Create<ITripStorageService>(
                new Uri("fabric:/TicketFabric/TripStorage"),
                new ServicePartitionKey(1));

            var availabilityTracketServiceProxy = ActorServiceProxy.Create(
                    new Uri("fabric:/TicketFabric/AvailabilityTrackerActorService"),
                    new ActorId(tripId));

            await tripStorage.DeleteTrip(tripId);
            await availabilityTracketServiceProxy.DeleteActorAsync(new ActorId(tripId), new CancellationToken());

            return Ok("delete");
        }
    }
}
