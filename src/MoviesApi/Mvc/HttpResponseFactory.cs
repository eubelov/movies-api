using Microsoft.AspNetCore.Mvc;

namespace MoviesApi.Mvc;

public class HttpResponseFactory
{
    public static ObjectResult UnknownErrorResponse()
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Unexpected error occurred",
            Title = "Unexpected Error",
            Type = "https://movies-ui.untrap.net/api/unexpected-error",
        };

        return new(problem)
        {
            StatusCode = StatusCodes.Status500InternalServerError,
        };
    }
}