using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RemoteAttributeBaseTests
    {
        private class TestableRemoteAttribute : RemoteAttributeBase
        {
            public void PublicCheckForLocalizer(ClientModelValidationContext context)
            {
                CheckForLocalizer(context);
            }
        }

        [Fact]
        public void CheckForLocalizer_ShouldCallGetRequiredService()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var factoryMock = new Mock<IStringLocalizerFactory>();
            var providerMock = new Mock<Func<Type, IStringLocalizerFactory, IStringLocalizer>>();

            servicesMock
                .Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns(optionsMock.Object);

            servicesMock
                .Setup(s => s.GetService<IStringLocalizerFactory>())
                .Returns(factoryMock.Object);

            optionsMock
                .Setup(o => o.Value)
                .Returns(new MvcDataAnnotationsLocalizationOptions
                {
                    DataAnnotationLocalizerProvider = providerMock.Object
                });

            var contextMock = new Mock<ClientModelValidationContext>();
            contextMock
                .Setup(c => c.ActionContext.HttpContext.RequestServices)
                .Returns(servicesMock.Object);

            var attribute = new TestableRemoteAttribute();

            // Act
            attribute.PublicCheckForLocalizer(contextMock.Object);

            // Assert
            servicesMock.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
        }
    }
}
