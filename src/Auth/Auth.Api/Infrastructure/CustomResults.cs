using SharedKernel;

namespace Auth.Api.Infrastructure;

internal static class CustomResults
{
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Can't create a problem response for a success result");
        }

        return Results.Problem(
            title: result.Error.Code,
            detail: result.Error.Description,
            statusCode: GetStatusCode(result.Error.Type));
    }

    public static IResult Problem<TValue>(Result<TValue> result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Can't create a problem response for a success result");
        }

        return Results.Problem(
            title: result.Error.Code,
            detail: result.Error.Description,
            statusCode: GetStatusCode(result.Error.Type));
    }

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
}
