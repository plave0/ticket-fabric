using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TicketFabric.Entities
{
    [DataContract]
    public class TicketPurchase
    {
        [DataMember]
        public string TripId {  get; set; }
        [DataMember]
        public string ReservationKey {  get; set; }

        public TicketPurchase()
        {
            TripId = string.Empty;
            ReservationKey = string.Empty;
        }
    }
}
