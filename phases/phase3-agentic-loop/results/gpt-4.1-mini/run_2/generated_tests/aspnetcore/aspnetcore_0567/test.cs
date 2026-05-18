using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Test
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
            var servicesMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();

            var optionsValue = new MvcDataAnnotationsLocalizationOptions();
            optionsMock.Setup(o => o.Value).Returns(optionsValue);

            servicesMock.Setup(s => s.GetService(typeof(IOptions<MvcDataAnnotationsLocalizationOptions>)))
                .Returns(optionsMock.Object);
            servicesMock.Setup(s => s.GetService(typeof(IStringLocalizerFactory)))
                .Returns(localizerFactoryMock.Object);

            var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(servicesMock.Object);

            var actionContext = new ActionContext()
            {
                HttpContext = httpContextMock.Object
            };

            var modelMetadataMock = new Mock<ModelMetadata>(ModelMetadataIdentity.ForType(typeof(string)));
            modelMetadataMock.Setup(m => m.GetDisplayName()).Returns("DisplayName");
            modelMetadataMock.Setup(m => m.PropertyName).Returns("PropertyName");
            modelMetadataMock.Setup(m => m.ContainerType).Returns(typeof(string));
            modelMetadataMock.Setup(m => m.ModelType).Returns(typeof(string));

            var clientModelValidationContext = new ClientModelValidationContext(
                actionContext,
                modelMetadataMock.Object,
                new Dictionary<string, string>());

            var attribute = new TestRemoteAttribute("http://test-url");

            // Act
            attribute.AddValidation(clientModelValidationContext);

            // Assert
            servicesMock.Verify(s => s.GetService(typeof(IOptions<MvcDataAnnotationsLocalizationOptions>)), Times.Once);
        }
    }
}
