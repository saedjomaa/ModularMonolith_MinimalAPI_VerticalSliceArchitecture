using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CourseHub.Shared.Results;

public static class ResultHttpExtensions
{

    /*
     كيف تعمل الثلاثة ملفات معًا؟
    Domain:    course.Publish() → حصل خطأ لان اليوزر لم يدخل السعر → Result.Failure(CourseErrors.PriceRequired)
    Handler:   يرجع الـ Result كما هو
    Endpoint:  result.IsSuccess ? TypedResults.NoContent() : result.ToProblem()
    Client:    409 + ProblemDetails

    تظهر بهذا الشكل (نفس الشكل لكل الأخطاء، وأخطاء 400 تضيف errors لكل حقل)
     {
          "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
          "title": "Conflict",
          "status": 409,
          "detail": "A course cannot be published without a price.",
          "instance": "/courses/{id}/publish",
          "code": "Courses.PriceRequired",
          "traceId": "00-..."
     }
     */
    /// <summary>
    /// Maps a failed <see cref="Result"/> to an RFC 9457 problem details response
    /// (see <see cref="ApiProblem"/> for the shape).
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
        ProblemDetails problem = error.Type == ErrorType.Validation
            ? new HttpValidationProblemDetails(ToCamelCaseKeys(
                error.FieldErrors ?? new Dictionary<string, string[]> { [string.Empty] = [error.Description] }))
            : new ProblemDetails();

        problem.Status = error.Type.ToStatusCode();
        problem.Detail = error.Description;
        problem.Extensions["code"] = error.Code;

        // type, title, instance and traceId are filled by the API's ProblemDetails customization.
        return TypedResults.Problem(problem);
    }

    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError
    };

    /// <summary>Converts field names to camelCase (e.g. <c>PhoneNumber</c> → <c>phoneNumber</c>) to match the JSON body.</summary>
    public static Dictionary<string, string[]> ToCamelCaseKeys(IEnumerable<KeyValuePair<string, string[]>> errors) =>
        errors
            .GroupBy(pair => JsonNamingPolicy.CamelCase.ConvertName(pair.Key))
            .ToDictionary(group => group.Key, group => group.SelectMany(pair => pair.Value).ToArray());
}
