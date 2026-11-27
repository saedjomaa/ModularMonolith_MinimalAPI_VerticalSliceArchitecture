using System.Reflection;
using CourseHub.Shared.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CourseHub.Shared;

public static class SharedServiceExtensions
{
    /// <summary>Registers the building blocks used by every module.</summary>
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<IEventBus, InMemoryEventBus>();
        return services;
    }

    /*
     تسجيل الـ Handlers تلقائي: الدالة AddIntegrationEventHandlersFrom(assembly) في SharedServiceExtensions.cs تبحث عن كل Handler في الوحدة وتسجّله في DI. كل وحدة تستدعيها في دالة AddXModule.
     */
    /// <summary>
    /// Registers every <see cref="IIntegrationEventHandler{TEvent}"/> found in <paramref name="assembly"/> as scoped.
    /// </summary>
    public static IServiceCollection AddIntegrationEventHandlersFrom(this IServiceCollection services, Assembly assembly)
    {
        var registrations = assembly.DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .SelectMany(type => type.ImplementedInterfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
                .Select(i => ServiceDescriptor.Scoped(i, type)));

        services.TryAddEnumerable(registrations);
        return services;
    }
}
