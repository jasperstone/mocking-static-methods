using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
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
        public void ObjectTemplate_ReturnsNullDisplayText_WhenModelIsNull()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelper();
            var viewData = htmlHelper.ViewData;
            var modelExplorer = viewData.ModelExplorer;
            modelExplorer.Model = null;

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlString>(result);
            var htmlString = result as HtmlString;
            Assert.Equal(modelExplorer.Metadata.NullDisplayText, htmlString.ToString());
        }

        [Fact]
        public void ObjectTemplate_ReturnsSimpleDisplayText_WhenTemplateDepthGreaterThanOne()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelper();
            var viewData = htmlHelper.ViewData;
            var modelExplorer = viewData.ModelExplorer;
            modelExplorer.Model = "Test";
            var metadata = modelExplorer.Metadata;
            metadata.HtmlEncode = false;
            var templateInfo = viewData.TemplateInfo;
            templateInfo.TemplateDepth = 2;

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlString>(result);
            var htmlString = result as HtmlString;
            Assert.Equal("Test", htmlString.ToString());
        }

        [Fact]
        public void ObjectTemplate_CallsGetRequiredService_ForViewEngineAndBufferScope()
        {
            // Arrange
            var services = new ServiceCollection();
            var viewEngineMock = new Mock<ICompositeViewEngine>();
            var viewBufferScopeMock = new Mock<IViewBufferScope>();
            services.AddSingleton(viewEngineMock.Object);
            services.AddSingleton(viewBufferScopeMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var htmlHelper = CreateHtmlHelper(serviceProvider);
            var viewData = htmlHelper.ViewData;
            var modelExplorer = viewData.ModelExplorer;
            modelExplorer.Model = new { Name = "Test" };
            var metadataProviderMock = new Mock<IModelMetadataProvider>();
            services.AddSingleton(metadataProviderMock.Object);
            var metadata = new Mock<ModelMetadata>();
            metadata.Setup(m => m.Properties).Returns(new List<ModelMetadata>());
            modelExplorer.Metadata = metadata.Object;

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper);

            // Assert
            viewEngineMock.VerifyAll();
            viewBufferScopeMock.VerifyAll();
        }

        private static IHtmlHelper CreateHtmlHelper(IServiceProvider serviceProvider = null)
        {
            var mockViewContext = new Mock<ViewContext>();
            var mockViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            var mockView = new Mock<IView>();
            mockViewContext.Setup(v => v.ViewData).Returns(mockViewData);
            mockViewContext.Setup(v => v.HttpContext.RequestServices).Returns(serviceProvider ?? new ServiceCollection().BuildServiceProvider());

            var mockHelper = new Mock<IHtmlHelper>();
            mockHelper.Setup(h => h.ViewData).Returns(mockViewData);
            mockHelper.Setup(h => h.ViewContext).Returns(mockViewContext.Object);
            mockHelper.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);
            return mockHelper.Object;
        }
    }
}
