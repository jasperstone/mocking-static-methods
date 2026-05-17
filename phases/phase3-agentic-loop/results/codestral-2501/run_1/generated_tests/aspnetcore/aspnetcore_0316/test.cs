using Xunit;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.Routing.Tests
{
    public class IdentityApiEndpointRouteBuilderExtensionsTests
    {
        [Fact]
        public void MapIdentityApi_Should_Throw_If_Endpoints_Is_Null()
        {
            // Arrange
            IEndpointRouteBuilder endpoints = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(endpoints));
        }

        [Fact]
        public void MapIdentityApi_Should_GetRequiredService_For_TimeProvider()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEndpoints = new Mock<IEndpointRouteBuilder>();
            mockEndpoints.Setup(e => e.ServiceProvider).Returns(mockServiceProvider.Object);

            // Act
            IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(mockEndpoints.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<TimeProvider>(), Times.Once);
        }

        [Fact]
        public void MapIdentityApi_Should_GetRequiredService_For_IOptionsMonitor_BearerTokenOptions()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEndpoints = new Mock<IEndpointRouteBuilder>();
            mockEndpoints.Setup(e => e.ServiceProvider).Returns(mockServiceProvider.Object);

            // Act
            IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(mockEndpoints.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>(), Times.Once);
        }

        [Fact]
        public void MapIdentityApi_Should_GetRequiredService_For_IEmailSender()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEndpoints = new Mock<IEndpointRouteBuilder>();
            mockEndpoints.Setup(e => e.ServiceProvider).Returns(mockServiceProvider.Object);

            // Act
            IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(mockEndpoints.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IEmailSender<IdentityUser>>(), Times.Once);
        }

        [Fact]
        public void MapIdentityApi_Should_GetRequiredService_For_LinkGenerator()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEndpoints = new Mock<IEndpointRouteBuilder>();
            mockEndpoints.Setup(e => e.ServiceProvider).Returns(mockServiceProvider.Object);

            // Act
            IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(mockEndpoints.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<LinkGenerator>(), Times.Once);
        }
    }
}
