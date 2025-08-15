Feature: Booking Management
  Scenario: Create a new booking
    Given I have valid booking details
    When I send a create booking request
    Then the response status should be 200
    And the booking ID should be returned

  Scenario: Get booking by ID
    Given I have valid booking details
    When I send a create booking request
    And I send a get booking request for that ID
    Then the response status should be 200
    And the booking details should match what I created
