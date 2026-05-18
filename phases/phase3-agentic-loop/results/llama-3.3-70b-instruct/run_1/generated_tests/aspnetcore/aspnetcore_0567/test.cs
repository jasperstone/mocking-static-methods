using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RemoteAttributeBaseTests
    {
        [Fact]
        public void CheckForLocalizer_CallsGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var factoryMock = new Mock<IStringLocalizerFactory>();
            var localizerMock = new Mock<IStringLocalizer>();

            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns(optionsMock.Object);

            serviceProviderMock.Setup(s => s.GetService<IStringLocalizerFactory>())
                .Returns(factoryMock.Object);

            factoryMock.Setup(f => f.Create(It.IsAny<Type>()))
                .Returns(localizerMock.Object);

            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = serviceProviderMock.Object
                    }
                },
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(object))
            };

            var remoteAttributeBase = new RemoteAttribute();

            // Act
            remoteAttributeBase.AddValidation(context);

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
        }
    }
}
