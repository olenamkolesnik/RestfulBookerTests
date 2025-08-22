Feature: Booking Management

Background:
	Given I have a booking with:
		| firstname | lastname | totalprice | depositpaid | checkin    | checkout   | additionalneeds |
		| John      | Doe      | 150        | true        | 2025-08-25 | 2025-08-30 | Breakfast       |
	When I send a create booking request

Scenario: Create a new booking   
	Then the response status should be 200
	And the response time should be less than 500 ms
	And the booking ID should be returned
	And the booking details should match what I created

Scenario: Get booking by ID
	When I send a get booking request for that ID
	Then the response status should be 200
	And the response time should be less than 500 ms
	And the booking details should match what I created
