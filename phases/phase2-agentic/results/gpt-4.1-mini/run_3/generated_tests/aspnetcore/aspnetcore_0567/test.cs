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
            var mockServiceProvider = new Mock<IServiceProvider>();

            var localizationOptions = new MvcDataAnnotationsLocalizationOptions();
            localizationOptions.DataAnnotationLocalizerProvider = (type, factory) => mockStringLocalizer.Object;

            mockOptions.Setup(o => o.Value).Returns(localizationOptions);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IStringLocalizerFactory))).Returns(mockLocalizerFactory.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService(typeof(IOptions<MvcDataAnnotationsLocalizationOptions>))).Returns(mockOptions.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = mockServiceProvider.Object;

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var modelMetadata = modelMetadataProvider.GetMetadataForProperty(typeof(DummyModel), nameof(DummyModel.Property));

            var clientModelValidationContext = new ClientModelValidationContext(
                actionContext,
                modelMetadata,
                new Dictionary<string, string>());

            var attribute = new TestRemoteAttribute("http://test-url");

            // Act
            attribute.AddValidation(clientModelValidationContext);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService(typeof(IOptions<MvcDataAnnotationsLocalizationOptions>)), Times.Once);
            Assert.True(clientModelValidationContext.Attributes.ContainsKey("data-val-remote-url"));
            Assert.Equal("http://test-url", clientModelValidationContext.Attributes["data-val-remote-url"]);
        }

        private class DummyModel
        {
            public string Property { get; set; } = string.Empty;
        }
    }
}
