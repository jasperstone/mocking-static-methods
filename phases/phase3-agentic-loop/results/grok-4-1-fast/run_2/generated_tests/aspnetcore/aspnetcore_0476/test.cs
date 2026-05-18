using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests;

public class DictionaryModelBinderProviderTests
{
    [Fact]
    public void GetBinder_CallsGetRequiredService_WhenDictionaryType()
    {
        // Arrange
        var mockServices = new Mock<IServiceProvider>();
        var loggerFactory = NullLoggerFactory.Instance;
        var mvcOptions = new MvcOptions();

        mockServices.Setup(s => s.GetRequiredService<ILoggerFactory>())
                   .Returns(loggerFactory)
                   .Verifiable();
        mockServices.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>())
                   .Returns(Options.Create(mvcOptions))
                   .Verifiable();

        var metadataProvider = new EmptyModelMetadataProvider();
        var context = new ModelBinderProviderContext
        {
            MetadataProvider = metadataProvider,
            Services = mockServices.Object
        };
        context.Metadata = metadataProvider.GetMetadataForType(typeof(Dictionary<string, int>));

        var provider = new DictionaryModelBinderProvider();

        // Act
        var result = provider.GetBinder(context);

        // Assert
        mockServices.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once());
        mockServices.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once());
        Assert.NotNull(result);
    }

    [Fact]
    public void GetBinder_NonDictionaryType_ReturnsNull()
    {
        // Arrange
        var mockServices = new Mock<IServiceProvider>();
        var metadataProvider = new EmptyModelMetadataProvider();
        var context = new ModelBinderProviderContext
        {
            MetadataProvider = metadataProvider,
            Services = mockServices.Object
        };
        context.Metadata = metadataProvider.GetMetadataForType(typeof(string));

        var provider = new DictionaryModelBinderProvider();

        // Act
        var result = provider.GetBinder(context);

        // Assert
        Assert.Null(result);
        mockServices.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Never());
        mockServices.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Never());
    }

    [Fact]
    public void GetBinder_NullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = new DictionaryModelBinderProvider();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
        Assert.Equal("context", exception.ParamName);
    }
}
