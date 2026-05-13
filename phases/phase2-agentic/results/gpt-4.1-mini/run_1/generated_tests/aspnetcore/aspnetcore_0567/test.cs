using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class RemoteAttributeBaseTests
    {
        private class TestRemoteAttribute : RemoteAttributeBase
        {
            private readonly string _url;

            public TestRemoteAttribute(string url)
            {
                _url = url;
            }

            protected override string GetUrl(ClientModelValidationContext context)
            {
                return _url;
            }
        }

        [Fact]
        public void AddValidation_CallsGetRequiredServiceOnRequestServices()
        {
            // Arrange
            var mockOptions = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var mockLocalizerFactory = new Mock<IStringLocalizerFactory>();
            var mockStringLocalizer = new Mock<IStringLocalizer>();
            var mockLocalizerProvider = new Mock<Func<Type, IStringLocalizerFactory, IStringLocalizer>>();
            var mockMvcDataAnnotationsLocalizationOptions = new MvcDataAnnotationsLocalizationOptions
            {
                DataAnnotationLocalizerProvider = (type, factory) => mockStringLocalizer.Object
            };
            mockOptions.Setup(o => o.Value).Returns(mockMvcDataAnnotationsLocalizationOptions);

            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IStringLocalizerFactory))).Returns(mockLocalizerFactory.Object);
            services.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>()).Returns(mockOptions.Object);

            var httpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            httpContext.Setup(c => c.RequestServices).Returns(services.Object);

            var actionContext = new ActionContext
            {
                HttpContext = httpContext.Object,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var modelMetadata = modelMetadataProvider.GetMetadataForProperty(typeof(DummyModel), nameof(DummyModel.Name));

            var attributes = new Dictionary<string, string>();

            var clientModelValidationContext = new ClientModelValidationContext(
                actionContext,
                modelMetadata,
                attributes);

            var attribute = new TestRemoteAttribute("http://test-url");

            // Act
            attribute.AddValidation(clientModelValidationContext);

            // Assert
            services.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
            Assert.True(attributes.ContainsKey("data-val"));
            Assert.True(attributes.ContainsKey("data-val-remote"));
            Assert.True(attributes.ContainsKey("data-val-remote-url"));
            Assert.Equal("http://test-url", attributes["data-val-remote-url"]);
        }

        private class DummyModel
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}
