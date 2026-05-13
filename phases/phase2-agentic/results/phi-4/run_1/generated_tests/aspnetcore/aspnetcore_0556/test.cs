using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void ObjectTemplate_ShouldRetrieveIViewBufferScopeFromServiceProvider()
        {
            // Arrange
            var modelExplorer = new ModelExplorer(
                new Mock<IModelMetadataProvider>().Object,
                container: null,
                metadata: new EmptyModelMetadataProvider().GetMetadataForType(typeof(object)),
                model: new object());

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                ModelExplorer = modelExplorer
            };

            var viewContext = new ViewContext
            {
                ViewData = viewData,
                HttpContext = new DefaultHttpContext()
            };

            var htmlHelper = new HtmlHelper(viewContext, viewData);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var viewBufferScopeMock = new Mock<IViewBufferScope>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ICompositeViewEngine>())
                .Returns(new DefaultViewEngine());

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IViewBufferScope>())
                .Returns(viewBufferScopeMock.Object);

            viewContext.HttpContext.RequestServices = serviceProviderMock.Object;

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IViewBufferScope>(), Times.Once);
            Assert.IsType<HtmlString>(result);
        }
    }
}
