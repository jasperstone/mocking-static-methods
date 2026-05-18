using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IdentityApiEndpointRouteBuilderExtensionsTests
{
    public class IdentityApiEndpointRouteBuilderExtensionsTests
    {
        [Fact]
        public async Task MapIdentityApi_ValidInput_ReturnsIEndpointConventionBuilder()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<TimeProvider, TimeProvider>()
                .AddScoped<IOptionsMonitor<BearerTokenOptions>, OptionsMonitor<BearerTokenOptions>>()
                .AddScoped<IEmailSender<IdentityUser>, EmailSender<IdentityUser>>()
                .AddScoped<LinkGenerator, LinkGenerator>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
            endpointRouteBuilder.Setup(e => e.ServiceProvider).Returns(serviceProvider);

            // Act
            var result = IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(endpointRouteBuilder.Object);

            // Assert
            Assert.IsType<IEndpointConventionBuilder>(result);
        }

        [Fact]
        public async Task MapIdentityApi_InvalidInput_ThrowsArgumentNullException()
        {
            // Arrange
            IEndpointRouteBuilder endpointRouteBuilder = null;

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<IdentityUser>(endpointRouteBuilder));
        }

        [Fact]
        public async Task MapIdentityApi_GetRequiredService_TimeProvider_ReturnsTimeProvider()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<TimeProvider, TimeProvider>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
            endpointRouteBuilder.Setup(e => e.ServiceProvider).Returns(serviceProvider);

            // Act
            var timeProvider = endpointRouteBuilder.Object.ServiceProvider.GetRequiredService<TimeProvider>();

            // Assert
            Assert.IsType<TimeProvider>(timeProvider);
        }

        [Fact]
        public async Task MapIdentityApi_GetRequiredService_BearerTokenOptions_ReturnsBearerTokenOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IOptionsMonitor<BearerTokenOptions>, OptionsMonitor<BearerTokenOptions>>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
            endpointRouteBuilder.Setup(e => e.ServiceProvider).Returns(serviceProvider);

            // Act
            var bearerTokenOptions = endpointRouteBuilder.Object.ServiceProvider.GetRequiredService<IOptionsMonitor<BearerTokenOptions>>();

            // Assert
            Assert.IsType<IOptionsMonitor<BearerTokenOptions>>(bearerTokenOptions);
        }

        [Fact]
        public async Task MapIdentityApi_GetRequiredService_EmailSender_ReturnsEmailSender()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IEmailSender<IdentityUser>, EmailSender<IdentityUser>>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
            endpointRouteBuilder.Setup(e => e.ServiceProvider).Returns(serviceProvider);

            // Act
            var emailSender = endpointRouteBuilder.Object.ServiceProvider.GetRequiredService<IEmailSender<IdentityUser>>();

            // Assert
            Assert.IsType<IEmailSender<IdentityUser>>(emailSender);
        }

        [Fact]
        public async Task MapIdentityApi_GetRequiredService_LinkGenerator_ReturnsLinkGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<LinkGenerator, LinkGenerator>()
                .BuildServiceProvider();

            var endpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
            endpointRouteBuilder.Setup(e => e.ServiceProvider).Returns(serviceProvider);

            // Act
            var linkGenerator = endpointRouteBuilder.Object.ServiceProvider.GetRequiredService<LinkGenerator>();

            // Assert
            Assert.IsType<LinkGenerator>(linkGenerator);
        }
    }
}
