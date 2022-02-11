using Microsoft.AspNetCore.Mvc;

using MoviesApi.Features.GetMovies;

namespace MoviesApi.Endpoints.Movies;

public sealed class GetMovies : EndpointBase
{
    /// <summary>
    /// Gets a list of movies.
    /// </summary>
    /// <response code="200">List of movies.</response>
    /// <response code="500">An unexpected error happened.</response>
    [HttpGet("movies", Name = "GetMovies")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMoviesResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    [ApiExplorerSettings(GroupName = "Movies")]
    public async Task<IActionResult> Execute(CancellationToken cancellationToken)
    {
        return await this.Send(new GetMoviesRequest(), this.Ok, cancellationToken);
    }
}