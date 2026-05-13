using System;
using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        private class DummyModelMetadata : ModelMetadata
        {
            public override string NullDisplayText => "null";
            public override bool HtmlEncode => true;
            public override bool IsNullableValueType => false;
            public override Type ModelType => typeof(string);
            public override PropertyDescriptorCollection Properties => new PropertyDescriptorCollection(Array.Empty<PropertyDescriptor>());
        }

        private class DummyModelExplorer : ModelExplorer
        {
            public DummyModelExplorer() : base(
                new EmptyModelMetadataProvider(),
                null,
                null)
            {
            }

            public override string GetSimpleDisplayText() => "simple text";
        }

        [Fact]
        public void CollectionTemplate_NullModel_ReturnsEmpty()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelperWithModel(null);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal(string.Empty, ((HtmlString)result).Value);
        }

        [Fact]
        public void CollectionTemplate_NonEnumerable_Throws()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelperWithModel(123);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper));
        }

        [Fact]
        public void CollectionTemplate_Enumerable_CallsGetRequiredService()
        {
            // Arrange
            var model = new[] { "a", "b" };
            var services = new ServiceCollection()
                .AddTransient<IModelMetadataProvider, DummyModelMetadataProvider>()
                .AddTransient<ICompositeViewEngine, DummyViewEngine>()
                .AddTransient<IViewBufferScope, DummyViewBufferScope>()
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext
            {
                RequestServices = services
            };

            var viewData = new ViewDataDictionary(new DummyModelMetadataProvider())
            {
                Model = model,
                ModelMetadata = new DummyModelMetadata(),
                TemplateInfo = new TemplateInfo(),
                ModelExplorer = new DummyModelExplorer()
            };

            var viewContext = new ViewContext
            {
                HttpContext = httpContext
            };

            var viewDataDictionary = new ViewDataDictionary(new DummyModelMetadataProvider())
            {
                Model = model,
                ModelMetadata = new DummyModelMetadata(),
                TemplateInfo = new TemplateInfo(),
                ModelExplorer = new DummyModelExplorer()
            };

            var htmlHelper = new HtmlHelper(
                viewContext,
                new DummyViewDataContainer(viewData),
                new HtmlHelperOptions());

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlContentBuilder>(result);
        }

        private static IHtmlHelper CreateHtmlHelperWithModel(object model)
        {
            var viewData = new ViewDataDictionary(new DummyModelMetadataProvider())
            {
                Model = model,
                ModelMetadata = new DummyModelMetadata(),
                TemplateInfo = new TemplateInfo(),
                ModelExplorer = new DummyModelExplorer()
            };

            var viewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext(),
                ViewData = viewData
            };

            return new HtmlHelper(viewContext, new DummyViewDataContainer(viewData), new HtmlHelperOptions());
        }

        private class DummyViewDataContainer : IViewDataContainer
        {
            public DummyViewDataContainer(ViewDataDictionary viewData) => ViewData = viewData;
            public ViewDataDictionary ViewData { get; set; }
        }

        private class DummyModelMetadataProvider : IModelMetadataProvider
        {
            public ModelMetadata GetMetadataForType(Type modelType)
            {
                return new DummyModelMetadata();
            }

            public ModelMetadata GetMetadataForProperty(object container, string propertyName)
            {
                throw new NotImplementedException();
            }
        }

        private class DummyViewEngine : ICompositeViewEngine
        {
            public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage) => ViewEngineResult.NotFound(viewName, Array.Empty<string>());
            public ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage) => ViewEngineResult.NotFound(viewPath, Array.Empty<string>());
        }

        private class DummyViewBufferScope : IViewBufferScope
        {
            public BufferItem GetPage(int pageSize) => new BufferItem();
        }
    }
}
