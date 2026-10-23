using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;

namespace EfCoreTests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDbContext_Should_Call_GetService_For_TContextService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Create a mock for IServiceProvider to verify GetService call
            var serviceProviderMock = new Mock<IServiceProvider>();
            var calledTypes = new List<Type>();

            serviceProviderMock
                .Setup(sp => sp.GetService(It.IsAny<Type>()))
                .Callback<Type>(type => calledTypes.Add(type))
                .Returns((Type t) =>
                {
                    // Return a dummy object for TContextService type
                    if (t == typeof(ITestService))
                        return new TestService() as object;
                    // For TContextImplementation, return null or a mock
                    if (t == typeof(TestDbContext))
                        return null;
                    return null;
                });

            // Register the mock IServiceProvider in the service collection
            services.AddSingleton(serviceProviderMock.Object);

            // Register a dummy TContextService and TContextImplementation
            services.TryAddScoped<ITestService, TestService>();
            services.TryAddScoped<TestDbContext>();

            // Act
            // Call the extension method with dummy types
            services.AddDbContext<ITestService, TestDbContext>();

            // Build the provider
            var provider = services.BuildServiceProvider();

            // Act: resolve the scoped service to trigger the registration
            var scope = provider.CreateScope();
            var sp = scope.ServiceProvider;

            // Manually invoke the registration delegate to simulate the registration process
            // Since the code calls p.GetService<T>(), we simulate that here
            var _ = sp.GetService<ITestService>();

            // Assert
            // Verify that GetService was called with TContextService type
            Assert.Contains(typeof(ITestService), calledTypes);
        }

        // Dummy interface and class for testing
        public interface ITestService { }
        public class TestService : ITestService { }

        public class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        }
    }
}
