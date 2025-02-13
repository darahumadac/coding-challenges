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
- Add logging with [`Serilog`](#logging-using-serilog)
- Caching with [`redis`](#redis)
- Add your service as a scoped service so that it can be used for Dependency Injection via constructor
```c#
builder.Services.AddScoped<MyService>();
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
- During unit testing, can also create an instance of an in memory database as database provider:
```bash
# make sure to add the in memory database package
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```
```c#
var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("databaseName").Options;
var _db = new AppDbContext(options);
```
  - if in memory database is used for unit testing, cannot mock exceptions

## Moq
- requires the DbSet<T> and entities to be virtual, and needs a parameterless ctor so that it can mock entity framework dbcontext
  - Prepare mock data list and cast mock data `AsQueryable()`
  - To mock data for the DbSet<T>, must setup mock `mockDbSet.As<IQueryable<T>>().Setup(s => s.GetEnumerator()).Returns(mockData.GetEnumerator())`
  - Pass the mockDbSet to mockDbContext `mockDbContext.Setup(db => db.SampleSet).Returns(mockDbSet.Object)`
  - Need to mock `DbSet.Find`; `IQueryable.FirstOrDefault` no need to mock
- when mocking methods, make sure to mock all parameters including optional ones. for example, mocking `redis.HashSet(RedisKey, RedisValue, RedisValue, When, CommandFlags)`, all of those parameters need to be mocked / specified. similarly, when using `.Callback((RedisKey, RedisValue, RedisValue, When, CommandFlags))`, all parameters should be specified

## NUnit
- `TestCase` attribute only accepts constant parameters. To be able to tell the Test where to get test data, need to use `TestCaseSource` attribute (see [TestCaseSource](https://docs.nunit.org/articles/nunit/writing-tests/attributes/testcasesource.html))
  - The test case provider must be `static` and must return an `IEnumerable`

## Logging using Serilog
- https://serilog.net/
- https://nblumhardt.com/2016/08/context-and-correlation-structured-logging-concepts-in-net-5/
- .net core provides out of the box support for logging http request and responses (see below). This can also be used together with serilog
```c#
builder.Services.AddHttpLogging();
//using the httplogging
app.UseHttpLogging();
```
- a `sink` in a logger is an output where the logger writes the log's stream to
- add the Serilog package:
```bash
dotnet add package Serilog.AspNetCore
#for logging to console
dotnet add package Serilog.Sinks.Console
#for logging to file
dotnet add package Serilog.Sinks.File
```
- configure `Serilog` in `Program.cs`
```c#
//configure logging service
Log.Logger = new LoggerConfiguration() //make sure this is Log.Logger
  .ReadFrom.Configuration(builder.Configuration)
  .CreateLogger();
builder.Logging.ClearProviders();
builder.Services.AddSerilog(); //add serilog as log provider

//Using the logger...
//then use the logger from the classes
private Serilog.ILogger _logger = Log.ForContext<MyService>(); //creates a logger with SourceContext
_logger.ForContext("MyCustomProp", "test property").Information("doing something");
//{..."MessageTemplate":"doing something"..."Properties":{"SourceContext": "MyService", "MyCustomProp":"test property"}}


//logger is accessed via dependency injection (Microsoft.Extensions.Logging.ILogger) and also achieves the same SourceContext for the service
public MyService(ILogger<MyService> logger){
  _logger = logger;
}
public void DoAction(){
  using(LogContext.PushProperty("MyCustomProp", "test prop")){
    _logger.LogInformation("doing action");
  }
  _logger.LogInformation("doing action without custom prop");
}
//{..."MessageTemplate":"doing action"..."Properties":{"SourceContext": "MyService", "MyCustomProp":"test prop"}}
//{..."MessageTemplate":"doing action without custom prop"..."Properties":{"SourceContext": "MyService"}}

```
- Using enrichers: https://github.com/serilog/serilog/wiki/Enrichment
```c#
app.UseSerilogRequestLogging();
//HTTP POST /shorten responded 200 in 913.7836 ms

//serilog request logging with diagnostic enrichers - example below adds TestBody property which has the value of the request body. This can also be done by using out of the box .net core feature - builder.Services.AddHttpLogging() then app.UseHttpLogging();
//1) ensure that the request body can be read multiple times.  request bodies are streams, and making the request reads the stream, so if a request contains a body, you can't make it twice. so we need to ensure that body can be read multiple times
app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});
//2) add the enrich diagnostic
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = async (diagnosticContext, httpContext) =>
    {
        //make sure to set the position to the beginning of the stream
        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
        using (var reader = new StreamReader(httpContext.Request.Body))
        {
            //read to end
            var body = await reader.ReadToEndAsync();
            diagnosticContext.Set("TestBody", body);
        }
    };
});
```
### Using Seq with Serilog
- to use seq locally, pull the docker image and run it
```bash
docker pull datalust/seq
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```
- to ingest log files, install `seqcli` tool via dotnet tool, and configure it, then run ingest command
```bash
dotnet tool install --global seqcli

