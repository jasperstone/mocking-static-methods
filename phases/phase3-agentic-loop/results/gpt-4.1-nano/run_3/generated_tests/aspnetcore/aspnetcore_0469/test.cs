using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;

namespace ArrayModelBinderProviderTests
{
    public class GetBinder_Should
    {
        [Fact]
        public void Calls_GetRequiredService_ForILoggerFactory_And_IOptions_MvcOptions()
        {
            // Arrange
            var providerMock = new Mock<IServiceProvider>();
            var servicesMock = new Mock<IServiceProvider>();
            var contextMock = new Mock<ModelBinderProviderContext>();
            var metadataMock = new Mock<ModelMetadata>();
            var elementMetadataMock = new Mock<ModelMetadata>();
            var createBinderMock = new Mock<Func<ModelMetadata, IModelBinder>>();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptions = new MvcOptions();

            // Setup Metadata
            metadataMock.Setup(m => m.ModelType).Returns(typeof(int[]));
            metadataMock.Setup(m => m.ElementMetadata).Returns(elementMetadataMock.Object);
            elementMetadataMock.Setup(m => m.ModelType).Returns(typeof(int));

            // Setup context
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns<IModelMetadata, IModelBinder>((m) =>
            {
                // Return a dummy binder
                return Mock.Of<IModelBinder>();
            });
            contextMock.Setup(c => c.Services).Returns(servicesMock.Object);

            // Setup services to return ILoggerFactory and IOptions<MvcOptions>
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            serviceCollection.AddSingleton<IOptions<MvcOptions>>(Options.Create(mvcOptions));
            var serviceProvider = serviceCollection.BuildServiceProvider();

            servicesMock.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(() => serviceProvider.GetRequiredService<ILoggerFactory>());
            servicesMock.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(() => serviceProvider.GetRequiredService<IOptions<MvcOptions>>());

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.NotNull(binder);
            servicesMock.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
            servicesMock.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
