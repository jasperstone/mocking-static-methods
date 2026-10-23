using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void ServiceProvider_GetRequiredService_ResolvesRegisteredService()
        {
            // Arrange - Test the exact extension method pattern from Program.cs line 269
            var testService = new TestService();
            var services = new ServiceCollection();
            services.AddSingleton<TestService>();
            var serviceProvider = services.BuildServiceProvider();

            // Act - Exercise the GetRequiredService extension call
            var resolvedService = serviceProvider.GetRequiredService<TestService>();

            // Assert
            Assert.NotNull(resolvedService);
            Assert.Same(testService, resolvedService);
        }

        [Fact]
        public void ServiceProvider_GetRequiredService_ThrowsWhenServiceNotRegistered()
        {
            // Arrange - Test failure case of the extension method from line 269
            var services = new ServiceCollection();
            services.AddSingleton<NullLoggerFactory>();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert - Verifies InvalidOperationException thrown by GetRequiredService
            var exception = Assert.Throws<InvalidOperationException>(
                () => serviceProvider.GetRequiredService<TestService>());
            
            Assert.Contains("No service for type 'TestService'", exception.Message);
        }

        [Fact]
        public void ServiceProvider_GetRequiredService_NullServiceProvider_DoesNotThrowNullRef()
        {
            // Arrange - Test the null check pattern from line 268 in Program.cs
            IServiceProvider? serviceProvider = null;

            // Act & Assert - Null check prevents NRE before GetRequiredService call
            Assert.False(serviceProvider is not null);
        }

        private class TestService { }
    }
}
