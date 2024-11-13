## Project Overview
The goal of this project is to build a .NET Web API to process Call Data Records (CDRs) in a telecom system. The key requirements include:

1. Developing CRUD endpoints for managing CDRs
2. Implementing charge calculation logic based on predefined rates
3. Providing analytical endpoints to retrieve call summaries and top users
4. Securing the API with API key authorization

## Approach

### 1. API Structure
#### NOTE:  names have changed in the project.
1. Created the following endpoints: 
 
   ## NOTE: I have changed all the names at the end to make it better looking at Swagger
 A. CRD
   - `GET /api/cdr/`: Retrieve all CDRs
   - `GET /api/cdr/{id}`: Retrieve a CDR by ID
   - `POST /api/cdr`: Create a new CDR
   - `PUT /api/cdr/{id}`: Update an existing CDR
   - `DELETE /api/cdr/{id}`: Delete a CDR
   - `GET /api/cdr/calculate-charge/{id}`: Calculate the charge for a CDR
   - `GET /api/cdr/summary/{userId}`: Retrieve call summary for a user
   - `GET /api/cdr/top-users`: Retrieve the top 5 users by call duration or charges
	
 B. User
   - `GET /api/user/`: Retrieve all users
   - `POST /api/user/`: Create a new user




### 2. Database Setup
1. Used Entity Framework Core to set up the database, with the following entities:
   - `User`: Stores user details (ID, name, MSISDN)
   - `CDR`: Stores call data (caller MSISDN, receiver MSISDN, duration, timestamp, type)
2. Implemented MSISDN validation to ensure it matches the E.164 standard format.

### 3. Charge Calculation Logic
1. Implemented a rate-based system for call charges:
   - Local calls: $0.05 per minute
   - Long-distance calls: $0.10 per minute
   - International calls: $0.50 per minute
2. Created the `/api/cdr/calculate-charge` endpoint to calculate the cost for a given CDR as mentioned.

### 4. API Key Authorization
1. Implemented middleware to check the API key's validity before processing requests.
2. If the API key is missing or invalid, the middleware returns a 401 Unauthorized status.
3. The API key is passed in the `x-api-key` header.

### 5. Additional Endpoints
1. `/api/cdr/summary/{userId}`: Returns the total number of calls, total duration, and total charges for a specific user.
2. `/api/cdr/top-users`: Returns a list of the top 5 users by call duration or charges.

### 6. Documentation
1. Used Swagger to document the API endpoints in the code, including examples of the required API key in the header.
2. Provided examples of valid MSISDN formats in the documentation.

## Project Deliverables
1. A fully functional .NET Web API with all the required endpoints and logic.
2. A database with sample data seeded for Users and CDRs.
3. API documentation using Swagger, explaining the API key requirement and MSISDN format.
4. Middleware code for validating the API key on every request.

## Optional Extensions
1. In Progress:   To implement rate limiting based on the API key to prevent abuse.
2. In Progress:   To Create an admin endpoint to manage (create/revoke) API keys.
3. Done: To add logging using Serilog.

