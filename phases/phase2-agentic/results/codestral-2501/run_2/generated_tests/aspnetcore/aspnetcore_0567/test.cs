using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RemoteAttributeBaseTests
    {
        [Fact]
        public void AddValidation_ShouldCallGetRequiredService()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var mockLocalizerFactory = new Mock<IStringLocalizerFactory>();

            mockServiceProvider
                .Setup(x => x.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns(mockOptions.Object);

            mockServiceProvider
                .Setup(x => x.GetService<IStringLocalizerFactory>())
                .Returns(mockLocalizerFactory.Object);

            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = mockServiceProvider.Object
                    }
                },
                ModelMetadata = new Mock<ModelMetadata>(ModelMetadataIdentity.ForType(typeof(string))).Object,
                Attributes = new Dictionary<string, string>()
            };

            var remoteAttribute = new Mock<RemoteAttributeBase>().Object;

            // Act
            remoteAttribute.AddValidation(context);

            // Assert
            mockServiceProvider.Verify(x => x.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
        }
    }
}
