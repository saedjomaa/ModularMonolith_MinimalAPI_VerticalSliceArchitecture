using CourseHub.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace CourseHub.Shared.Endpoints;

/*
 هدف هذا الملف: أن "يعرف" كل Endpoint كل الاستجابات الممكنة، فتظهر في OpenAPI / Scalar.
 - ProducesErrors: يكتبها كل Slice ليعلن أخطاء العمل الخاصة به (مثل 409 Users.EmailTaken).
 - WithStandardResponses: تُطبَّق مرة واحدة على كل الـ Endpoints، وتضيف تلقائيًا
   400 (إذا كان هناك Body) و401 و403 (حسب الصلاحيات) و500.
 */
public static class EndpointResponseExtensions
{
    private static readonly string[] ProblemContentTypes = ["application/problem+json"];

    /// <summary>
    /// Declares the business errors this endpoint can return. Adds one documented problem response
    /// per status code (<see cref="ApiValidationProblem"/> for 400, otherwise <see cref="ApiProblem"/>)
    /// and lists the error codes in the response description.
    /// </summary>
    public static TBuilder ProducesErrors<TBuilder>(this TBuilder builder, params Error[] errors)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(endpoint => AddErrorMetadata(endpoint.Metadata, errors));
        return builder;
    }

    /// <summary>
    /// Adds the responses every endpoint can produce, based on what the endpoint is:
    /// 400 when it reads a body, 401 when it requires authentication, 403 when it requires
    /// a policy or role, and 500 always. Apply it once to the group that holds all modules.
    /// </summary>
    public static TBuilder WithStandardResponses<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.Finally(endpoint =>
        {
            var metadata = endpoint.Metadata;
            var standard = new List<Error>();

            if (metadata.OfType<IAcceptsMetadata>().Any())
            {
                standard.Add(StandardErrors.ValidationFailed);
            }

            var authorize = metadata.OfType<IAuthorizeData>().ToList();
            if (authorize.Count > 0 && !metadata.OfType<IAllowAnonymous>().Any())
            {
                standard.Add(StandardErrors.Unauthorized);

                if (authorize.Any(a => a.Policy is not null || a.Roles is not null))
                {
                    standard.Add(StandardErrors.Forbidden);
                }
            }

            standard.Add(StandardErrors.Unexpected);
            AddErrorMetadata(metadata, standard);
        });

        return builder;
    }

    private static void AddErrorMetadata(IList<object> metadata, IReadOnlyList<Error> errors)
    {
        foreach (var status in errors.Select(e => e.Type.ToStatusCode()).Distinct())
        {
            var alreadyDeclared = metadata.OfType<IProducesResponseTypeMetadata>().Any(m => m.StatusCode == status);
            if (alreadyDeclared)
            {
                continue;
            }

            var problemType = status == StatusCodes.Status400BadRequest ? typeof(ApiValidationProblem) : typeof(ApiProblem);
            metadata.Add(new ProducesResponseTypeMetadata(status, problemType, ProblemContentTypes));
        }

        metadata.Add(new ErrorCodesMetadata(errors));
    }
}
