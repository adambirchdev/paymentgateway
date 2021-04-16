# Checkout.com Payment Gateway and Mock Implementation

The purpose of this application is to create a payment gateway implementation that receives a request for a payment, processes and returns a response dependent on the success.

## Bank simulator

The bank simulator is a purely a controller determining whether a transaction is successful. This is merely determined by whether the amount is exactly divisible by 5. As this is a simple simulator there is no validation on the request.

[POST] http://checkoutinterviewbank.uksouth.azurecontainer.io/Bank

This requires the following [POST] request:
~~~json
{
  "merchantId": "string",
  "expiryMonthYear": "string",
  "currencyCode": "string",
  "cardNumber": "string",
  "cvv": "string",
  "amount": 0
}
~~~

## Payment gateway

The payment gateway has 3 endpoints: getting the token, making a payment, and retrieving a transaction.

### Getting the token

The following request is a pretty elementary implementation to create a jwt token based on the provided merchant name.

[GET] http://checkoutinterviewapi.uksouth.azurecontainer.io/api/Merchant?name=[merchantname]

### Making the payment

*The following request requires the Authorization Bearer [JWT] header supplying received from the Getting the token request.*

This endpoint with actually make the payment. Using a correlation id to ensure the indempotency. The request will update the database with an initialised pending payment. The payment will then be sent to the bank. Once the bank returns the transaction id and status, the record will be updated in the database. Returning the status and the transaction id.

[POST] http://checkoutinterviewapi.uksouth.azurecontainer.io/api/Payment


~~~json
// Request
{
  "expiryMonthYear": "10/30", // must be in this format
  "currencyCode": "GBP", // must be 3 chars
  "cardNumber": "4444333322221111", //card number decides which client to use
  "cvv": "123", // must be 3 digits
  "amount": 9,
  "correlationId": "uniquestring" // subsequent requests with the same correlation id will fail unless payment still pending
}

//Response
{
  "bankTransactionId": "2be09329-f0c9-443f-a7d7-e518f6003ba1",
  "status": 2,
  "reason": "Success"
}
~~~

### Retrieving a transaction

*The following request requires the Authorization Bearer [JWT] header supplying received from the Getting the token request.*

The final endpoint is retrieve the the transaction details as specified when processing the transaction. The bank transaction id is that returned when making the payment.

[GET] http://checkoutinterviewapi.uksouth.azurecontainer.io/api/Payment?transactionId=[banktransactionid]

~~~json
// Response
{
  "expiryMonthYear": "10/30",   // The expiry month and year 
  "cardNumber": "************1111", // The masked card details, always showing last 4 digits
  "currencyCode": "GBP", // The currency code
  "amount": 9, // The amount for the transaction
  "status": 2,  // The status of the transaction
  "reason": "success"
}
~~~

# Deployment

The application is deployed to Azure in containers. This is done by Github actions that run when pushed to master. The CI actions also run the unit tests associated with the solution. 

# Data storage and encryption

The application uses an Azure sql instance and uses AES encryption by using data annotations on the data models, so that any sensitive fields are replaced.

# Api Client

There is an api client project that allows you to hit the payment controller. The whole idea is that the token would already be retrieved and then set when initialising the HttpClient in the consumer.

# Logging and metrics

Application insights has been added, using ILogger and ApplicationInsightsTelemetry, with the instrumentation key also pointing to Azure.