## Welcome to the port of JabbR to ASP.NET Core
---
This is an open source, cross platform version of David Fowler's [JabbR](https://github.com/JabbR/JabbR) running on .NET Core. 

Further documentation below on 
[setting up for local development](#setting-up-for-local-development), 
[accessing stored user secrets](#how-to-access-your-stored-user-secrets),
[configuring social authentication](#configuring-social-authentication), and
[testing](#testing).

## Features and commands
JabbR is a chat application similar to IRC, built with ASP.NET Core, using SignalR and Entity Framework.

#### Commands
Below is a list of commands to use while chatting.

Join and leave a room. 
```
/join [roomName]
To join a private room /join [roomName] [inviteCode]
/leave [roomName]
```
Find users. 
```
Type /who without parameters to list who's in the room.
/who [userName]
/where [userName]
```
Set a status. Use Afk to set your away message.
```
Type /note "Status message" to set your status message, and /note to clear it. 
/afk "Away message"
/flag [countryAbreviation] to set your country.
```
Personalize your room.
```
/topic "Set the topic"
/welcome "Set the welcome command"
```
Make a meme!
```
/meme [memeName] "top-text" "bottom-text"
The top and bottom text is optional, make sure any space between words is denoted with a dash. 
```
If you ever get lost,
```
Type /? - to show the full list of JabbR Commands
```

#### Content Provider Support
Inline image and content support for your favorite sites:
- BBC News
- Dictionary.com 
- GitHub (issues and comments)
- Imgur
- Nerd Dinner
- Slideshare
- Soundcloud 
- Spotify
- Twitter
- Xkcd
- Youtube
- 9gag


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
$ dotnet user-secrets set "connectionString" "Server=MYAPPNAME.database.windows.net,1433;Initial Catalog=MYCATALOG;Persist Security Info=False;User ID={plaintext user};Password={plaintext password};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
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
you will need to obtain API keys from these providers and configure them as user secrets.

Find out how to obtain API keys [here.](https://docs.asp.net/en/latest/security/authentication/sociallogins.html)

Once you have the AppID and AppSecret for each provider set them as user secrets using 

```bash
$ dotnet user-secrets set "Authentication:<ProviderName>:AppId" "<AppId>"
$ dotnet user-secrets set "Authentication:<ProviderName>:AppSecret" "<AppSecret>"
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
In order to run the written tests, you must open a command prompt or PowerShell command window. In the window, navigate to folder containing the source code of your test project (this will be in test\JabbR-Core.Tests).
To run the .NET CLI test runner, type `dotnet test` and press enter. It should run the tests and print out if there were any that had errors, skipped, or failed, along with where they might have failed. 

#### From Visual Studio
 Show the Test Explorer window by choosing **Test > Windows > Test Explorer**. The Test Explorer window will show inside Visual Studio, and your tests should be visible (if they're not, try building your project to kick off the test discovery process). 
 If you click the Run All link in the Test Explorer window, it will run your tests and show you successes and failurees. You can click on an individual test result to get failure information as well as stack trace information.

### To Write

We used xUnit to write our tests. A verbose guide on writing tests for .Net Core with xUnit can be found 
[here](https://xunit.github.io/docs/getting-started-dotnet-core.html).
Basically:
 - A reference to xUnit is already in the project.json of the JabbR-Core.Tests library. 
 - Make sure your class has a using statement for xUnit. (`using Xunit;`)
 - Tests should use the `[Fact]` tag and always have an assertion statement. 
 - You can also write Theories, which are tests that are true for a particular set of data. 