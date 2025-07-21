using Microsoft.Extensions.DependencyInjection;

namespace Hamelin.Tests.Unit.Helpers;

public class ServiceScopeHelpers
{
    public static IServiceScope CreateScope(IServiceProvider serviceProvider)
    {
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        return scope;
    }

    public static IServiceScopeFactory CreateScopeFactory(IServiceProvider serviceProvider)
    {
        var scope = CreateScope(serviceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        return scopeFactory;
    }
}
