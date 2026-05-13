using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class RemoteAttributeBaseTests
    {
        private class DummyLocalizer : IStringLocalizer
        {
            public LocalizedString this[string name, params object[] arguments] => new LocalizedString(name, $"Localized: {name}");
            public LocalizedString this[string name] => new LocalizedString(name, $"Localized: {name}");
            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();
            public IStringLocalizer WithCulture(System.Globalization.CultureInfo culture) => this;
        }

        private class DummyProvider
        {
            public static IStringLocalizerProvider Provider => (type, factory) => new DummyLocalizer();
        }

        [Fact]
        public void AddValidation_Should_Call_GetRequiredService_For_Localizer()
        {
            // Arrange
            var attributes = new Dictionary<string, string>();
            var modelMetadata = new Mock<ModelMetadata>();
            modelMetadata.Setup(m => m.PropertyName).Returns("Prop");
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var requestServices = new ServiceCollection()
                .AddTransient<IOptions<MvcDataAnnotationsLocalizationOptions>>(sp =>
                {
                    var options = new MvcDataAnnotationsLocalizationOptions
                    {
                        DataAnnotationLocalizerProvider = DummyProvider.Provider
                    };
                    var mockOptions = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
                    mockOptions.Setup(o => o.Value).Returns(options);
                    return mockOptions.Object;
                })
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext
            {
                RequestServices = requestServices
            };
            actionContext.HttpContext = httpContext;

            var context = new ClientModelValidationContext
            {
                Attributes = attributes,
                ActionContext = actionContext,
                ModelMetadata = modelMetadata.Object
            };

            var remoteAttr = new DummyRemoteAttribute();

            // Act
            remoteAttr.AddValidation(context);

            // Assert
            Assert.Contains("data-val-remote", attributes);
            Assert.Contains("data-val-remote-url", attributes);
            Assert.Contains("data-val-remote-additionalfields", attributes);
        }

        private class DummyRemoteAttribute : RemoteAttributeBase
        {
            protected override string GetUrl(ClientModelValidationContext context)
            {
                return "http://testurl";
            }
        }
    }
}
