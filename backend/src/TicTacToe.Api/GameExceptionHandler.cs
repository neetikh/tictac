using Microsoft.AspNetCore.Diagnostics;
using TicTacToe.Domain;

namespace TicTacToe.Api;

public sealed class GameExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case GameException invalid:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(new { error = invalid.Message }, cancellationToken);
                return true;
            case GameNotFoundException notFound:
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await httpContext.Response.WriteAsJsonAsync(new { error = notFound.Message }, cancellationToken);
                return true;
            default:
                return false;
        }
    }
}
