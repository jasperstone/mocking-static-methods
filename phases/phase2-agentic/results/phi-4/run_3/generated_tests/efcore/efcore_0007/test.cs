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
        public void AddDbContextPool_CallsGetService_WhenTContextServiceIsNotTContextImplementation()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopedDbContextLeaseMock = new Mock<IScopedDbContextLease<MockDbContext>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IScopedDbContextLease<MockDbContext>>())
                .Returns(scopedDbContextLeaseMock.Object);

            var optionsAction = (sp, ob) => { };

            // Act
            EntityFrameworkServiceCollectionExtensions
                .AddDbContextPool<MockDbContextService, MockDbContextImplementation>(
                    serviceCollection,
                    optionsAction);

            // Assert
            serviceProviderMock.Verify(
                sp => sp.GetService<MockDbContextService>(),
                Times.Once);
        }
    }

    public interface MockDbContextService { }

    public class MockDbContextImplementation : DbContext, MockDbContextService
    {
    }

    public interface IScopedDbContextLease<TContext> where TContext : DbContext
    {
        TContext Context { get; }
    }

    public class ScopedDbContextLease<TContext> : IScopedDbContextLease<TContext> where TContext : DbContext
    {
        public TContext Context { get; }
    }
}
