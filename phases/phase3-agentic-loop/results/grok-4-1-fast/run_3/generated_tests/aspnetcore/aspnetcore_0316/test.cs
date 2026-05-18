using System;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Routing.Tests
{
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
            services.AddSingleton<IOptionsMonitor<BearerTokenOptions>>(new FakeBearerTokenOptionsMonitor());
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
            Assert.Contains("IOptionsMonitor<BearerTokenOptions>", ex.Message);
        }

        private sealed class FakeBearerTokenOptionsMonitor : IOptionsMonitor<BearerTokenOptions>
        {
            public BearerTokenOptions Get(string? name) => new BearerTokenOptions();
            
            public IDisposable OnChange(Action<BearerTokenOptions, string> listener) 
                => new FakeDisposable();
        }

        private sealed class FakeDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
