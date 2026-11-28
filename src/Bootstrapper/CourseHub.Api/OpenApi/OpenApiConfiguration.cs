using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CourseHub.Api.OpenApi;

internal static class OpenApiConfiguration
{
    public const string BearerSchemeName = "Bearer";

    public static IServiceCollection AddCourseHubOpenApi(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
            // Slices all use nested "Request"/"Response" records; name schemas after the slice
            // (e.g. "RegisterRequest") so they don't collide in the document.
            options.CreateSchemaReferenceId = typeInfo => typeInfo.Type.IsNested && typeInfo.Type.DeclaringType is { } slice
                ? slice.Name + typeInfo.Type.Name
                : OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);

            // Lets Scalar send "Authorization: Bearer <token>" (Identity bearer token from /users/login).
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes[BearerSchemeName] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Description = "Access token from POST /users/login"
                };

                document.Security ??= [];
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(BearerSchemeName, document)] = []
                });

                return Task.CompletedTask;
            });

            // Lists the possible error codes under each error response, from ErrorCodesMetadata
            // (ProducesErrors on slices + WithStandardResponses on the root group).
            options.AddOperationTransformer((operation, context, _) =>
            {
                ErrorCodesDescriber.Describe(operation, context.Description.ActionDescriptor.EndpointMetadata);
                return Task.CompletedTask;
            });
        });
}
