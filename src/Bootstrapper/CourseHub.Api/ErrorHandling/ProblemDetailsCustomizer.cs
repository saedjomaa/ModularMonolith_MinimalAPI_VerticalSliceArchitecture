using System.Diagnostics;
using CourseHub.Shared.Results;

namespace CourseHub.Api.ErrorHandling;

/*
 يعمل على كل ProblemDetails يكتبه الـ Framework (Validation، 401/403، 404، 500)،
 ويكمله ليصبح بنفس شكل أخطاء الـ Domain: code و traceId و instance، ومفاتيح errors بصيغة camelCase.
 */
/// <summary>
/// Completes every problem response written through <see cref="IProblemDetailsService"/> so all
/// errors share one shape (<see cref="ApiProblem"/>): adds <c>code</c>, <c>traceId</c> and
/// <c>instance</c>, and converts validation <c>errors</c> keys to camelCase.
/// </summary>
internal static class ProblemDetailsCustomizer
{
    public static void Apply(ProblemDetailsContext context)
    {
        var problem = context.ProblemDetails;
        var httpContext = context.HttpContext;

        problem.Instance ??= httpContext.Request.Path;
        problem.Extensions.TryAdd("traceId", Activity.Current?.Id ?? httpContext.TraceIdentifier);
        problem.Extensions.TryAdd("code", StandardErrors.CodeFor(problem.Status));

        if (problem is HttpValidationProblemDetails validation && validation.Errors.Count > 0)
        {
            var camelCased = ResultHttpExtensions.ToCamelCaseKeys(validation.Errors);
            validation.Errors.Clear();
            foreach (var (field, messages) in camelCased)
            {
                validation.Errors[field] = messages;
            }
        }
    }
}
