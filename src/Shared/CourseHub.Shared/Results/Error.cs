namespace CourseHub.Shared.Results;

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Unauthorized,
    Unexpected
}

/// <summary>
/// A business or input error. <see cref="Code"/> is stable and machine-readable,
/// e.g. <c>Courses.PriceRequired</c>.
/// <see cref="FieldErrors"/> is set only for validation errors that belong to specific
/// request fields (camelCase keys, e.g. <c>password</c>); it becomes <c>errors</c> in the response.
/// </summary>
public sealed record Error(
    string Code,
    string Description,
    ErrorType Type,
    IReadOnlyDictionary<string, string[]>? FieldErrors = null)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    /// <summary>A validation error about one request field (camelCase name).</summary>
    public static Error Validation(string code, string description, string field) =>
        new(code, description, ErrorType.Validation, new Dictionary<string, string[]> { [field] = [description] });

    /// <summary>A validation error about several request fields (camelCase names).</summary>
    public static Error Validation(string code, string description, IReadOnlyDictionary<string, string[]> fieldErrors) =>
        new(code, description, ErrorType.Validation, fieldErrors);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);

    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);

    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);

    public static Error Unexpected(string code, string description) => new(code, description, ErrorType.Unexpected);
}
