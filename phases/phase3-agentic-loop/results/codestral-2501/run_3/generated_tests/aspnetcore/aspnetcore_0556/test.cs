using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void ObjectTemplate_ShouldCallGetRequiredService()
        {
            // Arrange
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            var mockViewData = new Mock<ViewDataDictionary>();
            var mockTemplateInfo = new Mock<TemplateInfo>();
            var mockModelExplorer = new Mock<ModelExplorer>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();

            mockHtmlHelper.Setup(h => h.ViewData).Returns(mockViewData.Object);
            mockViewData.Setup(v => v.TemplateInfo).Returns(mockTemplateInfo.Object);
            mockViewData.Setup(v => v.ModelExplorer).Returns(mockModelExplorer.Object);
            mockHtmlHelper.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(mockServiceProvider.Object);

            mockServiceProvider.Setup(sp => sp.GetRequiredService<ICompositeViewEngine>()).Returns(mockViewEngine.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IViewBufferScope>()).Returns(mockViewBufferScope.Object);

            // Act
            DefaultDisplayTemplates.ObjectTemplate(mockHtmlHelper.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<ICompositeViewEngine>(), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IViewBufferScope>(), Times.Once);
        }
    }
}
