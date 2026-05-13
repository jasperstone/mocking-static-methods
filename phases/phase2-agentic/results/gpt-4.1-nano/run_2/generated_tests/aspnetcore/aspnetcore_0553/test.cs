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

namespace Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        private class DummyModelMetadata : ModelMetadata
        {
            public override string NullDisplayText => "null";
            public override bool HtmlEncode => true;
            public override IList<ModelMetadata> Properties => new List<ModelMetadata>();
        }

        private class DummyModelExplorer : ModelExplorer
        {
            public string SimpleDisplayText { get; set; }
            public override string GetSimpleDisplayText() => SimpleDisplayText;
        }

        private class DummyViewData
        {
            public object Model { get; set; }
            public ModelMetadata Metadata { get; set; }
            public ModelExplorer ModelExplorer { get; set; }
            public TemplateInfo TemplateInfo { get; set; } = new TemplateInfo();
        }

        private class DummyViewContext
        {
            public HttpContext HttpContext { get; set; }
        }

        private class DummyHttpContext
        {
            public IServiceProvider RequestServices { get; set; }
        }

        private class DummyViewDataDictionary : ViewDataDictionary
        {
            public DummyViewDataDictionary() : base(new EmptyModelMetadataProvider()) { }
        }

        private class DummyHtmlHelper : IHtmlHelper
        {
            public ViewDataDictionary ViewData { get; set; }
            public ViewContext ViewContext { get; set; }
            public ModelExplorer ModelExplorer { get; set; }
            public TemplateInfo TemplateInfo { get; set; } = new TemplateInfo();
            public object FormattedModelValue { get; set; }
            public string Encode(string value) => value.ToUpperInvariant();

            public IHtmlContent Encode(object value) => new HtmlString(value.ToString().ToUpperInvariant());

            public string ViewDataString => throw new NotImplementedException();

            public IHtmlContent HyperlinkTemplate(string uriString, string linkedText) => new HtmlString($"<a href='{uriString}'>{linkedText}</a>");

            public IHtmlContent StringTemplate(IHtmlHelper htmlHelper) => new HtmlString(htmlHelper.ViewData.TemplateInfo.FormattedModelValue.ToString());

            public IHtmlContent HtmlString(string value) => new HtmlString(value);
        }

        [Fact]
        public void CollectionTemplate_NullModel_ReturnsEmpty()
        {
            var helper = new DummyHtmlHelper
            {
                ViewData = new DummyViewData { Model = null }
            };
            var result = DefaultDisplayTemplates.CollectionTemplate(helper);
            Assert.IsType<HtmlString>(result);
            Assert.Equal(string.Empty, ((HtmlString)result).Value);
        }

        [Fact]
        public void CollectionTemplate_NonEnumerable_Throws()
        {
            var helper = new DummyHtmlHelper
            {
                ViewData = new DummyViewData { Model = 123 }
            };
            var ex = Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(helper));
            Assert.Contains("Type must implement IEnumerable", ex.Message);
        }

        [Fact]
        public void CollectionTemplate_Enumerable_CallsGetRequiredService()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMetadataProvider = new Mock<IModelMetadataProvider>();
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<IModelMetadataProvider>()).Returns(mockMetadataProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<ICompositeViewEngine>()).Returns(mockViewEngine.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IViewBufferScope>()).Returns(mockViewBufferScope.Object);

            var enumerable = new List<string> { "item1", "item2" };
            var helper = new DummyHtmlHelper
            {
                ViewData = new DummyViewData { Model = enumerable, ModelExplorer = new DummyModelExplorer { SimpleDisplayText = "test" } },
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext { RequestServices = mockServiceProvider.Object }
                }
            };
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "prefix";

            var result = DefaultDisplayTemplates.CollectionTemplate(helper);
            Assert.IsType<HtmlContentBuilder>(result);
        }

        [Fact]
        public void ObjectTemplate_ModelNull_ReturnsNullDisplayText()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<ICompositeViewEngine>()).Returns(mockViewEngine.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IViewBufferScope>()).Returns(mockViewBufferScope.Object);

            var modelExplorer = new DummyModelExplorer
            {
                Model = null,
                Metadata = new DummyModelMetadata()
            };

            var helper = new DummyHtmlHelper
            {
                ViewData = new DummyViewData
                {
                    ModelExplorer = modelExplorer,
                    Model = modelExplorer.Model
                },
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext { RequestServices = mockServiceProvider.Object }
                }
            };

            var result = DefaultDisplayTemplates.ObjectTemplate(helper);
            Assert.IsType<HtmlString>(result);
            Assert.Equal("null", ((HtmlString)result).Value);
        }

        [Fact]
        public void ObjectTemplate_TemplateDepthGreaterThanOne_ReturnsSimpleDisplayText()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<ICompositeViewEngine>()).Returns(mockViewEngine.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IViewBufferScope>()).Returns(mockViewBufferScope.Object);

            var modelExplorer = new DummyModelExplorer
            {
                Model = "test",
                SimpleDisplayText = "display"
            };

            var helper = new DummyHtmlHelper
            {
                ViewData = new DummyViewData
                {
                    ModelExplorer = modelExplorer,
                    Model = modelExplorer.Model
                },
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext { RequestServices = mockServiceProvider.Object }
                }
            };
            helper.ViewData.TemplateInfo.TemplateDepth = 2;

            var result = DefaultDisplayTemplates.ObjectTemplate(helper);
            Assert.IsType<HtmlString>(result);
            Assert.Equal("display", ((HtmlString)result).Value);
        }

        [Fact]
        public void GetRequiredService_CalledOnLine108()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMetadataProvider = new Mock<IModelMetadataProvider>();
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<IModelMetadataProvider>()).Returns(mockMetadataProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<ICompositeViewEngine>()).Returns(mockViewEngine.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IViewBufferScope>()).Returns(mockViewBufferScope.Object);

            var model = new List<int> { 1, 2, 3 };
            var modelMetadata = new DummyModelMetadata();
            var modelExplorer = new DummyModelExplorer
            {
                Model = model,
                Metadata = modelMetadata,
                SimpleDisplayText = "list"
            };

            var helper = new DummyHtmlHelper
            {
                ViewData = new DummyViewData
                {
                    Model = model,
                    ModelExplorer = modelExplorer
                },
                ViewContext = new ViewContext
                {
                    HttpContext = new DefaultHttpContext { RequestServices = mockServiceProvider.Object }
                }
            };

            var result = DefaultDisplayTemplates.ObjectTemplate(helper);
            Assert.IsType<HtmlContentBuilder>(result);
        }
    }
}
