## Welcome to the port of JabbR to ASP.NET Core
---
This is an open source, cross platform version of David Fowler's [JabbR](https://github.com/JabbR/JabbR) running on .NET Core.

## Setting up for local development
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

The above command will set the key `connectionString` to an environment variable containing the given database connection string.
We've used the structure of a connection string hosted on Azure, but **you will have to use your own**

## NOTE

 - You **need** to replace the above connection string with your own.
 - You will have to replace `{plaintext user}` and `{plaintext password}` in the above connection string with your **real** credentials.

*"Help, I can't find my connection string!"* 
[No problem, read more about Azure DB's and connections here](https://azure.microsoft.com/en-us/documentation/articles/sql-database-develop-dotnet-simple/)

[Learn more about safe storage of user secrets in .NET](https://docs.asp.net/en/latest/fundamentals/configuration.html)