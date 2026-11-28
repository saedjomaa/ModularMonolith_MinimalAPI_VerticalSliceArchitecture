namespace CourseHub.Shared.Handlers;

/// <summary>
/// Marks a vertical slice's handler so it is registered in DI automatically
/// (see <see cref="SharedServiceExtensions.AddHandlersFrom"/>). The endpoint
/// receives the handler as a lambda parameter.
/// </summary>
public interface IHandler;
