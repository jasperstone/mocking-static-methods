using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ArrayModelBinderProviderTests
{
    public class GetBinder_CallsGetRequiredService
    {
        [Fact]
        public void CallsGetRequiredService_ForLoggerFactory_And_MvcOptions()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var contextMock = new Mock<ModelBinderProviderContext>();
            var metadataMock = new Mock<ModelMetadata>();
            var elementMetadataMock = new Mock<ModelMetadata>();
            var elementBinderMock = Mock.Of<IModelBinder>();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptions = new MvcOptions();

            // Setup Metadata
            metadataMock.Setup(m => m.ModelType).Returns(typeof(int[]));
            metadataMock.Setup(m => m.ElementMetadata).Returns(elementMetadataMock.Object);
            elementMetadataMock.Setup(m => m.ModelType).Returns(typeof(int));

            // Setup context
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(elementBinderMock);
            contextMock.Setup(c => c.Services).Returns(servicesMock.Object);

            // Setup services to return mocks
            servicesMock.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(new OptionsWrapper<MvcOptions>(mvcOptions));

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(contextMock.Object);

            // Assert
            servicesMock.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
            servicesMock.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
            Assert.NotNull(binder);
        }
    }
}
