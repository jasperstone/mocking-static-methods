using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void ObjectTemplate_GetRequiredService_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var viewEngineMock = new Mock<ICompositeViewEngine>();
            var viewBufferScopeMock = new Mock<IViewBufferScope>();
            serviceProviderMock.Setup(p => p.GetRequiredService<ICompositeViewEngine>()).Returns(viewEngineMock.Object);
            serviceProviderMock.Setup(p => p.GetRequiredService<IViewBufferScope>()).Returns(viewBufferScopeMock.Object);

            var htmlHelperMock = new Mock<IHtmlHelper>();
            htmlHelperMock.Setup(p => p.ViewContext.HttpContext.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            DefaultDisplayTemplates.ObjectTemplate(htmlHelperMock.Object);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<ICompositeViewEngine>(), Times.Once);
            serviceProviderMock.Verify(p => p.GetRequiredService<IViewBufferScope>(), Times.Once);
        }
    }
}
