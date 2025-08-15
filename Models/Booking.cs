namespace RestfulBookerTests.Models
{
    public class Booking
    {
        public required string Firstname { get; set; }
        public required string Lastname { get; set; }
        public int Totalprice { get; set; }
        public bool Depositpaid { get; set; }
        public required BookingDates Bookingdates { get; set; }
        public string? Additionalneeds { get; set; }
    }
}
