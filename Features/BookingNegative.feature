Feature: Booking Management - Negative Scenarios

Scenario Outline: Create booking with missing or invalid required fields
	Given I have a booking with:
		| firstname   | lastname   | totalprice   | depositpaid   | checkin   | checkout   | additionalneeds   |
		| <firstname> | <lastname> | <totalprice> | <depositpaid> | <checkin> | <checkout> | <additionalneeds> |
	When I send a create booking request
	# According to the documentation all fields are optional
	Then the response status should be 200
	And the response time should be less than 1000 ms

Examples:
	| firstname | lastname | totalprice | depositpaid | checkin    | checkout   | additionalneeds |
	|           | Doe      | 150        | true        | 2025-08-25 | 2025-08-30 | Breakfast       |
	| John      |          | 150        | true        | 2025-08-25 | 2025-08-30 | Breakfast       |
	| John      | Doe      | 150        | true        | 2025-08-30 | 2025-08-25 | Breakfast       |


Scenario: Create booking without authentication
	Given I have a booking with:
		| firstname | lastname | totalprice | depositpaid | checkin    | checkout   | additionalneeds |
		| John      | Doe      | 150        | true        | 2025-08-25 | 2025-08-30 | Breakfast       |
	When I send a create booking request without auth
	# According to the documentation auth is not required for creation
	Then the response status should be 200 
	And the response time should be less than 1000 ms

Scenario: Get booking with invalid ID
	When I send a get booking request for ID 99999
	Then the response status should be 404
	And the response time should be less than 500 ms

Scenario: Update booking with invalid ID
	Given I have updated booking details with:
		| firstname | lastname | totalprice | depositpaid | checkin    | checkout   | additionalneeds |
		| Jane      | Smith    | 200        | false       | 2025-09-01 | 2025-09-05 | Lunch           |
	When I send an update booking request for ID 99999
	# match actual API behavior
	Then the response status should be 405 
	And the response time should be less than 500 ms

Scenario: Delete booking with invalid ID
	When I send a delete booking request for ID 99999
	# match actual API behavior
	Then the response status should be 405
	And the response time should be less than 500 ms