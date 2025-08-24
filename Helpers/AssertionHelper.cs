using System.Net;
using NUnit.Framework;
using RestfulBookerTests.Models;

namespace RestfulBookerTests.Helpers
{
    public static class AssertionHelper
    {
        public static void AssertStatusCode(HttpStatusCode actual, int expected)
        {
            Assert.That((int)actual, Is.EqualTo(expected),
                $"Expected status code {expected}, but got {(int)actual}");
        }

        public static void AssertResponseTime(long elapsedMs, int maxMilliseconds)
        {
            Assert.That(elapsedMs, Is.LessThanOrEqualTo(maxMilliseconds),
                $"Response time {elapsedMs}ms exceeded {maxMilliseconds}ms");
        }

        public static void AssertBookingEquality(Booking expected, Booking actual)
        {
            Assert.Multiple(() =>
            {
                Assert.That(actual.Firstname, Is.EqualTo(expected.Firstname), "Firstname does not match");
                Assert.That(actual.Lastname, Is.EqualTo(expected.Lastname), "Lastname does not match");
                Assert.That(actual.Totalprice, Is.EqualTo(expected.Totalprice), "Total price does not match");
                Assert.That(actual.Depositpaid, Is.EqualTo(expected.Depositpaid), "Deposit paid does not match");
                Assert.That(actual.Bookingdates.Checkin, Is.EqualTo(expected.Bookingdates.Checkin), "Check-in date does not match");
                Assert.That(actual.Bookingdates.Checkout, Is.EqualTo(expected.Bookingdates.Checkout), "Check-out date does not match");
                Assert.That(actual.Additionalneeds, Is.EqualTo(expected.Additionalneeds), "Additional needs do not match");
            });
        }
    }
}
