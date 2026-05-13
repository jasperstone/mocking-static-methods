using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void BuildDisplayTemplate_UsesGetRequiredServiceForViewEngine()
        {
            // Arrange
            var model = new List<string> { "Item1", "Item2" };
            var metadataProviderMock = new Mock<IModelMetadataProvider>();
            var viewEngineMock = new Mock<ICompositeViewEngine>();
            var viewBufferScopeMock = new Mock<IViewBufferScope>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IModelMetadataProvider>())
                .Returns(metadataProviderMock.Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<ICompositeViewEngine>())
                .Returns(viewEngineMock.Object);
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IViewBufferScope>())
                .Returns(viewBufferScopeMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var viewContextMock = new Mock<ViewContext>();
            viewContextMock.SetupGet(v => v.HttpContext).Returns(httpContextMock.Object);

            var htmlHelperMock = new Mock<IHtmlHelper>();
            htmlHelperMock.SetupGet(h => h.ViewContext).Returns(viewContextMock.Object);
            htmlHelperMock.SetupGet(h => h.ViewData).Returns(new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()));

            // Act
            var result = DefaultDisplayTemplates.BuildDisplayTemplate(htmlHelperMock.Object, model);

            // Assert
            viewEngineMock.Verify(v => v.GetView(null, null, false), Times.Once);
            viewBufferScopeMock.Verify(v => v.EnterScope(), Times.Once);
        }
    }
}
