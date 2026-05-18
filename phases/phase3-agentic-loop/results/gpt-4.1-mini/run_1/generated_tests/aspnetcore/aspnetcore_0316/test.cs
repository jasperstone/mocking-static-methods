using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Routing
{
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
        public void MapIdentityApi_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            var timeProvider = TimeProvider.System;
            var bearerTokenOptionsMock = new Mock<IOptionsMonitor<BearerTokenOptions>>();
            var emailSenderMock = new Mock<IEmailSender<TestUser>>();
            var linkGeneratorMock = new Mock<LinkGenerator>();

            // Setup GetRequiredService extension method behavior by using the real extension method
            serviceProviderMock.Setup(sp => sp.GetRequiredService<TimeProvider>()).Returns(timeProvider);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>()).Returns(bearerTokenOptionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IEmailSender<TestUser>>()).Returns(emailSenderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<LinkGenerator>()).Returns(linkGeneratorMock.Object);

            var endpointRouteBuilderMock = new Mock<IEndpointRouteBuilder>();
            endpointRouteBuilderMock.SetupGet(e => e.ServiceProvider).Returns(serviceProviderMock.Object);

            // Setup MapGroup to return a mock IEndpointRouteBuilder to avoid null reference
            var routeGroupMock = new Mock<IEndpointRouteBuilder>();
            endpointRouteBuilderMock.Setup(e => e.MapGroup(It.IsAny<string>())).Returns(routeGroupMock.Object);

            // Act
            var result = IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<TestUser>(endpointRouteBuilderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<TimeProvider>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IEmailSender<TestUser>>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<LinkGenerator>(), Times.Once);
            Assert.NotNull(result);
        }
    }
}
