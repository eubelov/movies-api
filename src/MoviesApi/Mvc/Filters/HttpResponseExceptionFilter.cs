﻿using Microsoft.AspNetCore.Mvc.Filters;

namespace MoviesApi.Mvc.Filters;

public sealed class HttpResponseExceptionFilter : IAsyncActionFilter
{
    private readonly ILogger<HttpResponseExceptionFilter> logger;

    public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger)
    {
        this.logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();
        if (result.Exception is null)
        {
            return;
        }

        this.logger.LogError(result.Exception, "Unhandled exception occured");

        result.Result = HttpResponseFactory.UnknownErrorResponse();
        result.ExceptionHandled = true;
    }
}