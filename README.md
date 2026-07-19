# AI SQL Agent using C# .NET 8 and OpenAI

An AI-powered SQL Agent built using ASP.NET Core (.NET 8), SQL Server, and OpenAI APIs.

This project allows users to ask questions in natural language, converts them into SQL queries using AI, executes the queries against SQL Server, and explains the generated SQL.

# Features

- Natural Language to SQL conversion
- SQL query execution against SQL Server
- AI-generated SQL explanation
- REST API using ASP.NET Core Web API
- Swagger API testing support
- Secure configuration using appsettings and environment separation
- Dependency Injection based architecture
- Modular service-based implementation

# Tech Stack

| Technology | Purpose |
|---|---|
| C# | Backend development |
| ASP.NET Core .NET 8 | Web API |
| SQL Server | Database |
| OpenAI API | AI-powered SQL generation |
| Swagger | API testing |
| Visual Studio 2022 | Development IDE |
| Git & GitHub | Source control |

# Project Architecture

User Request -> QueryController -> AIService -> OpenAI API -> Generated SQL Query -> SqlExecutorService -> SQL Server Database -> Results Returned to User

# Project Structure

SqlAgent
|-Controllers
|----QueryController.cs
|
|-Models
|----QueryRequest.cs
|----QueryResponse.cs
|
|-Services
|----AIService.cs
|----SqlExecutorService.cs
|----SchemaService.cs
|
|-appsettings.json
|-Program.cs
|-README.md
|-SqlAgent.csproj

# API endpoints
	POST /api/query/ask
	----- Request ----
	{
		"question": "Get all customers"
	}

	Response :: 
	{
  "success": true,
  "question": "get 2 customer",
  "sqlQuery": "SELECT TOP 2 * FROM Customers;",
  "resultCount": 2,
  "data": [
    {
      "Id": 1,
      "Name": "Alice",
      "Country": "USA"
    },
    {
      "Id": 2,
      "Name": "Bob",
      "Country": "Canada"
    }
  ],
  "explanation": "The query retrieves the first two customers from the database. In this case, it returns Alice from the USA and Bob from Canada. This helps you quickly see basic details for two customers."
}

	POST /api/query/explain
	"Get all customers"

	----- Response ----
	{
		"question": "Get all customers"
	}


}
