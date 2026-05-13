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
        private class DummyModelMetadata : ModelMetadata
        {
            public override string NullDisplayText { get; set; } = "null";
            public override bool HtmlEncode { get; set; } = true;
            public override IList<ModelMetadata> Properties { get; } = new List<ModelMetadata>();
            public override bool IsNullableValueType { get; set; } = false;
            public override Type ModelType { get; set; }
        }

        private class DummyModelExplorer : ModelExplorer
        {
            public string SimpleDisplayText { get; set; } = "display";
            public override string GetSimpleDisplayText() => SimpleDisplayText;
        }

        private class DummyViewData : ViewDataDictionary
        {
            public DummyViewData() : base(new EmptyModelMetadataProvider()) { }
            public override ModelMetadata Metadata { get; set; }
            public override object Model { get; set; }
            public override ModelExplorer ModelExplorer { get; set; }
            public override TemplateInfo TemplateInfo { get; set; } = new TemplateInfo();
        }

        private class DummyViewContext : ViewContext
        {
            public DummyViewContext()
            {
                HttpContext = new DefaultHttpContext();
            }
        }

        [Fact]
        public void HtmlTemplate_ReturnsFormattedModelValue()
        {
            // Arrange
            var viewData = new DummyViewData
            {
                FormattedModelValue = "formatted"
            };
            var helper = new Mock<IHtmlHelper>();
            helper.Setup(h => h.ViewData).Returns(viewData);
            viewData.TemplateInfo.FormattedModelValue = "formatted";

            // Act
            var result = DefaultDisplayTemplates.HtmlTemplate(helper.Object);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal("formatted", ((HtmlString)result).ToString());
        }

        [Fact]
        public void ObjectTemplate_ModelNull_ReturnsNullDisplayText()
        {
            // Arrange
            var modelExplorer = new DummyModelExplorer
            {
                Model = null,
                Metadata = new DummyModelMetadata { NullDisplayText = "null text" }
            };
            var viewData = new DummyViewData
            {
                ModelExplorer = modelExplorer,
                TemplateInfo = new TemplateInfo { TemplateDepth = 0 }
            };
            var viewContext = new DummyViewContext();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICompositeViewEngine, DummyViewEngine>()
                .AddSingleton<IViewBufferScope, DummyViewBufferScope>()
                .BuildServiceProvider();

            var helper = new Mock<IHtmlHelper>();
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewContext).Returns(viewContext);
            helper.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(serviceProvider);
            helper.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(modelExplorer);
            helper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);
            helper.Setup(h => h.ViewData.Model).Returns(modelExplorer.Model);
            helper.Setup(h => h.ViewData.ModelMetadata).Returns(modelExplorer.Metadata);
            helper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(modelExplorer);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewContext).Returns(viewContext);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewData.Model).Returns((object)null);
            helper.Setup(h => h.ViewData.ModelMetadata).Returns(modelExplorer.Metadata);
            helper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(modelExplorer);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(serviceProvider);
            helper.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(modelExplorer);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewData.Model).Returns((object)null);
            helper.Setup(h => h.ViewData.ModelMetadata).Returns(modelExplorer.Metadata);
            helper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(modelExplorer);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewContext).Returns(viewContext);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewData.Model).Returns((object)null);
            helper.Setup(h => h.ViewData.ModelMetadata).Returns(modelExplorer.Metadata);
            helper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(modelExplorer);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewContext).Returns(viewContext);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewData.Model).Returns((object)null);
            helper.Setup(h => h.ViewData.ModelMetadata).Returns(modelExplorer.Metadata);
            helper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(modelExplorer);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewContext).Returns(viewContext);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewData.Model).Returns((object)null);
            helper.Setup(h => h.ViewData.ModelMetadata).Returns(modelExplorer.Metadata);
            helper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(modelExplorer);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewContext).Returns(viewContext);
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewData.Model).Returns((object)null);
            helper.Setup(h => h.ViewData.ModelMetadata).Returns(modelExplorer.Metadata);
            helper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(modelExplorer);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(helper.Object);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal("null text", ((HtmlString)result).ToString());
        }

        [Fact]
        public void ObjectTemplate_TemplateDepthGreaterThanOne_ReturnsSimpleDisplayText()
        {
            // Arrange
            var modelExplorer = new DummyModelExplorer
            {
                Model = "test",
                SimpleDisplayText = "simple",
                Metadata = new DummyModelMetadata { HtmlEncode = false }
            };
            var viewData = new DummyViewData
            {
                ModelExplorer = modelExplorer,
                TemplateInfo = new TemplateInfo { TemplateDepth = 2 }
            };
            var viewContext = new DummyViewContext();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICompositeViewEngine, DummyViewEngine>()
                .AddSingleton<IViewBufferScope, DummyViewBufferScope>()
                .BuildServiceProvider();

            var helper = new Mock<IHtmlHelper>();
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewContext).Returns(viewContext);
            helper.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(serviceProvider);
            helper.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(modelExplorer);
            helper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(helper.Object);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal("simple", ((HtmlString)result).ToString());
        }

        [Fact]
        public void ObjectTemplate_TemplateDepthOne_ReturnsHtmlContentBuilder()
        {
            // Arrange
            var modelExplorer = new DummyModelExplorer
            {
                Model = "test",
                Metadata = new DummyModelMetadata { HtmlEncode = false, Properties = new List<ModelMetadata>() }
            };
            var viewData = new DummyViewData
            {
                ModelExplorer = modelExplorer,
                TemplateInfo = new TemplateInfo { TemplateDepth = 1 }
            };
            var viewContext = new DummyViewContext();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICompositeViewEngine, DummyViewEngine>()
                .AddSingleton<IViewBufferScope, DummyViewBufferScope>()
                .BuildServiceProvider();

            var helper = new Mock<IHtmlHelper>();
            helper.Setup(h => h.ViewData).Returns(viewData);
            helper.Setup(h => h.ViewContext).Returns(viewContext);
            helper.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(serviceProvider);
            helper.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(modelExplorer);
            helper.Setup(h => h.ViewData.TemplateInfo).Returns(viewData.TemplateInfo);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(helper.Object);

            // Assert
            Assert.IsType<HtmlString>(result);
        }

        [Fact]
        public void CollectionTemplate_NullModel_ReturnsEmpty()
        {
            // Arrange
            var viewData = new DummyViewData
            {
                Model = null
            };
            var helper = new Mock<IHtmlHelper>();
            helper.Setup(h => h.ViewData).Returns(viewData);
            var viewContext = new DummyViewContext();
            helper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(helper.Object);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal(string.Empty, ((HtmlString)result).ToString());
        }

        [Fact]
        public void CollectionTemplate_NonEnumerableModel_Throws()
        {
            // Arrange
            var viewData = new DummyViewData
            {
                Model = 123
            };
            var helper = new Mock<IHtmlHelper>();
            helper.Setup(h => h.ViewData).Returns(viewData);
            var viewContext = new DummyViewContext();
            helper.Setup(h => h.ViewContext).Returns(viewContext);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(helper.Object));
        }

        [Fact]
        public void CollectionTemplate_EnumerableModel_ProducesContent()
        {
            // Arrange
            var list = new List<string> { "a", "b" };
            var viewData = new DummyViewData
            {
                Model = list,
                ModelMetadata = new DummyModelMetadata { ElementMetadata = new DummyModelMetadata { IsNullableValueType = false, ModelType = typeof(string) } }
            };
            var helper = new Mock<IHtmlHelper>();
            helper.Setup(h => h.ViewData).Returns(viewData);
            var viewContext = new DummyViewContext();
            helper.Setup(h => h.ViewContext).Returns(viewContext);
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ICompositeViewEngine, DummyViewEngine>()
                .AddSingleton<IViewBufferScope, DummyViewBufferScope>()
                .AddSingleton<IModelMetadataProvider, DummyModelMetadataProvider>()
                .BuildServiceProvider();

            helper.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(serviceProvider);
            helper.Setup(h => h.ViewData.ModelMetadata).Returns(viewData.ModelMetadata);
            helper.Setup(h => h.ViewData.Model).Returns(list);
            helper.Setup(h => h.ViewData.ModelExplorer).Returns(new DummyModelExplorer { Model = list });
            helper.Setup(h => h.ViewData.TemplateInfo).Returns(new TemplateInfo());

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(helper.Object);

            // Assert
            Assert.IsType<HtmlString>(result);
            var htmlContent = (HtmlString)result;
            Assert.Contains("a", htmlContent.ToString());
            Assert.Contains("b", htmlContent.ToString());
        }
    }

    // Dummy implementations for dependencies
    public class DummyViewEngine : ICompositeViewEngine
    {
        public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage) => ViewEngineResult.NotFound(viewName, Array.Empty<string>());
        public ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage) => ViewEngineResult.NotFound(viewPath, Array.Empty<string>());
        public void ReleaseView(ActionContext context, IView view) { }
    }

    public class DummyViewBufferScope : IViewBufferScope
    {
        public BufferItem GetPage(int pageSize) => new BufferItem(new byte[pageSize], 0);
        public BufferItem GetSegment(int segmentSize) => new BufferItem(new byte[segmentSize], 0);
    }

    public class DummyModelMetadataProvider : IModelMetadataProvider
    {
        public ModelMetadata GetMetadataForType(Type modelType) => new DummyModelMetadata { ModelType = modelType };
        public ModelMetadata GetMetadataForProperties(Type modelType) => throw new NotImplementedException();
        public IEnumerable<ModelMetadata> GetMetadataForProperty(Type modelType, string propertyName) => throw new NotImplementedException();
        public ModelMetadata GetMetadataForParameter(System.Reflection.ParameterInfo parameter) => throw new NotImplementedException();
    }
}
