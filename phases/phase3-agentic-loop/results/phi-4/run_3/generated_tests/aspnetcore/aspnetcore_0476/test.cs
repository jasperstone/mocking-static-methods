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
    public void GetBinder_WhenModelTypeIsDictionary_ShouldReturnDictionaryModelBinder()
    {
        // Arrange
        var context = new Mock<ModelBinderProviderContext>();
        var modelType = typeof(IDictionary<string, int>);
        context.Setup(c => c.Metadata.ModelType).Returns(modelType);

        var keyBinder = new Mock<IModelBinder>();
        var valueBinder = new Mock<IModelBinder>();

        context.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns<IModelBinder>(binder =>
        {
            if (binder is Mock<IModelBinder> mock && mock.Object == keyBinder.Object)
            {
                return keyBinder.Object;
            }
            return valueBinder.Object;
        });

        var loggerFactory = new Mock<ILoggerFactory>();
        var mvcOptions = new MvcOptions();

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(ILoggerFactory, loggerFactory.Object)
            .AddSingleton<IOptions<MvcOptions>>(new OptionsWrapper<MvcOptions>(mvcOptions))
            .BuildServiceProvider();

        context.Setup(c => c.Services).Returns(services);

        var provider = new DictionaryModelBinderProvider();

        // Act
        var binder = provider.GetBinder(context.Object);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<DictionaryModelBinder<string, int>>(binder);

        loggerFactory.Verify(lf => lf.CreateLogger(It.IsAny<string>()), Times.Once);
        services.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
        services.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
    }
}
