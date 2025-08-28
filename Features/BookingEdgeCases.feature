Feature: Booking Management - Edge Cases

Scenario Outline: Create booking with multiple valid payloads
	Given I have a booking with:
		| firstname   | lastname   | totalprice   | depositpaid   | checkin   | checkout   | additionalneeds   |
		| <firstname> | <lastname> | <totalprice> | <depositpaid> | <checkin> | <checkout> | <additionalneeds> |
	When I send a create booking request
	Then the response status should be 200
	And the booking ID should be returned
	And the booking details should match what I created

Examples:
	| firstname | lastname | totalprice | depositpaid | checkin    | checkout   | additionalneeds |
	| Alice     | Green    | 100        | true        | 2025-08-10 | 2025-08-15 | Breakfast       |
	| Bob       | White    | 300        | false       | 2025-09-01 | 2025-09-05 | Dinner          |
	| Carol     | Black    | 0          | true        | 2025-10-01 | 2025-10-03 | None            |

Scenario Outline: Create booking with edge case values
	Given I have a booking with:
		| firstname   | lastname   | totalprice   | depositpaid   | checkin   | checkout   | additionalneeds   |
		| <firstname> | <lastname> | <totalprice> | <depositpaid> | <checkin> | <checkout> | <additionalneeds> |
	When I send a create booking request
	Then the response status should be <expectedStatus>

Examples:
	| firstname | lastname | totalprice | depositpaid | checkin    | checkout   | additionalneeds | expectedStatus |
	| Max       | Price    | 1000000    | true        | 2025-08-01 | 2025-08-10 | VIP             | 200            |
	| Zero      | Price    | 0          | false       | 2025-08-01 | 2025-08-02 | None            | 200            |
	| BadDates  | User     | 100        | true        | 2025-08-10 | 2025-08-01 | Breakfast       | 200            |
	| Special   | Chars    | 250        | true        | 2025-09-05 | 2025-09-10 | !@#$%^&*()      | 200            |
