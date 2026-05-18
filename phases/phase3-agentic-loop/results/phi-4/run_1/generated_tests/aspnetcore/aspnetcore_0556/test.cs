using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
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
        public void ObjectTemplate_CallsGetRequiredServiceForIViewBufferScope()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var viewBufferScopeMock = new Mock<IViewBufferScope>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IViewBufferScope>()).Returns(viewBufferScopeMock.Object);

            var viewContextMock = new Mock<ViewContext>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(ctx => ctx.RequestServices).Returns(serviceProviderMock.Object);
            viewContextMock.SetupGet(vc => vc.HttpContext).Returns(httpContextMock.Object);

            var viewDataMock = new Mock<ViewDataDictionary>();
            var htmlHelperMock = new Mock<IHtmlHelper>();
            htmlHelperMock.SetupGet(h => h.ViewContext).Returns(viewContextMock.Object);
            htmlHelperMock.SetupGet(h => h.ViewData).Returns(viewDataMock.Object);

            // Act
            DefaultDisplayTemplates.ObjectTemplate(htmlHelperMock.Object);

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<IViewBufferScope>(), Times.Once);
        }
    }
}
