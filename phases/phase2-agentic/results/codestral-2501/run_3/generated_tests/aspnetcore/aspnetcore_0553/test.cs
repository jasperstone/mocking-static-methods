using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
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
        public void CollectionTemplate_ThrowsInvalidOperationException_WhenModelIsNotEnumerable()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelperWithModel(new object());
            var exception = Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper));

            // Act & Assert
            Assert.Equal(
                $"The model item passed into the dictionary is of type '{typeof(object).FullName}', but this dictionary requires a model item of type '{typeof(IEnumerable).FullName}'.",
                exception.Message);
        }

        [Fact]
        public void CollectionTemplate_CallsGetRequiredService_ForICompositeViewEngine()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var viewEngineMock = new Mock<ICompositeViewEngine>();
            var viewBufferScopeMock = new Mock<IViewBufferScope>();
            var metadataProviderMock = new Mock<IModelMetadataProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ICompositeViewEngine>())
                .Returns(viewEngineMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IViewBufferScope>())
                .Returns(viewBufferScopeMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IModelMetadataProvider>())
                .Returns(metadataProviderMock.Object);

            var htmlHelper = CreateHtmlHelperWithModel(new List<int> { 1, 2, 3 }, serviceProviderMock.Object);

            // Act
            DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ICompositeViewEngine>(), Times.Once);
        }

        private static IHtmlHelper CreateHtmlHelperWithModel(object model, IServiceProvider serviceProvider = null)
        {
            var viewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = serviceProvider ?? new Mock<IServiceProvider>().Object
                }
            };

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            return new HtmlHelper(viewContext, viewData);
        }
    }
}
