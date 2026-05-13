using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
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
            var model = new { Name = "Test" };
            var modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, model.GetType());
            var modelExplorer = new ModelExplorer(modelMetadata, model);
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), modelExplorer);
            var viewContext = new ViewContext
            {
                ViewData = viewData,
                HttpContext = new DefaultHttpContext()
            };

            var serviceProviderMock = new Mock<IServiceProvider>();
            var viewBufferScopeMock = new Mock<IViewBufferScope>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IViewBufferScope>()).Returns(viewBufferScopeMock.Object);

            var htmlHelper = new HtmlHelper(viewContext, viewData);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper);

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<IViewBufferScope>(), Times.Once);
            Assert.NotNull(result);
        }
    }
}
