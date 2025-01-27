# Notes
- use `Results` to return responses
  - call `.WithName("MyEndpointName")` method to set a name for the endpoint so it can be referenced by name
  - `Results.CreatedAt("MyEndpointName", new {parameter1 = "value"}, responseObj)`
- use `System.Text.Json.Serialization` and annotate a *record type field* with `[property: JsonPropertyName("custom_name")]` to set a custom json field name. annotate with `[JsonPropertyName("custom_name")]` if it is a class property
  - the naming convention can be configured in the middleware also
- Data annotations cannot be used directly with positional syntax for record types because Data Annotations apply only to properties, so must use the expanded syntax for records. Alternatively, can validate the fields manually or using FluentValidation library
- In minimal APIs, add `HttpContext` as parameter to access **HttpContext**
  - inject `LinkGenerator` to utilize in creating URI using `HttpContext`
- To use hashing algorithms, add nuget package `System.IO.Hashing`
```bash
dotnet add package System.IO.Hashing
```


## Entity Framework Core
- install `dotnet ef` cli tool - `dotnet tool install --global dotnet-ef`
- To use EF Core for SQL Server, add the following nuget packages:
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
```
- Add connection string for db in the appropriate `appsettings.json` or `appsettings<EnvironmentName>.json`. Make sure the connection string is properly formatted e.g. slashes are escaped
- Create a DbContext instance and register the instance in the app service provider:
```c#
//MyAppDbContext.cs - creat the db context instance
public class MyAppDbContext : DbContext
{
    public MyAppDbContext(DbContextOptions options) : base(options){}
}
//Program.cs - register the db context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); //assumes connection string always exists
builder.Services.AddDbContext<MyAppDbContext>(options => options.UseSqlServer(connectionString))
```
- Add the DbContext instance to the endpoint routebuilder delegate
- Setting Indexes:
  - by naming convention, primary key is the property with Id in its name
  - add single index by adding `[Index(nameof(<PropertyName>))]` at class level
  - add composite index by using `[Index(nameof(<PropertyName>), nameof(Property2))]` at class level
  - set unique index by using `[Index(nameof(<PropertyName>)), IsUnique = true]`
- DB Migrations and DB update:
```bash
# to add migration
dotnet ef migrations add InitialCreateDb
# to remove migrations
dotnet ef migrations remove
# to update database
dotnet ef database update
```
- need to set the properties for entities e.g. DbSet<T> and T to `virtual` so that they can be mocked using Moq;
- needs a parameterless ctor

## Moq
- requires the DbSet<T> and entities to be virtual, and needs a parameterless ctor so that it can mock entity framework dbcontext
  - Prepare mock data list and cast mock data `AsQueryable()`
  - To mock data for the DbSet<T>, must setup mock `mockDbSet.As<IQueryable<T>>().Setup(s => s.GetEnumerator()).Returns(mockData.GetEnumerator())`
  - Pass the mockDbSet to mockDbContext `mockDbContext.Setup(db => db.SampleSet).Returns(mockDbSet.Object)`

## NUnit
- `TestCase` attribute only accepts constant parameters. To be able to tell the Test where to get test data, need to use `TestCaseSource` attribute (see [TestCaseSource](https://docs.nunit.org/articles/nunit/writing-tests/attributes/testcasesource.html))
  - The test case provider must be `static` and must return an `IEnumerable`

## Redis
- To add redis in .net core app, add package `StackExchange.Redis`
```bash
dotnet add package StackExchange.Redis
```

## .NET Core environments
- to run .net core app in different enivornments, set the `--environment` flag when running the project
```bash
dotnet run --environment Production
```

## dotnet user-secrets
- `dotnet user-secrets init --project <projname>` --> this will generate a `<UserSecretsId>` property in the `.csproj` file
- Location of the secrets.json file is in - `Secrets%APPDATA%\microsoft\UserSecrets\<userSecretsId>\secrets.json`
- Then set the secret like `dotnet user-secrets set "mysecretkey" "myvalue" --project <projname>`
- Access the secret by using .AddUserSecrets<T> in the configuration setup in Program.cs, and inject it to the class, or call the `builder.Configuration["mysecretkey"]`


## REST Client
- declare variables as `@host=http://localhost:8080`
### Sample Requests
```bash
@host=http://localhost:8080

GET {{host}}/test
Content-Type: application/json

###

POST {{host}}/shorten
Content-Type: application/json

{
    "sample": "data"
}

```