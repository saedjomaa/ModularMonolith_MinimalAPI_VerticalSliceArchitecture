using NetArchTest.Rules;
using Shouldly;

namespace CourseHub.ArchitectureTests;

/// <summary>Dependency rules from docs/Architecture.md §3.</summary>
public sealed class ModuleBoundaryTests
{
    [Fact]
    public void Shared_DoesNotDependOnAnyModule()
    {
        Modules.SolutionReferences(Modules.Shared).ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(Modules.Names), MemberType = typeof(Modules))]
    public void Module_DependsOnlyOnSharedAndContracts(string module)
    {
        var forbidden = Modules.SolutionReferences(Modules.Main[module])
            .Where(name => name != Modules.Shared.GetName().Name && !name.EndsWith(".Contracts", StringComparison.Ordinal));

        forbidden.ShouldBeEmpty($"module '{module}' must reach other modules only through their Contracts");
    }

    [Theory]
    [MemberData(nameof(Modules.Names), MemberType = typeof(Modules))]
    public void Contracts_DependOnlyOnShared(string module)
    {
        var forbidden = Modules.SolutionReferences(Modules.Contracts(module))
            .Where(name => name != Modules.Shared.GetName().Name);

        forbidden.ShouldBeEmpty($"'{module}.Contracts' may reference only CourseHub.Shared");
    }

    [Theory]
    [MemberData(nameof(Modules.Names), MemberType = typeof(Modules))]
    public void Contracts_DoNotUseEfCoreOrAspNetCore(string module)
    {
        var result = Types.InAssembly(Modules.Contracts(module))
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Theory]
    [MemberData(nameof(Modules.Names), MemberType = typeof(Modules))]
    public void Module_ExposesOnlyItsRegistrationClass(string module)
    {
        var result = Types.InAssembly(Modules.Main[module])
            .That()
            .DoNotHaveName($"{module}Module")
            .And()
            .DoNotResideInNamespaceEndingWith(".Infrastructure.Migrations") // EF Core generates public migration classes
            .ShouldNot()
            .BePublic()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(string.Join(", ", result.FailingTypeNames ?? []));
    }
}