# configure seqcli to connect to the seq server
seqcli config -k connection.serverUrl -v https://your-seq-server
#if connecting in docker in localhost, use your ip address
seqcli config -k connection.serverUrl -v http://<your ip here>:<your port>
#you can also configure api keys
seqcli config -k connection.apiKey -v your-api-key # can skip this if running locally. i did not set up my api keys

#then, run ingest command
seqcli ingest -i log_*.txt --json #this ingests all log files and reads events as json
```
- documentation on importing log files in seq: https://docs.datalust.co/docs/importing-log-files
- querying logs: https://docs.datalust.co/docs/the-seq-query-language

## Redis
- To add redis in .net core app, add package `StackExchange.Redis`
```bash
dotnet add package StackExchange.Redis
```
- To run redis in windows, make sure to download `redis` docker image
- Then, create a container and run it
```bash
#create container without redis persistence
docker create -p 6379:6379 --name redis redis
#start container
docker start redis
```
- To run `redis-cli` commands in the running redis container:
```bash
docker exec -it redis redis-cli
```
- Add redis as a singleton service
```c#
//register redis service
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));
```
- Access redis through using dependency injection
```c#
private IDatabase _redis;
public MyService(IConnectionMultiplexer mux){
  _redis = mux.GetDatabase();
}
```
- can mock redis using Moq

## .NET Core environments
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-8.0
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

## Frontend
### React / Javascript
- `for` attribute of a `label` tag should be equal to the `id` of an `input` tag to bind them together
- bootstrap a react.js app using vite:
```bash
npm create vite
```
- Some HTML attributes that are renamed in react:
  - `class` --> `className`
  - `for` --> `htmlFor`
- When using `form` in react:
  - `onSubmit vs action` attribute
    - `action` - allows to invoke a server action on a `form` element, with *formData* as an argument
    - `onSubmit` - allows to specify a function that will be executed *after* the server action is completed
    - `event.preventDefault()` prevents postback, which the the default behavior for form submission
  - handling text input:
    - set a state for the input text: `const [myText, setMyText] = useState("");`
    - the input text must then use the state by setting the attributes `value` and `onChange`
    ```html
    <input type="text" value={myText} onChange={e => setMyText(e.target.value)}/>
    ```
- `useReducer` - use this to consolidate all state update logic into a single function called a `reducer`
  - https://react.dev/learn/extracting-state-logic-into-a-reducer
  - use this to manage multiple state updates that are dependent on each other
  - `reducer`
    - a reducer function has 2 parameters: `currentState` and `action` object
    - a `reducer` returns a next *state* for React to set depending on the `action.type`
    - the convention is to use switch case statement inside reducers 
```js
const reducerFunction = (currentState, action) => {
  switch(action.type){
    case "done":{
      return {
        ...currentState,
        loading: false,
        message: "Done loading",
        done: true
      }
    }
    case "loading":{
      return {
        ...currentState,
        loading: true,
        message: action.message
      }
    }
    default: {
      throw new Error('Unknown action: ' + action.type);
    }
  }
}

