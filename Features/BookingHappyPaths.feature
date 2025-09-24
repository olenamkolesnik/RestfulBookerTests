Feature: Booking Management - Happy Paths

Background:
	Given I have a booking with:
		| firstname | lastname | totalprice | depositpaid | checkin    | checkout   | additionalneeds |
		| John      | Doe      | 150        | true        | 2025-08-25 | 2025-08-30 | Breakfast       |
	When I send a create booking request

Scenario: Create a new booking
	Then the response status should be 200
	And the response time should be less than 1000 ms
	And the booking ID should be returned
	And the booking details should match what I created
	And the response matches the "BookingCreatedResponse" schema
	And the booking should exist in the database

Scenario: Get booking by ID
	When I send a get booking request for that ID
	Then the response status should be 200
	And the response time should be less than 500 ms
	And the booking details should match what I created
	And the response matches the "BookingCreatedResponse" schema

Scenario: Update booking
	Given I have updated booking details with:
		| firstname | lastname | totalprice | depositpaid | checkin    | checkout   | additionalneeds |
		| Jane      | Smith    | 200        | false       | 2025-09-01 | 2025-09-05 | Lunch           |
	When I send an update booking request for that ID
	Then the response status should be 200
	And the response time should be less than 500 ms
	And the booking details should match what I updated
	And the response matches the "BookingCreatedResponse" schema

Scenario: Delete booking
	When I send a delete booking request for that ID
	Then the response status should be 204
	And the response time should be less than 500 ms