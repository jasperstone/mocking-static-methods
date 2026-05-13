using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace Microsoft.AspNetCore.Routing;

public class IdentityApiEndpointRouteBuilderExtensionsTests
{
    private class TestUser { }

    [Fact]
    public void MapIdentityApi_ThrowsArgumentNullException_WhenEndpointsIsNull()
    {
        IEndpointRouteBuilder? endpoints = null;
        Assert.Throws<ArgumentNullException>(() => IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<TestUser>(endpoints!));
    }

    [Fact]
    public void MapIdentityApi_ResolvesRequiredServices()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var serviceCollection = new ServiceCollection();

        var timeProvider = TimeProvider.System;
        var bearerTokenOptionsMock = new Mock<IOptionsMonitor<BearerTokenOptions>>();
        var emailSenderMock = new Mock<IEmailSender<TestUser>>();
        var linkGeneratorMock = new Mock<LinkGenerator>();

        serviceCollection.AddSingleton(timeProvider);
        serviceCollection.AddSingleton(bearerTokenOptionsMock.Object);
        serviceCollection.AddSingleton(emailSenderMock.Object);
        serviceCollection.AddSingleton(linkGeneratorMock.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var endpointRouteBuilderMock = new Mock<IEndpointRouteBuilder>();
        endpointRouteBuilderMock.Setup(e => e.ServiceProvider).Returns(serviceProvider);
        endpointRouteBuilderMock.Setup(e => e.MapGroup(It.IsAny<string>())).Returns(Mock.Of<IEndpointRouteBuilder>());

        // Act
        var result = IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<TestUser>(endpointRouteBuilderMock.Object);

        // Assert
        Assert.NotNull(result);
    }
}
