# Orleans.HttpGateway.AspNetCore
a http gateway for Microsoft Orleans.

[![Build status](https://ci.appveyor.com/api/projects/status/6omov0335yw8a9c5?svg=true)](https://ci.appveyor.com/project/rikbosch/orleans-httpgateway-aspnetcore) [![Nuget version](https://img.shields.io/nuget/v/Orleans.HttpGateway.AspNetCore.svg)](https://www.nuget.org/packages/Orleans.HttpGateway.AspNetCore)



## Installation

`Install-Package Orleans.HttpGateway.AspNetCore`

## Configuring the gateway

Startup.cs

``` csharp
public void ConfigureService(IServiceCollection services)
{
    // add known grain interface assemblies
    Assembly grainInterfaceAssembly = GetGrainInterfacesAssembly();
    services.AddOrleansHttpGateway(c => c.AddAssemblies(grainInterfaceAssembly));

    // ensure IGrainFactory is registered in the service container
    services.AddSingleton<IGrainFactory>(GetGrainFactory());
}

public void Configure(IApplicationBuilder app)
{
    // register the middleware in the application
    // note: the middleware internally uses Microsoft.AspNetCore.Routing
    // default route is: "{grainInterface}/{grainId}/{grainMethod}"
    app.UseOrleansHttpGateway();
}

```

__IMPORTANT: Please note that ALL grains are accessible with this gateway, it is adviceable to not publicly expose the endpoint and use any of the default methods to securing your web app__

## Calling grains with http client

Grainmethods can be invoked using the following url format

`http://your.aspnet.url/{grainInterface}/{grainId}/{grainMethod}`

> example: `http://localhost:5000/Orleans.HttpGateway.AspNetCore.Tests.ITestGrain3/6/GetObjectWith2Parameters?one=1&two=okay`

**Providing method parameters**

given the following grain interface:

```csharp
public interface ITestGrain : IGrainWithStringKey
{
    Task<int> TestGrainMethod(string p1, bool p2, string[]p3);
}
```

parameters can be supplied by QueryParameters for `GET` requests, e.g. `?p1=one&p2=true&p3=[a,b,c]`

complex parameters can also be provided in the request body using `PUT` or `POST`, names of the root elements must match the variable names in the GrainInterface and contenttype must be `application/json`:

```json
{
    "p1":"one",
    "p2":true,
    "p3": ["a","b","c"]
}
```

__note: not supplied values will default to null in the resulting grain call__

**Using Compound Grain Keys**

Compound keys can be provided, seperated with a `,`
e.g for an IGrainWithIntegerCompoundKey: `http://localhost:5000/Orleans.HttpGateway.AspNetCore.Tests.ITestGrain3/1234,myStringkey/GetObjectWith2Parameters?one=1&two=okay`




## Acknowledgements

this project uses `ObjectMethodExecutor` from `https://github.com/aspnet/Common/tree/dev/shared/Microsoft.Extensions.ObjectMethodExecutor.Sources`







