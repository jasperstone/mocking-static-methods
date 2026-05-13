using Xunit;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void ObjectTemplate_ShouldCallGetRequiredServiceForIViewBufferScope()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();

            mockServiceProvider
                .Setup(x => x.GetRequiredService<ICompositeViewEngine>())
                .Returns(mockViewEngine.Object);

            mockServiceProvider
                .Setup(x => x.GetRequiredService<IViewBufferScope>())
                .Returns(mockViewBufferScope.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.RequestServices).Returns(mockServiceProvider.Object);

            var mockViewContext = new Mock<ViewContext>();
            mockViewContext.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

            var mockHtmlHelper = new Mock<IHtmlHelper>();
            mockHtmlHelper.Setup(x => x.ViewContext).Returns(mockViewContext.Object);

            var mockViewData = new Mock<ViewDataDictionary>();
            var mockTemplateInfo = new Mock<TemplateInfo>();
            var mockModelExplorer = new Mock<ModelExplorer>();

            mockViewData.Setup(x => x.TemplateInfo).Returns(mockTemplateInfo.Object);
            mockViewData.Setup(x => x.ModelExplorer).Returns(mockModelExplorer.Object);

            mockHtmlHelper.Setup(x => x.ViewData).Returns(mockViewData.Object);

            // Act
            DefaultDisplayTemplates.ObjectTemplate(mockHtmlHelper.Object);

            // Assert
            mockServiceProvider.Verify(x => x.GetRequiredService<IViewBufferScope>(), Times.Once);
        }
    }
}
