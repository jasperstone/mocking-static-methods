using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace DefaultDisplayTemplatesTests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void CollectionTemplate_GetRequiredService_ICompositeViewEngine()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<ICompositeViewEngine, CompositeViewEngine>()
                .AddScoped<IViewBufferScope, ViewBufferScope>()
                .AddScoped<IModelMetadataProvider, ModelMetadataProvider>()
                .BuildServiceProvider();

            var htmlHelper = new HtmlHelper(
                new ViewContext(),
                new Mock<IViewDataDictionary>().Object,
                new Mock<IModelMetadataProvider>().Object);

            htmlHelper.ViewContext.HttpContext = new DefaultHttpContext();
            htmlHelper.ViewContext.HttpContext.RequestServices = serviceProvider;

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void CollectionTemplate_GetRequiredService_IViewBufferScope()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<ICompositeViewEngine, CompositeViewEngine>()
                .AddScoped<IViewBufferScope, ViewBufferScope>()
                .AddScoped<IModelMetadataProvider, ModelMetadataProvider>()
                .BuildServiceProvider();

            var htmlHelper = new HtmlHelper(
                new ViewContext(),
                new Mock<IViewDataDictionary>().Object,
                new Mock<IModelMetadataProvider>().Object);

            htmlHelper.ViewContext.HttpContext = new DefaultHttpContext();
            htmlHelper.ViewContext.HttpContext.RequestServices = serviceProvider;

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void CollectionTemplate_GetRequiredService_IModelMetadataProvider()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<ICompositeViewEngine, CompositeViewEngine>()
                .AddScoped<IViewBufferScope, ViewBufferScope>()
                .AddScoped<IModelMetadataProvider, ModelMetadataProvider>()
                .BuildServiceProvider();

            var htmlHelper = new HtmlHelper(
                new ViewContext(),
                new Mock<IViewDataDictionary>().Object,
                new Mock<IModelMetadataProvider>().Object);

            htmlHelper.ViewContext.HttpContext = new DefaultHttpContext();
            htmlHelper.ViewContext.HttpContext.RequestServices = serviceProvider;

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.NotNull(result);
        }
    }
}
