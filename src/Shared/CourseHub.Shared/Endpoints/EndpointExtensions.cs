using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace CourseHub.Shared.Endpoints;

public static class EndpointExtensions
{
    /*
     يحتوي على دالة واحدة اسمها MapEndpointsFrom(assembly)، وتعمل بثلاث خطوات:
    تبحث في الـ Assembly الخاص بالوحدة عن كل كلاس يطبّق IEndpoint، بشرط ألا يكون abstract ولا interface. البحث يشمل الكلاسات internal، وهذا مهم لأن كل شيء في الوحدة internal.
    تنشئ نسخة من كل كلاس عبر Activator.CreateInstance(type, nonPublic: true).
    تستدعي Map(app) على كل نسخة، فيُسجَّل الـ Endpoint داخل مجموعة الوحدة.
     */
    /// <summary>
    /// Finds every <see cref="IEndpoint"/> in <paramref name="assembly"/> (internal types included)
    /// and maps it onto <paramref name="app"/>.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpointsFrom(this IEndpointRouteBuilder app, Assembly assembly)
    {
        var endpointTypes = assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoint)));

        foreach (var type in endpointTypes)
        {
            var endpoint = (IEndpoint)Activator.CreateInstance(type, nonPublic: true)!;
            endpoint.Map(app);
        }

        return app;
    }
}
