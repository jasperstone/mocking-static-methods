using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDbContextPool_adds_services_correctly_when_types_are_equal()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddDbContextPool<TestDbContext>(
                (sp, options) => { });

            // Assert
            var testDbContextDescriptors = services.Where(d => d.ServiceType == typeof(TestDbContext)).ToList();
            Assert.Single(testDbContextDescriptors);
            Assert.Equal(ServiceLifetime.Scoped, testDbContextDescriptors[0].Lifetime);
        }

        [Fact]
        public void AddDbContextPool_adds_casting_service_when_TContextService_differs_from_TContextImplementation()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddDbContextPool<ITestDbContext, TestDbContext>(
                (sp, options) => { });

            // Assert - main service (lease)
            var mainDescriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(ITestDbContext)));
            Assert.Equal(ServiceLifetime.Scoped, mainDescriptor.Lifetime);

            // Assert - casting service (line 347: p.GetService<TContextService>()!)
            var castingDescriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(TestDbContext)));
            Assert.Equal(ServiceLifetime.Scoped, castingDescriptor.Lifetime);

            // Verify the casting factory resolves correctly via GetService
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var contextFromInterface = scope.ServiceProvider.GetRequiredService<ITestDbContext>();
            Assert.IsType<TestDbContext>(contextFromInterface);
            
            var contextFromImpl = scope.ServiceProvider.GetService<TestDbContext>();
            Assert.NotNull(contextFromImpl);
            Assert.Same(contextFromInterface, contextFromImpl);
        }

        [Fact]
        public void AddDbContextPool_does_not_add_casting_service_when_types_are_equal()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddDbContextPool<TestDbContext>(
                (sp, options) => { });

            // Assert - only main service when types match (no casting service)
            var testDbContextDescriptors = services.Where(d => d.ServiceType == typeof(TestDbContext)).ToList();
            Assert.Single(testDbContextDescriptors);
        }

        // Test classes to satisfy generic constraints
        public class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        }

        public interface ITestDbContext { }
    }
}
