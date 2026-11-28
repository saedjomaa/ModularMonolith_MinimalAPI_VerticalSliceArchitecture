using Microsoft.AspNetCore.Http;

namespace CourseHub.Shared.Results;

/*
 أخطاء عامة لا تخص وحدة معيّنة، ويولّدها الـ Framework نفسه (Validation، المصادقة، الصلاحيات، المسار غير الموجود، الأعطال).
 نعطيها code ثابتًا حتى يكون لكل استجابة خطأ نفس الشكل، سواء جاءت من الـ Domain أو من الـ Framework.
 */
/// <summary>
/// Errors produced by the framework rather than a module (validation filter, authentication,
/// authorization, unknown routes, unhandled exceptions). Giving them stable codes keeps every
/// error response in the same shape.
/// </summary>
public static class StandardErrors
{
    public static readonly Error ValidationFailed =
        Error.Validation("Validation.Failed", "One or more request fields are invalid. See 'errors'.");

    public static readonly Error Unauthorized =
        Error.Unauthorized("Auth.Unauthorized", "Authentication is required. Send 'Authorization: Bearer <accessToken>'.");

    public static readonly Error Forbidden =
        Error.Forbidden("Auth.Forbidden", "You don't have permission to perform this action.");

    public static readonly Error RouteNotFound =
        Error.NotFound("Route.NotFound", "The requested resource was not found.");

    public static readonly Error Unexpected =
        Error.Unexpected("Server.Unexpected", "An unexpected error occurred. Use 'traceId' when reporting it.");

    /// <summary>The code to use for a framework-produced problem that has no code of its own.</summary>
    public static string CodeFor(int? statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => ValidationFailed.Code,
        StatusCodes.Status401Unauthorized => Unauthorized.Code,
        StatusCodes.Status403Forbidden => Forbidden.Code,
        StatusCodes.Status404NotFound => RouteNotFound.Code,
        StatusCodes.Status405MethodNotAllowed => "Route.MethodNotAllowed",
        StatusCodes.Status415UnsupportedMediaType => "Request.UnsupportedMediaType",
        >= 500 => Unexpected.Code,
        _ => "Request.Failed"
    };
}
