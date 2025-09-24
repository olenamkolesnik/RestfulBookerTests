using RestfulBookerTests.Models;

namespace RestfulBookerTests.Helpers
{
    public class DbAssertionHelper
    {
        public static void AssertBookingInDb(Booking expected, Booking actual)
        {
            Assert.That(actual.Firstname, Is.EqualTo(expected.Firstname));
            Assert.That(actual.Lastname, Is.EqualTo(expected.Lastname));
            Assert.That(actual.Totalprice, Is.EqualTo(expected.Totalprice));
        }
    }
}
