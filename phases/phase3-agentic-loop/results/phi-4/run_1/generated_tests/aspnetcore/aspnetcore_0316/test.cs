using System;
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
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>())
                .Returns(new Mock<IOptionsMonitor<BearerTokenOptions>>().Object);

            var endpointsMock = new Mock<IEndpointRouteBuilder>();
            endpointsMock.Setup(e => e.ServiceProvider).Returns(serviceProviderMock.Object);

            // Act
            IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(endpointsMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>(), Times.Once);
        }
    }
}
