using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Tests
{
    public class IdentityApiEndpointRouteBuilderExtensionsTests
    {
        [Fact]
        public void MapIdentityApi_ShouldRetrieveBearerTokenOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var bearerTokenOptionsMock = new Mock<IOptionsMonitor<BearerTokenOptions>>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>())
                .Returns(bearerTokenOptionsMock.Object);

            var endpointsMock = new Mock<IEndpointRouteBuilder>();
            endpointsMock
                .Setup(ep => ep.ServiceProvider)
                .Returns(serviceProviderMock.Object);

            // Act
            Microsoft.AspNetCore.Routing.IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<object>(endpointsMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>(), Times.Once);
        }
    }
}
