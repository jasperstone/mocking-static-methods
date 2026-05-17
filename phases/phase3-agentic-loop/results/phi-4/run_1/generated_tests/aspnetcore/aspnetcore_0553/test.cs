using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void BuildDisplayTemplate_ShouldRetrieveCorrectServices()
        {
            // Arrange
            var htmlHelperMock = new Mock<IHtmlHelper>();
            var viewContextMock = new Mock<IViewContext>();
            var viewDataMock = new Mock<IViewDataDictionary>();
            var modelMetadataProviderMock = new Mock<IModelMetadataProvider>();
            var viewEngineMock = new Mock<ICompositeViewEngine>();
            var viewBufferScopeMock = new Mock<IViewBufferScope>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IModelMetadataProvider>())
                .Returns(modelMetadataProviderMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ICompositeViewEngine>())
                .Returns(viewEngineMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IViewBufferScope>())
                .Returns(viewBufferScopeMock.Object);

            htmlHelperMock.Setup(h => h.ViewContext).Returns(viewContextMock.Object);
            htmlHelperMock.Setup(h => h.ViewData).Returns(viewDataMock.Object);
            viewContextMock.Setup(v => v.HttpContext.RequestServices).Returns(serviceProviderMock.Object);

            var model = new List<string> { "item1", "item2" };

            // Act
            var result = DefaultDisplayTemplates.BuildDisplayTemplate(htmlHelperMock.Object, model);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IModelMetadataProvider>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ICompositeViewEngine>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IViewBufferScope>(), Times.Once);
        }
    }
}
