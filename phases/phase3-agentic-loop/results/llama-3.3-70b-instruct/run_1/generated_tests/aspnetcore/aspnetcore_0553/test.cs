using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DefaultDisplayTemplatesTests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void CollectionTemplate_ModelIsNull_ReturnsEmptyHtmlString()
        {
            // Arrange
            var htmlHelper = new HtmlHelper(
                new ViewContext(),
                new Mock<IViewDataDictionary>().Object,
                new Mock<IModelMetadataProvider>().Object);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal(string.Empty, result.ToString());
        }

        [Fact]
        public void CollectionTemplate_ModelIsNotIEnumerable_ThrowsInvalidOperationException()
        {
            // Arrange
            var htmlHelper = new HtmlHelper(
                new ViewContext(),
                new Mock<IViewDataDictionary>().Object,
                new Mock<IModelMetadataProvider>().Object);
            htmlHelper.ViewData.Model = "test";

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper));
        }

        [Fact]
        public void CollectionTemplate_ModelIsIEnumerable_ReturnsHtmlContentBuilder()
        {
            // Arrange
            var htmlHelper = new HtmlHelper(
                new ViewContext(),
                new Mock<IViewDataDictionary>().Object,
                new Mock<IModelMetadataProvider>().Object);
            htmlHelper.ViewData.Model = new List<string> { "test1", "test2" };

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlContentBuilder>(result);
        }

        [Fact]
        public void CollectionTemplate_GetRequiredService_ICompositeViewEngine()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var viewEngine = new Mock<ICompositeViewEngine>().Object;
            serviceProvider.GetService<ICompositeViewEngine>().Returns(viewEngine);
            var htmlHelper = new HtmlHelper(
                new ViewContext(),
                new Mock<IViewDataDictionary>().Object,
                new Mock<IModelMetadataProvider>().Object);
            htmlHelper.ViewContext.HttpContext.RequestServices = serviceProvider;

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlContentBuilder>(result);
        }

        [Fact]
        public void CollectionTemplate_GetRequiredService_IViewBufferScope()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var viewBufferScope = new Mock<IViewBufferScope>().Object;
            serviceProvider.GetService<IViewBufferScope>().Returns(viewBufferScope);
            var htmlHelper = new HtmlHelper(
                new ViewContext(),
                new Mock<IViewDataDictionary>().Object,
                new Mock<IModelMetadataProvider>().Object);
            htmlHelper.ViewContext.HttpContext.RequestServices = serviceProvider;

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlContentBuilder>(result);
        }
    }
}
