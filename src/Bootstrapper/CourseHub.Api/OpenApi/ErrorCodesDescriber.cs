using System.Globalization;
using CourseHub.Shared.Endpoints;
using CourseHub.Shared.Results;
using Microsoft.OpenApi;

namespace CourseHub.Api.OpenApi;

/// <summary>
/// Writes the possible error codes into each error response's description, e.g.
/// <c>409 Conflict — Users.EmailTaken: An account with this email already exists.</c>
/// </summary>
internal static class ErrorCodesDescriber
{
    public static void Describe(OpenApiOperation operation, IList<object> endpointMetadata)
    {
        if (operation.Responses is null)
        {
            return;
        }

        var errorsByStatus = endpointMetadata
            .OfType<ErrorCodesMetadata>()
            .SelectMany(m => m.Errors)
            .DistinctBy(e => e.Code)
            .GroupBy(e => e.Type.ToStatusCode().ToString(CultureInfo.InvariantCulture));

        foreach (var group in errorsByStatus)
        {
            if (operation.Responses.TryGetValue(group.Key, out var response) && response is OpenApiResponse concrete)
            {
                var codes = group.Select(e => $"- `{e.Code}`: {e.Description}");
                concrete.Description = $"{concrete.Description}\n\nPossible codes:\n{string.Join('\n', codes)}";
            }
        }
    }
}
