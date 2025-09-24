# **RestfulBooker Test Automation Framework**

![.NET](https://img.shields.io/badge/.NET-9.0-blue)  
![Tests](https://img.shields.io/github/actions/workflow/status/olenamkolesnik/RestfulBookerTests/test-allure.yml?label=Tests&logo=github)  
[![Allure Report](https://img.shields.io/badge/Allure-Report-brightgreen?style=flat&logo=allure)](https://olenamkolesnik.github.io/RestfulBookerTests/)

---

## **📌 Overview**

This repository contains an automated testing framework for **Restful Booker API** built with:

- ✅ **.NET 9**
- ✅ **Reqnroll** (SpecFlow alternative for BDD in .NET)
- ✅ **NUnit** for assertions
- ✅ **RestSharp** for API requests
- ✅ **Allure** for rich test reporting
- ✅ **GitHub Actions** for CI/CD and report publishing

It follows a **Behavior-Driven Development (BDD)** approach to ensure readable and maintainable tests.

---

## **✨ Features**

✔ **API Test Automation** for Restful Booker API  
✔ **BDD Style Scenarios** using Reqnroll (`.feature` files)  
✔ **Configurable via appsettings.json & .env**  
✔ **Allure Reporting** with GitHub Pages integration  
✔ **Secrets Management** for credentials via GitHub Secrets  
✔ **CI/CD Pipeline** with GitHub Actions  

---

## **📂 Project Structure**
```
RestfulBookerTests/
├── Features/ # Reqnroll feature files (.feature)
├── Steps/ # Step definitions for BDD
├── Hooks/ # Test lifecycle hooks
├── Models/ # Request/Response models
├── TestData/ # Test data files
├── Clients/ # API client classes (BaseClient, BookingClient)
├── DB/ # Database clients (DbClient, InMemoryDbClient)
├── Extensions/ # ScenarioContext extensions
├── Helpers/ # Assertion, logging, JSON, schema helpers
├── appsettings.json # App configuration
├── .env # Local environment variables
└── allureConfig.json # Allure configuration
```

---

## **🛠 Technologies**

- **Language:** C# (.NET 9)
- **Testing Framework:** NUnit
- **BDD Tool:** Reqnroll
- **HTTP Client:** RestSharp
- **Reporting:** Allure
- **CI/CD:** GitHub Actions

---

## **🚀 Getting Started**

### ✅ **Prerequisites**
- Install [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Install **Reqnroll CLI**  
  ```bash
  dotnet tool install --global Reqnroll.Tools
  ```

### ✅ Clone the Repository
git clone https://github.com/olenamkolesnik/RestfulBookerTests.git
cd RestfulBookerTests

### ✅ Set Environment Variables

Create a .env file:
```
USERNAME=***
PASSWORD=***
LOGLEVEL=INFO
```
Or set them via GitHub Secrets for CI/CD.

### ✅ Run Tests
```
dotnet test
```

### ✅ Generate Allure Report (Locally)
```
allure serve ./RestfulBookerTests/bin/Debug/net9.0/allure-results
```

---
## **📊 Allure Report**
After each pipeline run, the **latest Allure Report** is published to **GitHub Pages**:

👉 **[View Allure Report](https://olenamkolesnik.github.io/RestfulBookerTests/)**

---

## **⚙️ Continuous Integration**
GitHub Actions workflow:
- Runs tests on every **push** and **pull request**
- Generates **Allure Report**
- Publishes report to **GitHub Pages**

Workflow file: [`.github/workflows/test-allure.yml`](.github/workflows/test-allure.yml)

---

## **🔒 Secrets**
Add the following in **GitHub → Settings → Secrets and Variables → Actions**:
- `USERNAME` = ***
- `PASSWORD` = ***

---

## **📌 Example BDD Test**

### **Feature File (`Booking.feature`):**
```
Feature: Booking Management
  In order to manage hotel bookings
  As an API client
  I want to create and retrieve bookings

Scenario: Create a new booking
  Given I have a valid booking payload
  When I send a POST request to create a booking
  Then the response status code should be 200
  And the response should contain a booking id
```
### **Step Definition (`BookingSteps.cs`):**
```
[Binding]
public class BookingSteps
{
    private readonly ScenarioContext _context;
    private RestResponse _response;

    public BookingSteps(ScenarioContext context)
    {
        _context = context;
    }

    [Given(@"I have a valid booking payload")]
    public void GivenIHaveAValidBookingPayload()
    {
       var booking = new Booking
      {
          Firstname = "John",
          Lastname = "Doe",
          Totalprice = 150,
          Depositpaid = true,
          Bookingdates = new BookingDates { Checkin = "2025-09-01", Checkout = "2025-09-10" }
      };
      _context.SetData(ScenarioKeys.CurrentBooking, booking);
    }

    [When(@"I send a POST request to create a booking")]
    public async Task WhenISendAPostRequestToCreateABooking()
    {
        var client = new RestClient("https://restful-booker.herokuapp.com/");
        var request = new RestRequest("booking", Method.Post);
        request.AddJsonBody(_context.GetData<Booking>(ScenarioKeys.CurrentBooking));
        _response = await client.ExecuteAsync(request);
    }

    [Then(@"the response status code should be (.*)")]
    public void ThenTheResponseStatusCodeShouldBe(int expectedStatus)
    {
        Assert.AreEqual(expectedStatus, (int)_response.StatusCode);
    }

    [Then(@"the response should contain a booking id")]
    public void ThenTheResponseShouldContainABookingId()
    {
        var json = JsonDocument.Parse(_response.Content);
        Assert.IsTrue(json.RootElement.TryGetProperty("bookingid", out _));
    }
}
```
---
## **📄 License**
This project is licensed under the **MIT License**.

---

## **📊 Architecture Diagram**

```mermaid
flowchart TD
    A[Reqnroll Feature Files] --> B[Step Definitions in C#]
    B --> C[NUnit Test Execution]
    C --> D[Allure Results]
    D --> E[GitHub Actions Workflow]
    E --> F[Build Allure Report]
    F --> G[Publish to GitHub Pages]
    G --> H[View Report Online]

