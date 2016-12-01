## Welcome to the port of JabbR to ASP.NET Core
---
This is an open source, cross platform version of David Fowler's [JabbR](https://github.com/JabbR/JabbR) running on .NET Core.

## Setting up for Local Development
 For security, connection strings and other sensitive information must be stored in environment variables.
 - You can easily read and write these key-value pairs using the `dotnet user-secrets` command
   - You must first add a reference to `Microsoft.Extensions.Configuration.UserSecrets` in your `project.json`
 - This command must be executed from the top level of your ***project*** (read: not solution). This is
   because it will try to find a `project.json` file referencing the executable.
   - You can easily test if the executable is found by typing 
   ```
   $ dotnet user-secrets -h
   ```

 If you correctly followed the above steps, you'll be met with a help dialog when you execute
 `dotnet user-secrets -h` that looks like this. ![dotnet user-secrets help](https://i.gyazo.com/9220f055788ff9bb6cd24da9a14e6076.png)

 If you receive an error like the following
 ```bash
$ dotnet user-secrets -h
No executable found matching command "dotnet-user-secrets"
 ```

 Then you likely need to rebuild your solution, or move into the correct project directory where the `project.json` file can be found.

To set a user secret, you need to use `dotnet user-secrets set <key> <value>`, like so

```bash
$ dotnet user-secrets set "connectionString" "Server=MYAPPNAME.database.windows.net,1433;Initial Catalog=MYCATALOG;Persist Security Info=False;User ID={plaintext user};Password={plaintext password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

Alternatively, if you want to use [LocalDB, an alternative built in database](https://blogs.msdn.microsoft.com/sqlexpress/2011/07/12/introducing-localdb-an-improved-sql-express/), you 
can run the same command as above, but with a simpler connection string like so

```bash
$ dotnet user-secrets set "connectionString" "Server=(localdb)\mssqllocaldb;Database=JabbRCore;Trusted_Connection=True;MultipleActiveResultSets=true"
```

Note that this command will double escape the back slash in the connection string for you, so you only need to enter one as shown above.

## How to Access your Stored User Secrets

In your `Startup.cs` file, you can access the Configuration API easily by creating an `IConfigurationRoot` object instance.
This is created for you in the Visual Studio template project for a .NET Core Web Application, as below. 
Whatever key you defined for your key-value pair when you used `dotnet user-secrets set <key> <value>` is what you will
use to access that value in your code.

```csharp
private IConfigurationRoot _configuration;

public Startup(IHostingEnvironment env)
{
    // This code is templated for you upon creation of a new project in Visual Studio
    // File > New Project > .NET Core > .NET Core Web Application > Pick any of Empty, Web API, or Web Application
    var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

    if (env.IsDevelopment())
    {
        builder.AddUserSecrets();
    }

    builder.AddEnvironmentVariables();

    _configuration = builder.Build();
}

public void ConfigureServices(IServiceCollection services)
{
    // Here you are accessing the environment variable you stored under the key "connectionString"
    string connection = _configuration["connectionString"];

    // And then you can use that value to register other services, like your database.
    services.AddDbContext<JabbrContext>(options => options.UseSqlServer(connection));
```

The above command will set the key `connectionString` to an environment variable containing the given database connection string.
We've used the structure of a connection string hosted on Azure, but **you will have to use your own**

## Configuring Social Authentication

If you want your users to have the option to autenticate via external providers such as Facebook, Google, Twitter and Microsoft 
you will need to obtain API keys from these providers and configure them as user secrets. We will use Facebook as an example.

Find out how to obtain API keys [here.](https://docs.asp.net/en/latest/security/authentication/sociallogins.html)

Once you have the AppID and AppSecret for each provider set them as user secrets using 

```bash
$ dotnet user-secrets set "Authentication:<ProviderName>:AppId" "<AppId>"
$ dotnet user-secrets set "Authentication:<ProviderName>:AppSecret" "<AppSecret>"
```

Then enable the middleware for each provider by installing the proper NuGet package `Microsoft.AspNetCore.Authentication.<ProviderName>`.

Lastly, configure the options for each provider in your Configure() method of Startup.cs as shown below.

```csharp
var facebookAppId = _configuration["Authentication:Facebook:AppId"];
var facebookAppSecret = _configuration["Authentication:Facebook:AppSecret"];
if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
{
    app.UseFacebookAuthentication(new FacebookOptions()
    {
        AppId = facebookAppId,
        AppSecret = facebookAppSecret
    });
}
```

## NOTE

 - You **need** to replace the above connection string with your own.
 - You will have to replace `{plaintext user}` and `{plaintext password}` in the above connection string with your **real** credentials.

*"Help, I can't find my connection string!"* 
[No problem, read more about Azure DB's and connections here](https://azure.microsoft.com/en-us/documentation/articles/sql-database-develop-dotnet-simple/)

[Learn more about safe storage of user secrets in .NET](https://docs.asp.net/en/latest/fundamentals/configuration.html)

## Testing

### To Run

You can run the written tests from either Visual Studio or your Console. 

#### From the Console
In order to run the written tests, you must open a command prompt or PowerShell command window. In the window, navigate to folder containing the source code of your test project (this will be in JabbR-Core.Tests).
To run the .NET CLI test runner, type `dotnet test` and press enter. It should run the tests and print out if there were any that had errors, skipped, or failed, along with where they might have failed. 

#### From Visual Studio
 Show the Test Explorer window by choosing **Test > Windows > Test Explorer**. The Test Explorer window will show inside Visual Studio, and your test should be visible (if they're not, try building your project to kick off the test discovery process). If you click the Run All link in the Test Explorer window, it will run your tests and show you success and failure. You can click on an individual test result to get failure information as well as stack trace information.

### To Write

We used xUnit to write our tests. A verbose guide on writing tests for .Net Core with xUnit can be found 
[here](https://xunit.github.io/docs/getting-started-dotnet-core.html).
Basically:
 - A reference to xUnit should already be in the project.json of the JabbR-Core.Tests library. 
 - Make sure your class has a using statement for xUnit. (`using Xunit;`)
 - Tests should use the `[Fact]` tag and always have an assertion statement. 
 - You can also write Theories, which are tests that are true for a particular set of data. 