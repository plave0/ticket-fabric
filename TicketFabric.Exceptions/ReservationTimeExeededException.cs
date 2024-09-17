namespace TicketFabric.Exceptions
{
    [Serializable()]
    public class ReservationTimeExeededException : Exception
    {
        public ReservationTimeExeededException(string message) : base(message) { }
    }
}
