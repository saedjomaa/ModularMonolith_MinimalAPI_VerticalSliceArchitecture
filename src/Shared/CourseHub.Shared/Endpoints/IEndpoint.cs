using Microsoft.AspNetCore.Routing;

namespace CourseHub.Shared.Endpoints;
/*
 هذه واجهة صغيرة فيها دالة واحدة اسمها Map. كل Slice يكتب كلاسًا يطبّق هذه الواجهة، ويعرّف داخلها الـ Route والـ HTTP Method والصلاحيات.
 */
/// <summary>
/// A Minimal API endpoint belonging to one vertical slice.
/// <paramref name="app"/> is the module's route group (e.g. <c>/courses</c>).
/// </summary>
public interface IEndpoint
{
    void Map(IEndpointRouteBuilder app);
}
