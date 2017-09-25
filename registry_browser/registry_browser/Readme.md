# Registry Viewer
This is a simple to use registry viewer for Docker Registries.
It supports any v2 Docker Registry.

Running this is is easy:
1. Build a small docker compose file which uses this image:
	```
	version: '2'

	services:
	registry_viewer:
		image: christiansauer/registry_viewer
		volumes:
		- "config/config/appsettings.json:/<your path>"
	```

2. Place an apssetings.json in the path specified inm #1:
	```json
	{
		"Registry": {
			"Url": "URL OF YOUR REGISTRY ",
			"User": "USERNAME",
			"Password": "PASSWORD"
		}
	}
	```
3. Start it with docker-compose up
4. You will be notified if the registry can be reached by the viewer

## Setting the Registry
You have multiple options:
1. Add a /config/appsettings.json file via volume. The content should look like appsettings.json
1. Use the commandline with these parameters:
	- /Registry:Url (a valid url)
	- /Registry:Password (the password for the registry, leave it blank if there is no user)
	- /Registry:User (the username for the registy, leave it blank if there is no user)
1. Use Environment Variables: Set Registry__Url, Registry__Password and Registry__User (See step 2 for the rules).
1. You can expand on this image and overwrite appsettings.json, but this will store your username and password in the image.

Hint: You can overwrite any value specified in Appsettings.json with commandline arguments or environment variables. See The ASP.NET Core documentation for details (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
Note: Commandline Args overwrite environment variables which overwrite appsettings.json.


