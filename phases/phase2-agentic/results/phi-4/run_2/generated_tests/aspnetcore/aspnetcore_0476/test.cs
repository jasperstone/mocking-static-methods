using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class DictionaryModelBinderProviderTests
{
    [Fact]
    public void GetBinder_ShouldReturnDictionaryModelBinder_WhenModelTypeIsIDictionary()
    {
        // Arrange
        var context = new Mock<ModelBinderProviderContext>();
        var modelType = typeof(IDictionary<string, int>);
        var metadata = new Mock<ModelMetadata>();
        metadata.Setup(m => m.ModelType).Returns(modelType);

        context.Setup(c => c.Metadata).Returns(metadata.Object);
        context.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(new Mock<IModelBinder>().Object);

        var serviceProvider = new Mock<IServiceProvider>();
        var loggerFactory = new Mock<ILoggerFactory>();
        var mvcOptions = new Mock<IOptions<MvcOptions>>();
        var mvcOptionsValue = new MvcOptions();

        serviceProvider.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(loggerFactory.Object);
        serviceProvider.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(mvcOptions.Object);
        mvcOptions.Setup(o => o.Value).Returns(mvcOptionsValue);

        context.Setup(c => c.Services).Returns(serviceProvider.Object);

        var provider = new DictionaryModelBinderProvider();

        // Act
        var binder = provider.GetBinder(context.Object);

        // Assert
        Assert.NotNull(binder);
        serviceProvider.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
        serviceProvider.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
    }
}
