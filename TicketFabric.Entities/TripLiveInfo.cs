using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TicketFabric.Entities
{
    [DataContract]
    public class TripLiveInfo
    {
        [DataMember]
        public string TripId {  get; set; }
        [DataMember]
        public Trip Trip { get; set; }
        [DataMember]
        public ushort AvailableSeats { get; set; }
        
        public TripLiveInfo(string tripId, Trip trip, ushort availableSeats)
        {
            TripId = tripId;
            Trip = trip;
            AvailableSeats = availableSeats;
        }
    }
}
