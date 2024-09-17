using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

using TicketFabric.Entities;
using TicketFabric.Interfaces;
using AvailabilityTracker.Interfaces;

namespace TripStorage
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TripStorage : StatefulService, ITripStorageService
    {

        private readonly IReliableStateManager mStateManager;

        public TripStorage(StatefulServiceContext context)
            : base(context)
        { }

        public async Task AddTrip(Trip trip)
        {
            var tripStorage = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Trip>>("TripStorage");

            using (var tx = this.StateManager.CreateTransaction())
            {
                await tripStorage.AddAsync(tx, trip.GetKey(), trip);
                await tx.CommitAsync();
            }

            var seatCountActor = ActorProxy.Create<IAvailabilityTracker>(
                new ActorId(trip.GetKey()), 
                new Uri("fabric:/TicketFabric/AvailabilityTrackerActorService"));

            await seatCountActor.SetCount(trip.TotalSeatsAvailable);
        }

        public async Task<Trip> GetTrip(string tripId)
        {
            var tripStorage = await StateManager.GetOrAddAsync<IReliableDictionary<string, Trip>>("TripStorage");

            using (var tx = StateManager.CreateTransaction())
            {
                var returnVal = await tripStorage.TryGetValueAsync(tx, tripId);
                await tx.CommitAsync();

                if (returnVal.HasValue)
                { 
                    return returnVal.Value;
                }
                else
                {
                    return new Trip();
                }
            }
                
        }

        public async Task<IEnumerable<Trip>> GetTrips()
        {
            var tripStorage = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Trip>>("TripStorage");

            List<Trip> tripList = new List<Trip>();
            Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, Trip>> tripEnumerable;

            using (var tx = this.StateManager.CreateTransaction())
            {
                tripEnumerable = await tripStorage.CreateEnumerableAsync(tx);
                await tx.CommitAsync();
            }

            var tripEnumerator = tripEnumerable.GetAsyncEnumerator();
            while (await tripEnumerator.MoveNextAsync(new CancellationToken()))
            {
                tripList.Add(tripEnumerator.Current.Value);
            }

            return tripList;
        }

        public async Task DeleteTrip(string tripId)
        {
            var tripStorage = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Trip>>("TripStorage");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var deletedEntry = await tripStorage.TryRemoveAsync(tx, tripId);
                await tx.CommitAsync();
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }
           
        protected override async Task<bool> OnDataLossAsync(RestoreContext restoreCtx, CancellationToken cancellationToken)
        {

            ServiceEventSource.Current.Message("Backup restore triggered");
            await Task.Delay(1000);
            return true;
        }

    }
}
