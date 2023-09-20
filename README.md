Project Documentation

1. Postman Collection
Location: The Postman collection for this project can be found in the Postman directory.

2. Running Tests
Command: To conduct the tests, utilize the command:
dotnet test

3. Launching the Server
Command: Start the server using:
dotnet run

4. Registering a New Customer
Endpoint: {{baseUrl}}/api/customers
HTTP Method: POST
Copy code
{
    "Title": "Dr",
    "Forename": "Sam",
    "Surname": "Brown",
    "EmailAddress": "sam.brown@example.com",
    "MobileNo": "1122334455",
    "IsActive": true,
    "Addresses": [
        {
            "AddressLine1": "123 Sample St",
            "AddressLine2": "Suite 456",
            "Town": "Sample Town",
            "County": "Sample County",
            "Postcode": "12345",
            "Country": "Sample Country",
            "IsMain": true
        },
        {
            "AddressLine1": "789 Another St",
            "AddressLine2": "Apt 101",
            "Town": "Another Town",
            "County": "Another County",
            "Postcode": "67890",
            "Country": "Another Country",
            "IsMain": false
        }
    ]
}