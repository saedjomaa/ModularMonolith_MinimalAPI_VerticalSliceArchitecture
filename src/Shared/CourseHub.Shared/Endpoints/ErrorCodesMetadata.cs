using CourseHub.Shared.Results;

namespace CourseHub.Shared.Endpoints;

/// <summary>
/// Endpoint metadata listing the errors an endpoint can return. The API's OpenAPI
/// configuration turns it into response descriptions (code + description per status).
/// </summary>
public sealed record ErrorCodesMetadata(IReadOnlyList<Error> Errors);
