# BookStoreWeb

A backend-focused BookStore application built using ASP.NET Core Web API. The project demonstrates clean backend development practices including REST APIs, Entity Framework Core, JWT authentication, dependency injection, repository/unit-of-work patterns, and unit testing.

## 🚀 Features

* Book management
* Category management
* User authentication and authorization
* JWT-based authentication
* Shopping cart functionality
* Order management
* Pagination and filtering
* Entity Framework Core
* Repository and Unit of Work patterns
* Dependency Injection
* RESTful APIs
* Unit testing
* API documentation with Swagger

## 🛠️ Technology Stack

### Backend

* ASP.NET Core Web API
* C#
* .NET 8
* Entity Framework Core
* SQLite / SQL Server
* JWT Authentication

### Testing

* xUnit
* Moq
* FluentAssertions

### Development Tools

* Visual Studio / Visual Studio Code
* Git & GitHub
* Swagger / OpenAPI

## 🏗️ Project Structure

```text
BookStoreWeb
│
├── BookStore.API
├── BookStore.Application
├── BookStore.Domain
├── BookStore.Infrastructure
├── BookStore.API.Tests
│
├── .gitignore
├── README.md
└── BookStoreWeb.sln
```

The project follows a layered architecture to separate API, application/business logic, domain models, infrastructure, and testing concerns.

## 🔐 Authentication

The application uses JWT (JSON Web Token) authentication for securing protected API endpoints.

Sensitive configuration such as:

* JWT secret keys
* Database connection strings

should not be committed to the repository.

## 🧪 Testing

The project includes unit tests using:

* xUnit
* Moq
* FluentAssertions

Tests cover the application's services and business logic to help verify expected behavior and reduce regressions.

## ▶️ How to Run

### 1. Clone the repository

```bash
git clone https://github.com/hemanthpudi/BookStoreWeb.git
```

### 2. Open the solution

Open:

```text
BookStoreWeb.sln
```

using Visual Studio or Visual Studio Code.

### 3. Configure application settings

Create your local configuration with the required database connection string and JWT settings.

Do not commit sensitive configuration values to GitHub.

### 4. Restore dependencies

```bash
dotnet restore
```

### 5. Build the project

```bash
dotnet build
```

### 6. Run the API

```bash
dotnet run
```

### 7. Open Swagger

Once the API is running, open the Swagger URL shown in the terminal to explore and test the available endpoints.

## 📌 Current Focus

The current version focuses primarily on the backend implementation and API development.

Frontend development using React can be integrated as a future enhancement.

## 🔮 Future Improvements

* React frontend
* Payment integration
* Improved product search
* Cloud deployment
* Additional integration and end-to-end tests
* Docker containerization
* CI/CD pipeline

## 👨‍💻 Author

**Hemanth Pudi**

GitHub: `https://github.com/hemanthpudi`

---

This project is created for learning, practice, and demonstrating backend development skills with ASP.NET Core.
