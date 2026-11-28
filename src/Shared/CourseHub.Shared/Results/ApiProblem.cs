namespace CourseHub.Shared.Results;

/*
 هذان النوعان للتوثيق فقط: يظهران في OpenAPI / Scalar كشكل موحّد لكل استجابات الخطأ.
 الاستجابة الفعلية تُكتب كـ ProblemDetails، لكن بنفس هذه الحقول تمامًا.
 */
/// <summary>
/// Documentation-only shape of every error response (RFC 9457 problem details plus
/// <c>code</c> and <c>traceId</c>). Endpoints declare it through
/// <see cref="Endpoints.EndpointResponseExtensions.ProducesErrors{TBuilder}"/>.
/// </summary>
public class ApiProblem
{
    /// <summary>A URI that identifies the problem type.</summary>
    public string? Type { get; init; }

    /// <summary>The HTTP reason phrase, e.g. "Conflict".</summary>
    public string? Title { get; init; }

    public int Status { get; init; }

    /// <summary>Human-readable explanation of this occurrence.</summary>
    public string? Detail { get; init; }

    /// <summary>The request path.</summary>
    public string? Instance { get; init; }

    /// <summary>Stable, machine-readable error code, e.g. <c>Users.EmailTaken</c>.</summary>
    public required string Code { get; init; }

    /// <summary>Correlates the response with server logs.</summary>
    public required string TraceId { get; init; }
}

/// <summary>Shape of a 400 response: <see cref="ApiProblem"/> plus errors per request field (camelCase keys).</summary>
public sealed class ApiValidationProblem : ApiProblem
{
    public required IDictionary<string, string[]> Errors { get; init; }
}
