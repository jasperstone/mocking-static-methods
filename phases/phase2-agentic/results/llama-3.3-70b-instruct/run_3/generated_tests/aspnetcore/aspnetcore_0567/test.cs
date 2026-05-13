using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class RemoteAttributeBaseTests
    {
        [Fact]
        public void CheckForLocalizer_GetRequiredServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var factoryMock = new Mock<IStringLocalizerFactory>();
            var contextMock = new Mock<ClientModelValidationContext>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns(optionsMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService<IStringLocalizerFactory>())
                .Returns(factoryMock.Object);

            contextMock
                .SetupGet(c => c.ActionContext.HttpContext.RequestServices)
                .Returns(serviceProviderMock.Object);

            var remoteAttributeBase = new RemoteAttribute();

            // Act
            remoteAttributeBase.CheckForLocalizer(contextMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
        }

        [Fact]
        public void CheckForLocalizer_GetServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var factoryMock = new Mock<IStringLocalizerFactory>();
            var contextMock = new Mock<ClientModelValidationContext>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns(optionsMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService<IStringLocalizerFactory>())
                .Returns(factoryMock.Object);

            contextMock
                .SetupGet(c => c.ActionContext.HttpContext.RequestServices)
                .Returns(serviceProviderMock.Object);

            var remoteAttributeBase = new RemoteAttribute();

            // Act
            remoteAttributeBase.CheckForLocalizer(contextMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<IStringLocalizerFactory>(), Times.Once);
        }

        private class RemoteAttribute : RemoteAttributeBase
        {
            protected override string GetUrl(ClientModelValidationContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
