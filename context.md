Paytently Take Home Technical Test

Create a Payment Gateway
Payment Gateways are the cornerstone of processing payments online, with a number of
different functions enabling merchants to offer products and services.
A shopper makes a payment on a Merchant’s website, the Merchant sends the payment to a
Payment Gateway and the Gateway sends the payment to an Acquirer.
Paytently acts as a Payment Gateway in this flow, accepting payments from Merchants and
sending them to a variety of acquirers.

Objectives
Build a Payment Gateway API which can process a payment and retrieve the status of a
processed payment, using a simulated acquirer
Requirements
Given a payment is created by a merchant
When the request is sent to the Payment Gateway
Then the Payment Gateway should respond with a success or failure
Given a valid payment ID
When a status request is sent to the Payment Gateway
Then the Payment Gateway responds with the status of the payment

Technical Details
A payment request should include data relevant to a payment, including (but not
limited to): amount, currency, card number, expiry month/date, cvv.
Please note that the acquirer should be mocked out in the solution, with the ability to
swap to an actual acquirer in the future. This acquirer should be able to simulate
successful or failed transactions to the Gateway.
When retrieving a payment the card number and other sensitive information should
be masked and return the status of the payment

Additional Bonuses
Logging, Metrics, Documentation, Authentication, Containerisation, Testing, Build Scripts,
anything else you’d like to show off!
Notes

All code must be written in C# using .NET Core
The solution should document how it can be run, such as using dotnet docker compose, so that an engineer can review the test
Please share code in a public repository in Github
