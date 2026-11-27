using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CourseHub.Shared.Results;

public static class ResultHttpExtensions
{

    /*
     كيف تعمل الثلاثة ملفات معًا؟
    Domain:    course.Publish() → حصل خطأ لان اليوزر لم يدخل السعر → Result.Failure(CourseErrors.PriceRequired)
    Handler:   يرجع الـ Result كما هو
    Endpoint:  result.IsSuccess ? TypedResults.NoContent() : result.ToProblem()
    Client:    409 + ProblemDetails

    تظهر بهذا الشكل
     {
          "status": 409,
          "title": "Courses.PriceRequired",
          "detail": "A course cannot be published without a price.",
          "code": "Courses.PriceRequired"
     }
     */
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
