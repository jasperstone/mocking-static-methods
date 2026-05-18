using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

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
            Assert.Equal(string.Empty, ((HtmlString)result).Value);
        }

        [Fact]
        public void CollectionTemplate_NonEnumerableModel_Throws()
        {
            // Arrange
            var model = new object();
            var htmlHelper = CreateHtmlHelper(model);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper));
        }

        [Fact]
        public void CollectionTemplate_EnumerableModel_CallsGetRequiredService()
        {
            // Arrange
            var items = new List<string> { "a", "b" };
            var modelMetadata = new Mock<ModelMetadata>();
            modelMetadata.Setup(m => m.ElementMetadata).Returns(new ModelMetadata());
            var modelExplorer = new Mock<ModelExplorer>();
            modelExplorer.Setup(m => m.Model).Returns(items);
            modelExplorer.Setup(m => m.Metadata).Returns(new ModelMetadata());
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = items,
                ModelExplorer = modelExplorer.Object,
                TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "prefix" }
            };
            var viewContext = new ViewContext();
            var httpContext = new DefaultHttpContext();
            var services = new ServiceCollection().BuildServiceProvider();
            var requestServices = new Mock<IServiceProvider>();
            requestServices.Setup(r => r.GetService(typeof(ICompositeViewEngine))).Returns(Mock.Of<ICompositeViewEngine>());
            requestServices.Setup(r => r.GetService(typeof(IViewBufferScope))).Returns(Mock.Of<IViewBufferScope>());
            requestServices.Setup(r => r.GetService(typeof(IModelMetadataProvider))).Returns(Mock.Of<IModelMetadataProvider>());

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(requestServices.Object);
            var viewContextMock = new Mock<ViewContext>();
            viewContextMock.Setup(c => c.HttpContext).Returns(httpContextMock.Object);
            var htmlHelper = new Mock<IHtmlHelper>();
            htmlHelper.Setup(h => h.ViewData).Returns(viewData);
            htmlHelper.Setup(h => h.ViewContext).Returns(viewContextMock.Object);
            htmlHelper.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);

            // Assert
            Assert.IsType<HtmlString>(result);
        }

        private static IHtmlHelper CreateHtmlHelper<T>(T model)
        {
            var viewData = new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model,
                TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "prefix" }
            };
            var viewContext = new ViewContext();
            var httpContext = new DefaultHttpContext();
            var services = new ServiceCollection()
                .AddSingleton<IModelMetadataProvider, DefaultModelMetadataProvider>()
                .AddSingleton<ICompositeViewEngine, DefaultViewEngine>()
                .AddSingleton<IViewBufferScope, DefaultViewBufferScope>()
                .BuildServiceProvider();

            var requestServices = new Mock<IServiceProvider>();
            requestServices.Setup(r => r.GetService(typeof(ICompositeViewEngine))).Returns(services.GetService<ICompositeViewEngine>());
            requestServices.Setup(r => r.GetService(typeof(IViewBufferScope))).Returns(services.GetService<IViewBufferScope>());
            requestServices.Setup(r => r.GetService(typeof(IModelMetadataProvider))).Returns(services.GetService<IModelMetadataProvider>());

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(requestServices.Object);
            var viewContextMock = new Mock<ViewContext>();
            viewContextMock.Setup(c => c.HttpContext).Returns(httpContextMock.Object);
            viewContextMock.Setup(c => c.ViewData).Returns(viewData);
            var helper = new Mock<IHtmlHelper>();
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewContext).Returns(viewContextMock.Object);
            helper.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);
            return helper.Object;
        }

        private class DefaultViewEngine : ICompositeViewEngine
        {
            public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage) => throw new NotImplementedException();
            public ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage) => throw new NotImplementedException();
            public void ReleaseView(ActionContext context, IView view) { }
        }

        private class DefaultViewBufferScope : IViewBufferScope
        {
            public BufferItem GetPage(int pageSize) => throw new NotImplementedException();
            public BufferItem GetSegment(int segmentSize) => throw new NotImplementedException();
        }
    }
}
