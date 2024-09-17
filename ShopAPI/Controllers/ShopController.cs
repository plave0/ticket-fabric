using AvailabilityTracker.Interfaces;
using Microsoft.AspNetCore.Mvc;
using TicketFabric.Exceptions;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using TicketFabric.Entities;

namespace ShopAPI.Controllers
{
    [ApiController]
    public class ShopController : ControllerBase
    {

        [HttpPost]
        [Route("/trips/reserve")]
        public async Task<IActionResult> Reserve([FromBody] TicketReservation reservation)
        {

            var availabilityTracker = ActorProxy.Create<IAvailabilityTracker>(
                new ActorId(reservation.TripId),
                new Uri("fabric:/TicketFabric/AvailabilityTrackerActorService")
            );

            string reservationKey = await availabilityTracker.Reserve(reservation.Amount);

            if (reservationKey != "0")
            {
                return Ok(new string[] {reservation.TripId, reservationKey});
            }
            else
            {
                return BadRequest("invalid reservation");
            }

        }

        [HttpPost]
        [Route("/trips/buy")]
        public async Task<IActionResult> Buy([FromBody] TicketPurchase purchase)
        {
            var availabilityTracker = ActorProxy.Create<IAvailabilityTracker>(
                new ActorId(purchase.TripId),
                new Uri("fabric:/TicketFabric/AvailabilityTrackerActorService")
            );

            try
            {
                await availabilityTracker.Buy(purchase.ReservationKey);
                return Ok();
            }
            catch (AggregateException e)
            {
                return BadRequest("Reservation expired");
            }
        }
    }
}
