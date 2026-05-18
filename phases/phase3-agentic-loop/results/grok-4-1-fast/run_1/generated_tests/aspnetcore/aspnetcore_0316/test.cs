using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Tests;

public class IdentityApiEndpointRouteBuilderExtensionsTests
{
    [Fact]
    public void MapIdentityApi_GetRequiredServiceIOptionsMonitorBearerTokenOptions_CalledOnce()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<TimeProvider>()).Returns(Mock.Of<TimeProvider>());
        mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>())
                          .Returns(Mock.Of<IOptionsMonitor<BearerTokenOptions>>());
        mockServiceProvider.Setup(sp => sp.GetRequiredService<IEmailSender<object>>())
                          .Returns(Mock.Of<IEmailSender<object>>());
        mockServiceProvider.Setup(sp => sp.GetRequiredService<LinkGenerator>())
                          .Returns(Mock.Of<LinkGenerator>());

        var mockEndpoints = new Mock<IEndpointRouteBuilder>();
        mockEndpoints.Setup(e => e.ServiceProvider).Returns(mockServiceProvider.Object);
        mockEndpoints.Setup(e => e.MapGroup(It.IsAny<string>())).Returns(Mock.Of<IEndpointConventionBuilder>());

        // Act
        var result = IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<object>(mockEndpoints.Object);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>(), Times.Once());
        Assert.NotNull(result);
    }

    [Fact]
    public void MapIdentityApi_NullEndpoints_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<object>(null!));
    }
}
