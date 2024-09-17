using System.Runtime.Serialization;

namespace TicketFabric.Entities
{
    [DataContract]
    public class Trip
    {

        [DataMember]
        public string DepartureStation { get; set; }
        [DataMember]
        public string DestinationStation { get; set; }
        [DataMember]
        public DateTime DepartureTime { get; set; }
        [DataMember]
        public DateTime ArrivalTime { get; set; }
        [DataMember]
        public UInt16 TotalSeatsAvailable { get; set; }

        public Trip()
        {
            DepartureStation = string.Empty;
            DestinationStation = string.Empty;
            DepartureTime = DateTime.Now;
            ArrivalTime = DateTime.Now;
            TotalSeatsAvailable = 0;
        }

        public Trip(string departureStation, string destinationStation, DateTime departureTime, DateTime arrivalTime, UInt16 totalSeatsAvailable)
        {
            DepartureStation = departureStation;
            DestinationStation = destinationStation;
            DepartureTime = departureTime;
            ArrivalTime = arrivalTime;
            TotalSeatsAvailable = totalSeatsAvailable;
        }

        public string GetKey()
        {
            return $"t-{DepartureStation}-{DestinationStation}-{DepartureTime.ToString("yyyy-MM-dd-HH-mm")}";
        }
    }
}
