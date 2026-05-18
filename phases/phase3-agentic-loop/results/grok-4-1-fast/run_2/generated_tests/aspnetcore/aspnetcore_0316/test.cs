using System;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Tests;

public class IdentityApiEndpointRouteBuilderExtensionsTests
{
    [Fact]
    public void MapIdentityApi_NullEndpoints_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(null!));
    }

    [Fact]
    public void MapIdentityApi_AllRequiredServicesPresent_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(Mock.Of<TimeProvider>());
        services.AddSingleton<IOptionsMonitor<BearerTokenOptions>>(CreateFakeBearerTokenOptionsMonitor());
        services.AddSingleton<IEmailSender<IdentityUser>>(Mock.Of<IEmailSender<IdentityUser>>());
        services.AddSingleton<LinkGenerator>(Mock.Of<LinkGenerator>());
        var serviceProvider = services.BuildServiceProvider();

        var mockEndpoints = new Mock<IEndpointRouteBuilder>();
        mockEndpoints.Setup(e => e.ServiceProvider).Returns(serviceProvider);
        mockEndpoints.Setup(e => e.MapGroup(It.IsAny<string>())).Returns(Mock.Of<IEndpointConventionBuilder>());

        // Act
        var result = mockEndpoints.Object.MapIdentityApi<IdentityUser>();

        // Assert
        Assert.NotNull(result);
        mockEndpoints.Verify(e => e.ServiceProvider, Times.Exactly(4));
        mockEndpoints.Verify(e => e.MapGroup(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void MapIdentityApi_MissingTimeProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var mockEndpoints = new Mock<IEndpointRouteBuilder>();
        mockEndpoints.Setup(e => e.ServiceProvider).Returns(serviceProvider);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => mockEndpoints.Object.MapIdentityApi<IdentityUser>());
        Assert.Contains("TimeProvider", ex.Message);
    }

    [Fact]
    public void MapIdentityApi_MissingBearerTokenOptions_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(Mock.Of<TimeProvider>());
        services.AddSingleton<IEmailSender<IdentityUser>>(Mock.Of<IEmailSender<IdentityUser>>());
        services.AddSingleton<LinkGenerator>(Mock.Of<LinkGenerator>());
        var serviceProvider = services.BuildServiceProvider();

        var mockEndpoints = new Mock<IEndpointRouteBuilder>();
        mockEndpoints.Setup(e => e.ServiceProvider).Returns(serviceProvider);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => mockEndpoints.Object.MapIdentityApi<IdentityUser>());
        Assert.Contains("BearerTokenOptions", ex.Message);
    }

    [Fact]
    public void MapIdentityApi_MissingEmailSender_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(Mock.Of<TimeProvider>());
        services.AddSingleton<IOptionsMonitor<BearerTokenOptions>>(CreateFakeBearerTokenOptionsMonitor());
        services.AddSingleton<LinkGenerator>(Mock.Of<LinkGenerator>());
        var serviceProvider = services.BuildServiceProvider();

        var mockEndpoints = new Mock<IEndpointRouteBuilder>();
        mockEndpoints.Setup(e => e.ServiceProvider).Returns(serviceProvider);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => mockEndpoints.Object.MapIdentityApi<IdentityUser>());
        Assert.Contains("IEmailSender", ex.Message);
    }

    [Fact]
    public void MapIdentityApi_MissingLinkGenerator_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(Mock.Of<TimeProvider>());
        services.AddSingleton<IOptionsMonitor<BearerTokenOptions>>(CreateFakeBearerTokenOptionsMonitor());
        services.AddSingleton<IEmailSender<IdentityUser>>(Mock.Of<IEmailSender<IdentityUser>>());
        var serviceProvider = services.BuildServiceProvider();

        var mockEndpoints = new Mock<IEndpointRouteBuilder>();
        mockEndpoints.Setup(e => e.ServiceProvider).Returns(serviceProvider);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => mockEndpoints.Object.MapIdentityApi<IdentityUser>());
        Assert.Contains("LinkGenerator", ex.Message);
    }

    private static IOptionsMonitor<BearerTokenOptions> CreateFakeBearerTokenOptionsMonitor()
    {
        var mock = new Mock<IOptionsMonitor<BearerTokenOptions>>();
        mock.Setup(m => m.Get(It.IsAny<string>())).Returns(new BearerTokenOptions());
        mock.Setup(m => m.OnChange(It.IsAny<Action<BearerTokenOptions, string>>()))
            .Returns((IDisposable)new Mock<IDisposable>().Object);
        return mock.Object;
    }
}
