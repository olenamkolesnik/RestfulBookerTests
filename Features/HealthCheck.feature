Feature: API Healthcheck

  The API should respond to healthcheck requests so that we can verify it is up and running.

  @noauth  
  Scenario: Healthcheck endpoint is available
    When I send a ping request
    Then the response status should be 200
    And the response time should be less than 500 ms