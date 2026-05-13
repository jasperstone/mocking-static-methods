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
        [Fact]
        public void CheckForLocalizer_WhenServicesAreAvailable_SetsStringLocalizer()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequestServices = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var mockProvider = new Mock<Func<Type, IStringLocalizerFactory, IStringLocalizer>>();
            var mockFactory = new Mock<IStringLocalizerFactory>();

            mockOptions.Setup(o => o.Value).Returns(new MvcDataAnnotationsLocalizationOptions
            {
                DataAnnotationLocalizerProvider = mockProvider.Object
            });

            mockRequestServices.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns(mockOptions.Object);

            mockRequestServices.Setup(s => s.GetService<IStringLocalizerFactory>())
                .Returns(mockFactory.Object);

            var mockActionContext = new Mock<ActionContext>();
            mockActionContext.SetupGet(a => a.HttpContext).Returns(mockHttpContext.Object);

            var mockModelMetadata = new Mock<ModelMetadata>();
            var mockClientModelValidationContext = new Mock<ClientModelValidationContext>(
                mockActionContext.Object, mockModelMetadata.Object, "key", "value");

            var remoteAttributeBase = new RemoteAttributeBase
            {
                _checkedForLocalizer = false
            };

            // Act
            remoteAttributeBase.CheckForLocalizer(mockClientModelValidationContext.Object);

            // Assert
            mockRequestServices.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
            mockRequestServices.Verify(s => s.GetService<IStringLocalizerFactory>(), Times.Once);
            mockProvider.Verify(p => p(It.IsAny<Type>(), It.IsAny<IStringLocalizerFactory>()), Times.Once);
            Assert.NotNull(remoteAttributeBase._stringLocalizer);
        }

        [Fact]
        public void CheckForLocalizer_WhenServicesAreNotAvailable_DoesNotSetStringLocalizer()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequestServices = new Mock<IServiceProvider>();

            mockRequestServices.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns<IOptions<MvcDataAnnotationsLocalizationOptions>>(null);

            mockRequestServices.Setup(s => s.GetService<IStringLocalizerFactory>())
                .Returns<IStringLocalizerFactory>(null);

            var mockActionContext = new Mock<ActionContext>();
            mockActionContext.SetupGet(a => a.HttpContext).Returns(mockHttpContext.Object);

            var mockModelMetadata = new Mock<ModelMetadata>();
            var mockClientModelValidationContext = new Mock<ClientModelValidationContext>(
                mockActionContext.Object, mockModelMetadata.Object, "key", "value");

            var remoteAttributeBase = new RemoteAttributeBase
            {
                _checkedForLocalizer = false
            };

            // Act
            remoteAttributeBase.CheckForLocalizer(mockClientModelValidationContext.Object);

            // Assert
            mockRequestServices.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
            mockRequestServices.Verify(s => s.GetService<IStringLocalizerFactory>(), Times.Once);
            Assert.Null(remoteAttributeBase._stringLocalizer);
        }
    }
}
