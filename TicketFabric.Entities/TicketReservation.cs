using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TicketFabric.Entities
{
    [DataContract]
    public class TicketReservation
    {
        [DataMember]
        public string TripId { get; set; }
        [DataMember]
        public ushort Amount { get; set; }

        public TicketReservation(string tripId, ushort amount)
        {
            TripId = tripId;
            Amount = amount;
        }
    }
}
