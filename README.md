
# iLib - A Library Web Application

iLib is a library management system built as part of the final project for the Software Architectures and Methodologies course at the University of Florence. The system provides both citizen and administrator users the ability to manage and interact with various articles, such as books, magazines, and DVDs. The application is built with C# and .NET and uses NHibernate ORM for database management with MySQL.
## Features

### Citizen User
- Search for articles in the library's catalogue.
- Book available articles.
- View current loans and loan history.

### Administrator
- Add, modify, or remove articles from the library catalogue.
- Register and manage users.
- Book articles or extend loans on behalf of citizens.
- Manage loans and returns.
- Consult loan and booking history of users.

## Technologies Used

- **Backend**: C# with .NET Core and NHibernate.
- **Frontend**: Angular-based UI.
- **Database**: MySQL for persistent data storage.
- **Security**: JWT-based authentication using ASP.NET Core.
- **ORM**: Fluent NHibernate for mapping entities to the database.

## Setup Instructions

### Prerequisites
- **.NET Core SDK**
- **MySQL Database** with schemas `iLib_C#` and `iLib_C#_test`.
- **Visual Studio** or any preferred IDE for C# development.

### Database Setup

You must create two MySQL schemas for the application to function:
1. `iLib_C#`: The main schema used by the application.
2. `iLib_C#_test`: Schema used for end-to-end tests.

Ensure both schemas are created before running the application, as the system assumes their existence.

### NHibernate Configuration

The application uses Fluent NHibernate for ORM. The `NHibernateHelper` class configures the session factory to connect to the MySQL database. You can find this configuration in the `NHibernateHelper.cs` file.

For the main application database, the connection string is as follows:
```csharp
string connectionString = "Server=localhost;Port=3306;Database=iLib_C#;Uid=java-client;Pwd=password;SslMode=None;";
```

For testing purposes, switch to the `iLib_C#_test` database by modifying the connection string:
```csharp
string connectionString = "Server=localhost;Port=3306;Database=iLib_C#_test;Uid=java-client;Pwd=password;SslMode=None;";
```

### Application Configuration

At startup, the application registers a default administrator user if none exists. This user is created with the following credentials:
- **Email**: `admin@example.com`
- **Password**: `admin password`

These credentials can be modified in the `DatabaseInitializer.cs` file, located in the `Utils` folder.

### Running the Application

1. Install the necessary packages:
   ```bash
   dotnet restore
   ```

2. Build the application:
   ```bash
   dotnet build
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. Open the browser and navigate to:
   ```
   https://localhost:5001/ilib/v1
   ```

### Testing

End-to-end tests connect to the `iLib_C#_test` schema. Ensure that the test schema is correctly set up before running the tests.

To run tests, use the following command:
```bash
dotnet test
```
