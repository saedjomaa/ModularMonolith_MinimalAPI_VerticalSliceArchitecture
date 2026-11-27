using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CourseHub.Shared.Results;

public static class ResultHttpExtensions
{
    /// <summary>
    /// Maps a failed <see cref="Result"/> to an RFC 7807 <c>ProblemDetails</c> response.
    /// </summary>
    public static ProblemHttpResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert a successful result to a problem.");
        }

        return result.Error.ToProblem();
    }

    public static ProblemHttpResult ToProblem(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return TypedResults.Problem(
            statusCode: statusCode,
            title: error.Code,
            detail: error.Description,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