const initialState = {loading: false, message: "Not loading", done: false};
const [state, dispatch] = useReducer(reducerFunction, initialState);
//do something to trigger loading state...
dispatch(
  //this is the action object
  {type: "loading", message: "This is a loading message"}
);
//when done loading, trigger done loading state by dispatching done action
dispatch({type: "done"});

return (
  <p>{state.message}</p>
)
``` 

#### Event Handling
  - `addEventListener` is the recommended way to register an event listener / hander: https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/addEventListener
  ```js
  addEventListener(type, listener)
  addEventListener(type, listener, options)
  addEventListener(type, listener, useCapture)
  ```
  - all event handlers receive a notification (`event`) which implements the `Event` interface when an event occurs
    - https://developer.mozilla.org/en-US/docs/Web/API/Event

#### Fetch, Promises, Async/Await, Then
- https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API/Using_Fetch
- when using `fetch` api, it returns a `Promise`, which can be `await`ed
- **Await vs Then**
  - both are used for handling asynchronous operations, primarily with dealing with Promises 
  - `then` attaches a **callback** that will be executed when the `Promise` resolves
    - promise chaining
    - no need for async functions
    - good for multiple independent promises (using `.then` and `Promise.all([<Promise1>,<Promise2>...])`)
      - https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise/all
    - for **parallel** async operations
  - `await` *pauses execution of the function* until the `Promise` resolves
    - synchronous-like behavior
    - can only be used in an async function `const getData = async () => {}` or `async function getData(){}`
    - error handling with try catch
    - for **sequential** async operations


### HTML and CSS
- **Grid vs. Flexbox**
  - The CSS Grid Layout should be used for** two-dimensional layout**, with **rows AND columns**.
  - The CSS Flexbox Layout should be used for **one-dimensional layout**, with** rows OR columns**.
- **CSS Layouts:**
  - for row AND column: use `display: grid`
    - https://www.w3schools.com/css/css_grid.asp
  - for row OR column: use `display: flex`
    - https://www.w3schools.com/css/css3_flexbox.asp
- CSS Properties used:
  - `display`
  - `grid-template-columns`
  - `flex-direction`
  - `vertical-align`
  - `margin`
  - `padding`
  - `border-radius`
  - `color`
  - `background-color`
  - `font-size`
  - `border`
- CSS Selectors used:
  - https://www.w3schools.com/css/css_selectors.asp 
  - `>` --> child combinator. matches all CHILDREN (not descendant) of specified element
  - ` `(space) --> descendant combinator. matches all DESCENDANTS of specified element
  - `.` --> class selector
  - `#` --> id selector

## Makefile / Bash
- run `(cd ./my_folder && <some command>)` to navigate to the directory and run the command without changing directory
- add `&` at the end of the command to make it run in the background

## Cross-Origin Resource Sharing (CORS)
- Must set CORS policies in web api by adding CORS and using CORS in `Program.cs`:
```c#
builder.Services.AddCors(options => {
  //this adds a policy which can then be passed in to app.UseCors("policyName");
  options.AddPolicy("PolicyName", policy => {
    policy.WithOrgins("https://mywebsite.com", "https://anotherwebsite.com") //set the allowed origins that access the api
          .WithMethods("POST", "GET", "DELETE") //set the allowed methods for the api
          .WithHeaders("Content-Type", "Authorization", "Accepts"); //set the allowed headers for the api
  })

  //to set a default policy
  options.AddDefaultPolicy(options => {
    options.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
          .WithMethods("POST", "GET") 
          .AllowAnyHeader(); //this is not advisable
  })
});

//then use the cors
app.UseCors(); //or
//or use cors with configured policy name
app.UseCors("policyName"); 
```
- When adding policy to CORS, make sure that allowed origin (`WithOrigins`), allowed methods (`WithMethods`), and allowed headers (`WithHeaders`) are limited. 
- Do not `AllowAnyOrigin`, `AllowAnyMethod` or `AllowAnyHeader` in Production environment. Set CORS according to the environment