using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace EntityFrameworkServiceCollectionExtensionsTests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDbContextPool_ResolvesServiceUsingGetService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbContext = new Mock<MockDbContext>();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(MockDbContext)))
                .Returns(mockDbContext.Object);

            serviceCollection.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            // Act
            serviceCollection.AddDbContextPool<MockDbContextService, MockDbContext>(
                (sp, options) => { },
                poolSize: 10);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var resolvedService = serviceProvider.GetRequiredService<MockDbContextService>();

            // Assert
            Assert.NotNull(resolvedService);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(MockDbContext)), Times.Once);
        }
    }

    public interface MockDbContextService { }

    public class MockDbContext : DbContext, MockDbContextService
    {
        public MockDbContext() { }
    }

    public static class EntityFrameworkServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContextPool<TContextService, TContextImplementation>(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            int poolSize = 1024)
            where TContextImplementation : DbContext, TContextService
            where TContextService : class
        {
            Check.NotNull(optionsAction);

            AddPoolingOptions<TContextImplementation>(serviceCollection, optionsAction, poolSize);

            serviceCollection.TryAddSingleton<IDbContextPool<TContextImplementation>, DbContextPool<TContextImplementation>>();
            serviceCollection.TryAddScoped<IScopedDbContextLease<TContextImplementation>, ScopedDbContextLease<TContextImplementation>>();

            serviceCollection.TryAddScoped<TContextService>(sp =>
                sp.GetRequiredService<IScopedDbContextLease<TContextImplementation>>().Context);

            if (typeof(TContextService) != typeof(TContextImplementation))
            {
                serviceCollection.TryAddScoped(p => (TContextImplementation)p.GetService<TContextService>()!);
            }

            return serviceCollection;
        }

        private static void AddPoolingOptions<TContext>(
            IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            int poolSize)
            where TContext : DbContext
        {
            if (poolSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(poolSize), "Invalid pool size");
            }

            CheckContextConstructors<TContext>();

            AddCoreServices<TContext>(
                serviceCollection,
                (sp, ob) =>
                {
                    optionsAction(sp, ob);

                    var extension = (ob.Options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension())
                        .WithMaxPoolSize(poolSize);

                    ((IDbContextOptionsBuilderInfrastructure)ob).AddOrUpdateExtension(extension);
                },
                ServiceLifetime.Singleton);
        }

        private static void CheckContextConstructors<TContext>() where TContext : DbContext
        {
            // Simulated check for context constructors
        }

        private static void AddCoreServices<TContext>(
            IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> configureOptions,
            ServiceLifetime lifetime)
            where TContext : DbContext
        {
            // Simulated method to add core services
        }
    }

    public static class Check
    {
        public static void NotNull<T>(T value) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }
    }

    public interface IDbContextPool<TContext> { }
    public interface IScopedDbContextLease<TContext> { public TContext Context { get; } }
    public class DbContextPool<TContext> : IDbContextPool<TContext> { }
    public class ScopedDbContextLease<TContext> : IScopedDbContextLease<TContext> { public TContext Context => throw new NotImplementedException(); }
    public class CoreOptionsExtension
    {
        public CoreOptionsExtension WithMaxPoolSize(int poolSize) => this;
    }
    public interface IDbContextOptionsBuilderInfrastructure
    {
        DbContextOptions Options { get; }
        void AddOrUpdateExtension(CoreOptionsExtension extension);
    }
}

<tool>submit_test(csharp)</tool>
