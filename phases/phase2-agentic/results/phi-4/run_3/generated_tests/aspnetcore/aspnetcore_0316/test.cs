using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Tests
{
    public class IdentityApiEndpointRouteBuilderExtensionsTests
    {
        [Fact]
        public void MapIdentityApi_CallsGetRequiredServiceForBearerTokenOptions()
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
            IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<object>(endpointsMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>(), Times.Once);
        }
    }
}
