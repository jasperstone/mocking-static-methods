using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DefaultDisplayTemplatesTests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void ObjectTemplate_GetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var viewEngineMock = new Mock<ICompositeViewEngine>();
            var viewBufferScopeMock = new Mock<IViewBufferScope>();
            serviceProviderMock.Setup(s => s.GetRequiredService<ICompositeViewEngine>()).Returns(viewEngineMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IViewBufferScope>()).Returns(viewBufferScopeMock.Object);

            var htmlHelperMock = new Mock<IHtmlHelper>();
            htmlHelperMock.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            DefaultDisplayTemplates.ObjectTemplate(htmlHelperMock.Object);

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<ICompositeViewEngine>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IViewBufferScope>(), Times.Once);
        }

        [Fact]
        public void CollectionTemplate_GetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var viewEngineMock = new Mock<ICompositeViewEngine>();
            var viewBufferScopeMock = new Mock<IViewBufferScope>();
            var metadataProviderMock = new Mock<IModelMetadataProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<ICompositeViewEngine>()).Returns(viewEngineMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IViewBufferScope>()).Returns(viewBufferScopeMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IModelMetadataProvider>()).Returns(metadataProviderMock.Object);

            var htmlHelperMock = new Mock<IHtmlHelper>();
            htmlHelperMock.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            DefaultDisplayTemplates.CollectionTemplate(htmlHelperMock.Object);

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<ICompositeViewEngine>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IViewBufferScope>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IModelMetadataProvider>(), Times.Once);
        }
    }
}
