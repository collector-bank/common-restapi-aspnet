# Collector Common RestApi fot Asp.Net

[![Build status](https://ci.appveyor.com/api/projects/status/k3m0g3tc39p6avwa?svg=true)](https://ci.appveyor.com/project/HoudiniCollector/common-restapi-aspnet)
Provides a set of filters, services and model binding utilities for AspNet WebApi when using contracts implementing  [Collector.Common.RestContracts](https://github.com/collector-bank/common-restcontracts)
## TL;DR

**WebApiConfig**

```csharp
public static class WebApiConfig
{
	public static void Register(HttpConfiguration config)
	{
		config.MapHttpAttributeRoutes();

		config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

		config.ParameterBindingRules.Insert(
			0,
			descriptor => typeof(IRequest).IsAssignableFrom(descriptor.ParameterType)
							  ? new RestfulParameterBinding(descriptor)
							  : null);
		
		config.Services.Insert(typeof(ModelBinderProvider), 0, new BodyAwareModelBinderProvider());

		config.Services.Replace(typeof(IHttpActionInvoker), ResolveService<MyApiActionInvoker>());

		config.Filters.Add(ResolveService<ResponseLoggingFilter>());
		config.Filters.Add(new ContextActionFilter());
		config.Filters.Add(new CorrelationIdActionFilter());
		config.Filters.Add(ResolveService<RequestLoggingFilter>());
		config.Filters.Add(new RequestValidationFilter());
	}
  
	private static TService ResolveService<TService>()
	{
		var service = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(TService));

		return (TService)service;
	}
}
```
**Global.asax**
```csharp

public class WebApiApplication : System.Web.HttpApplication
{
	protected void Application_Start()
	{
	//configurations...
		GlobalConfiguration.Configure(WebApiConfig.Register);
	//other configurations...
	}
}
                    
```
**Controller**
```csharp
public class GetController : ApiController
{
	[RestfulRoute("api/MyRequestResource/{Id}")]
	public HttpResponseMessage Get([FromUri] MyGetRequest request)
	{
		try
		{
			var result = GetSomeDataForId(request.GetResourceIdentifier().Id);

			return Request.BuildOkDataResponse(new MyGetRequestResponse 
			{
				SomeProperty = result
			});
		}
		catch(SomeEntityNotFoundException ex)
		{
			return Request.CreateResponse(HttpStatusCode.NotFound);
		}
		catch(Exception ex)
		{
			return Request.CreateResponse(HttpStatusCode.InternalServiceError);
		}
	}
}
```

## Filters

The filters provided are optional, but can give you some nice features like: 

 - ResponseLoggingFilter - will log the response to the provided ILogger with timings, body and status codes.
 - RequestLoggingFilter - will log the request to the provided ILogger, name of the controller handling the request, the request body, http method etc.
 - RequestValidationFilter - will validate the request contract and if it is not valid then it will return an error response.
 - ContextActionFilter - will ensure that the provided context will be set on the response.
 - CorrelationIdActionFilter - will set a correlation id (all logs related to this request will have a common id set on the response)

```csharp
config.Filters.Add(new ResponseLoggingFilter(logger));
config.Filters.Add(new ContextActionFilter());
config.Filters.Add(new CorrelationIdActionFilter(setCorrelationIdFromContext: bool));
config.Filters.Add(new RequestLoggingFilter(logger));
config.Filters.Add(new RequestValidationFilter());
config.Filters.Add(new AuthorizeAttribute());
```

## Services
Two services are provided. 

The 'BodyAwareModelBinderProvider', mandatory. Provides model binding.

```csharp
config.Services.Insert(typeof(ModelBinderProvider), 0, new BodyAwareModelBinderProvider());
```
The ErrorHandlingActionInvoker is an abstract class that you may want to implement. It will ease error handling in the request pipeline. 
This will ensure that all 'unhandled' exceptions thrown from the controller actions will return a response object and if desired, contain a custom error code.

```csharp
config.Services.Replace(typeof(IHttpActionInvoker), new MyErrorHandlingActionInvoker(logger));
```
## Parameters, Routes and Responses

**ResponseHelper**
Is an utility class used to wrap your data in a contract response with the following util methods: 
```csharp
 public static HttpResponseMessage BuildOkDataResponse<T>(this HttpRequestMessage request, T data);
 
 public static HttpResponseMessage BuildOkVoidResponse(this HttpRequestMessage request);
 
 public static HttpResponseMessage BuildOkStreamResponse(this HttpRequestMessage request, Stream stream, string mediaType);
```
Example 

**Get Requests**

*Note that the RestfulRoute must match the resource identifier uri of the request object.*
*Note the [FromUri] that is needed on HTTP Get requests.*

```csharp
public class GetController : ApiController
{
	[RestfulRoute("api/MyRequestResource/{Id}")]
	public HttpResponseMessage Get([FromUri] MyGetRequest request)
	{
		try
		{
			var result = GetSomeDataForId(request.GetResourceIdentifier().Id);

			return Request.BuildOkDataResponse(new MyGetRequestResponse 
			{
				SomeProperty = result
			});
		}
		catch(SomeEntityNotFoundException ex)
		{
			return Request.CreateResponse(HttpStatusCode.NotFound);
		}
		catch(Exception ex)
		{
			return Request.CreateResponse(HttpStatusCode.InternalServiceError);
		}
	}
}
```
**Other requests**
```csharp
public class PostController : ApiController
{
	[RestfulRoute("api/MyRequestResource/{Id}")]
	public HttpResponseMessage Post(MyPostRequest request)
	{
		try
		{
			CreateSomeData(request.GetResourceIdentifier().Id, request.Property);

			return Request.BuildOkVoidResponse();
		}
		catch(SomeEntityNotFoundException ex)
		{
			return Request.CreateResponse(HttpStatusCode.NotFound);
		}
		catch(Exception ex)
		{
			return Request.CreateResponse(HttpStatusCode.InternalServerError);
		}
	}
	
	///...
}