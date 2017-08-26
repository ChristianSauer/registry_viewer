# Registry Viewer


## Setting the Registry
You have multiple options:
1. Use the commandline, using the parameters:
	- /Registry:Url (a valid url)
	- /Registry:Password (the password for the registry, leave it blank if the is no user)
	- /Registry:User (the username for the registy, leave it blank if the is no user)
1. Use Environment Variables: Set Registry__Url, Registry__Password and Registry__User (See step 1 for the rules).
1. Add a /config/config.json via volume. The content should look like appsettings.json
1. You can expand on this image and overwrite appsettings.json, but this will store your username, password in the image.

Hint: You can overwrite any value specified in Appsettings.json with commandline arguments or environment variables. See The ASP.NET Core documentation for details (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
Note: Commandline Args overwrite environment variables which overwrite appsettings.json.


