using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;

namespace DefaultDisplayTemplatesTests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void CollectionTemplate_GetRequiredService_CallsGetMetadataForType()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IModelMetadataProvider, MockModelMetadataProvider>()
                .AddScoped<ICompositeViewEngine, MockCompositeViewEngine>()
                .AddScoped<IViewBufferScope, MockViewBufferScope>()
                .BuildServiceProvider();

            var htmlHelper = new HtmlHelper(
                new ViewContext(),
                new MockIViewDataContainer().Object,
                serviceProvider);

            var model = new List<object> { "item1", "item2" };

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.NotNull(result);
        }

        private class MockModelMetadataProvider : IModelMetadataProvider
        {
            public ModelMetadata GetMetadataForType(Type modelType)
            {
                return new ModelMetadata(new EmptyModelMetadataProvider(), modelType);
            }
        }

        private class MockCompositeViewEngine : ICompositeViewEngine
        {
            public ViewEngineResult FindPartialView(ViewContext context, string partialViewName)
            {
                throw new NotImplementedException();
            }

            public ViewEngineResult FindView(ViewContext context, string viewName, bool isMainPage)
            {
                throw new NotImplementedException();
            }
        }

        private class MockViewBufferScope : IViewBufferScope
        {
            public TextWriter Writer { get; set; }
        }

        private class MockIViewDataContainer : Mock<IViewDataContainer>
        {
        }
    }
}
