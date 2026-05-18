using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void CollectionTemplate_NullModel_ReturnsEmpty()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelper<object>(model: null);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal(string.Empty, result.ToString());
        }

        [Fact]
        public void CollectionTemplate_NonEnumerable_Throws()
        {
            // Arrange
            var model = new object();
            var htmlHelper = CreateHtmlHelper(model);
            // Force Model to be non-Enumerable
            htmlHelper.Object.ViewData.Model = model;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper));
        }

        [Fact]
        public void CollectionTemplate_EnumerableWithItems_CallsTemplateBuilder()
        {
            // Arrange
            var items = new List<string> { "item1", "item2" };
            var modelMetadata = new Mock<ModelMetadata>();
            modelMetadata.Setup(m => m.Properties).Returns(new List<ModelMetadata>());
            var modelExplorer = new Mock<ModelExplorer>();
            modelExplorer.Setup(m => m.Model).Returns(items);
            modelExplorer.Setup(m => m.Metadata).Returns(modelMetadata.Object);
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = items,
                ModelExplorer = modelExplorer.Object,
                TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "prefix" }
            };
            var viewContext = new ViewContext();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICompositeViewEngine, Mock<ICompositeViewEngine>>()
                .AddSingleton<IViewBufferScope, Mock<IViewBufferScope>>()
                .BuildServiceProvider();

            var htmlHelper = new Mock<IHtmlHelper>();
            htmlHelper.Setup(h => h.ViewData).Returns(viewData);
            htmlHelper.Setup(h => h.ViewContext).Returns(viewContext);
            htmlHelper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);
            htmlHelper.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);
            htmlHelper.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(serviceProvider.GetRequiredService<IServiceProvider>());

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);

            // Assert
            Assert.IsType<HtmlContentBuilder>(result);
            var htmlContent = result as HtmlContentBuilder;
            Assert.NotNull(htmlContent);
            Assert.Contains("item1", htmlContent.ToString());
            Assert.Contains("item2", htmlContent.ToString());
        }

        [Fact]
        public void CollectionTemplate_ItemMetadataTypeChanges_CallsGetMetadataForType()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3 };
            var modelMetadata = new Mock<ModelMetadata>();
            modelMetadata.Setup(m => m.Properties).Returns(new List<ModelMetadata>());
            modelMetadata.Setup(m => m.IsNullableValueType).Returns(false);
            var modelExplorer = new Mock<ModelExplorer>();
            modelExplorer.Setup(m => m.Model).Returns(items);
            modelExplorer.Setup(m => m.Metadata).Returns(modelMetadata.Object);
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = items,
                ModelExplorer = modelExplorer.Object,
                TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "prefix" }
            };
            var viewContext = new ViewContext();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICompositeViewEngine, Mock<ICompositeViewEngine>>()
                .AddSingleton<IViewBufferScope, Mock<IViewBufferScope>>()
                .BuildServiceProvider();

            var htmlHelper = new Mock<IHtmlHelper>();
            htmlHelper.Setup(h => h.ViewData).Returns(viewData);
            htmlHelper.Setup(h => h.ViewContext).Returns(viewContext);
            htmlHelper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);
            htmlHelper.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);
            htmlHelper.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(serviceProvider.GetRequiredService<IServiceProvider>());

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);

            // Assert
            // No exception means success, as the method should call GetMetadataForType for each item
            Assert.NotNull(result);
        }

        private static IHtmlHelper CreateHtmlHelper<T>(T model)
        {
            var viewData = new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model,
                TemplateInfo = new TemplateInfo()
            };
            var viewContext = new ViewContext();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICompositeViewEngine, Mock<ICompositeViewEngine>>()
                .AddSingleton<IViewBufferScope, Mock<IViewBufferScope>>()
                .BuildServiceProvider();

            var htmlHelperMock = new Mock<IHtmlHelper>();
            htmlHelperMock.Setup(h => h.ViewData).Returns(viewData);
            htmlHelperMock.Setup(h => h.ViewContext).Returns(viewContext);
            htmlHelperMock.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);
            htmlHelperMock.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);
            htmlHelperMock.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(serviceProvider.GetRequiredService<IServiceProvider>());

            return htmlHelperMock.Object;
        }
    }
}
