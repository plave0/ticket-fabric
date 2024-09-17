using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace AvailabilityTracker.Interfaces
{

    public interface IAvailabilityTracker : IActor
    {

        Task SetCount(ushort amoutn);

        Task<UInt16> GetCount();

        Task<string> Reserve(ushort amountToReserve);

        Task Buy(string reservationKey);
    }
}
