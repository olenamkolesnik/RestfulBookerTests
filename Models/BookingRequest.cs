namespace RestfulBookerTests.Models
{
    public class BookingRequest
    {
        public string CustomerName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
    }
}
