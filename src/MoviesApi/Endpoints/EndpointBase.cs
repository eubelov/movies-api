using MediatR;

using Microsoft.AspNetCore.Mvc;

using MoviesApi.Models;
using MoviesApi.Mvc;
using MoviesApi.Mvc.Attributes;

namespace MoviesApi.Endpoints;

[ApiController]
[Produces("application/json")]
[VersionedEndpoint("1")]
public abstract class EndpointBase : ControllerBase
{
    private IMediator? mediator;

    private ILogger<EndpointBase>? logger;

    private IMediator Mediator => this.mediator ??= this.HttpContext.RequestServices.GetRequiredService<IMediator>();

    private ILogger<EndpointBase> Logger => this.logger ??= this.HttpContext.RequestServices.GetRequiredService<ILogger<EndpointBase>>();

    protected async Task<IActionResult> Send<TResponse>(
        IRequest<MediatorResponse<TResponse>> request,
        Func<TResponse, IActionResult> onResult,
        CancellationToken cancellationToken)
    {
        var result = await this.Mediator.Send(request, cancellationToken);

        return this.OnRequestExecuted(result, onResult);
    }

    private IActionResult OnRequestExecuted<T>(MediatorResponse<T> response, Func<T, IActionResult> onResult)
    {
        var exception = response.Exception;
        if (exception is null)
        {
            return onResult(response.Result!);
        }

        this.Logger.LogError(exception, "Exception occurred");

        return exception switch
        {
            _ => HttpResponseFactory.UnknownErrorResponse(),
        };
    }
}