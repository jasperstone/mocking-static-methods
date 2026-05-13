using Xunit;
using Microsoft.Extensions.DependencyInjection;
using EFCore.Extensions;
using Moq;

namespace EFCore.Tests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDbContextPool_ServiceProvider_GetService_Called()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopedDbContextLeaseMock = new Mock<IScopedDbContextLease<MyContextImplementation>>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IScopedDbContextLease<MyContextImplementation>)))
                .Returns(scopedDbContextLeaseMock.Object);

            var contextImplementation = new MyContextImplementation();
            scopedDbContextLeaseMock
                .Setup(s => s.Context)
                .Returns(contextImplementation);

            serviceCollection.TryAddScoped<IScopedDbContextLease<MyContextImplementation>>(sp => scopedDbContextLeaseMock.Object);
            serviceCollection.TryAddScoped<MyContextService>(sp => (MyContextImplementation)sp.GetService<MyContextService>()!);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var contextService = serviceProvider.GetService<MyContextService>();

            // Assert
            Assert.NotNull(contextService);
            Assert.IsType<MyContextImplementation>(contextService);
        }

        private class MyContextService : DbContext
        {
        }

        private class MyContextImplementation : MyContextService
        {
        }
    }
}
