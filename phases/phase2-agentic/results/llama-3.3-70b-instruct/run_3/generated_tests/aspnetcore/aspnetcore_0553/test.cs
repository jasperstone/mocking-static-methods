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
                .BuildServiceProvider();

            var htmlHelper = new HtmlHelper(
                new ViewContext(
                    new ActionContext(
                        new HttpContext(),
                        new RouteData(),
                        new ActionDescriptor()),
                    new Mock<IView>().Object,
                    new ViewDataDictionary(),
                    new StringWriter()),
                serviceProvider);

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
                .AddScoped<IViewBufferScope, ViewBufferScope>()
                .BuildServiceProvider();

            var htmlHelper = new HtmlHelper(
                new ViewContext(
                    new ActionContext(
                        new HttpContext(),
                        new RouteData(),
                        new ActionDescriptor()),
                    new Mock<IView>().Object,
                    new ViewDataDictionary(),
                    new StringWriter()),
                serviceProvider);

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
                .AddScoped<IModelMetadataProvider, ModelMetadataProvider>()
                .BuildServiceProvider();

            var htmlHelper = new HtmlHelper(
                new ViewContext(
                    new ActionContext(
                        new HttpContext(),
                        new RouteData(),
                        new ActionDescriptor()),
                    new Mock<IView>().Object,
                    new ViewDataDictionary(),
                    new StringWriter()),
                serviceProvider);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.NotNull(result);
        }
    }
}
