# Admin for MeetUpPlanner

Starting from [template](https://github.com/staticwebdev/blazor-starter/generate) for [Blazor WebAssembly](https://docs.microsoft.com/aspnet/core/blazor/?view=aspnetcore-3.1#blazor-webassembly) client application, a C# [Azure Functions](https://docs.microsoft.com/azure/azure-functions/functions-overview) and a C# class library with shared code hosted as Azure Static Web App

## Getting Started

Create a repository from the [GitHub template](https://docs.github.com/en/enterprise/2.22/user/github/creating-cloning-and-archiving-repositories/creating-a-repository-from-a-template) and then clone it locally to your machine.

Once you clone the project, open the solution in [Visual Studio](https://visualstudio.microsoft.com/vs/community/) and follow these steps:

- Rename `local.settings.example.json` to `local.settings.json`
- Press **F5** to launch both the client application and the Functions API app

_Note: If you're using the Azure Functions CLI tools, refer to [the documentation](https://docs.microsoft.com/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash) on how to enable CORS._

## Solution Structure

* **Client**: The Blazor WebAssembly sample application
* **API**: A C# Azure Functions API, which the Blazor application will call
* **Shared**: A C# class library with a shared data model between the Blazor and Functions application

## Deploy to Azure Static Web Apps

This application can be deployed to [Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps), to learn how, check out [our quickstart guide](https://aka.ms/blazor-swa/quickstart).

## PWA

To get the app enabled as PWA follow these steps:
- Create a solution in Visual Studio with PWA checkbox enabled to get a template to copy from
- Copy service-worker.js and servie-worker-published.js into wwwroot
- Copy icon-512.png or create a new one.
- Copy and adapt manifest.js into wwwroot
- Add references to icon and manifest.json to index.html
- Add script to register service-worker.js to index.html
- Add ServicesWorkerAssetsManifest elemet to project file
- Add ItemGroup/ServiceWorker element ot project file
- Change service-worker-published.js to exclude routes.json from cache
- Change service-worker-published.js to exclude paths ".auth/" from cache.

## Calling backend services

To access the backend api a class BackendApiRepository is created in Client/Utils to centralize the access to the backend.

## Authentication and authorization

Authentication and authorization can easily be implemented as described in https://docs.microsoft.com/en-us/azure/static-web-apps/authentication-authorization. But to get the authentication/authorization tools provided with Blazor working an AuthenticationStateProvider is needed. An example - it's really easy - is provided by Anthony Chu, see https://anthonychu.ca/post/blazor-auth-azure-static-web-apps/ and on GitHub https://github.com/anthonychu/blazor-auth-static-web-apps. The AuthenticationStateProvider is used in this example to get the ClientPrincipal converted to a standard ClaimsPrincipal. AuthorizeView etc are working with this implementation. On server side see function GetUserDetails how the user identity can be leveraged.

