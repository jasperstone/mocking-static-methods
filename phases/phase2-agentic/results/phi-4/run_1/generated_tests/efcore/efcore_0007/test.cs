using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace EFCore.Tests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDbContextPool_RegistersServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var optionsAction = Mock.Of<IServiceProvider, DbContextOptionsBuilder>((sp, ob) => { });
            var poolSize = 10;

            // Act
            serviceCollection.AddDbContextPool<ITestContext, TestContext>(optionsAction, poolSize);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var contextService = serviceProvider.GetService<ITestContext>();
            Assert.NotNull(contextService);
            Assert.IsType<ScopedDbContextLease<TestContext>>(serviceProvider.GetService<IScopedDbContextLease<TestContext>>());
        }

        [Fact]
        public void AddDbContextPool_GetServiceReturnsCorrectInstance()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var optionsAction = Mock.Of<IServiceProvider, DbContextOptionsBuilder>((sp, ob) => { });
            var poolSize = 10;

            serviceCollection.AddDbContextPool<ITestContext, TestContext>(optionsAction, poolSize);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var scopedLease = serviceProvider.GetService<IScopedDbContextLease<TestContext>>();

            // Act
            var context = serviceProvider.GetRequiredService<ITestContext>();

            // Assert
            Assert.Same(scopedLease.Context, context);
        }
    }

    public interface ITestContext : DbContext
    {
    }

    public class TestContext : DbContext, ITestContext
    {
    }
}
