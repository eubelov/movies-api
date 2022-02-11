using MediatR;

using MoviesApi.Models;

namespace MoviesApi.Features.GetMovies;

public sealed record GetMoviesRequest : IRequest<MediatorResponse<GetMoviesResponse>>;