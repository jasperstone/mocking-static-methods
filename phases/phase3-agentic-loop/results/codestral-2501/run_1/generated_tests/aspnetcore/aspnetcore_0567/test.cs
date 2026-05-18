using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        public void CheckForLocalizer_InitializesStringLocalizer_WhenServicesAreAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var mockLocalizerFactory = new Mock<IStringLocalizerFactory>();
            var mockLocalizer = new Mock<IStringLocalizer>();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IOptions<MvcDataAnnotationsLocalizationOptions>)))
                .Returns(mockOptions.Object);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IStringLocalizerFactory)))
                .Returns(mockLocalizerFactory.Object);

            mockOptions.Setup(o => o.Value).Returns(new MvcDataAnnotationsLocalizationOptions
            {
                DataAnnotationLocalizerProvider = (type, factory) => mockLocalizer.Object
            });

            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = mockServiceProvider.Object
                    }
                },
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(object))
            };

            var attribute = new Mock<RemoteAttributeBase>().Object;

            // Act
            typeof(RemoteAttributeBase)
                .GetMethod("CheckForLocalizer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(attribute, new object[] { context });

            // Assert
            var stringLocalizerField = typeof(RemoteAttributeBase)
                .GetField("_stringLocalizer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var stringLocalizer = stringLocalizerField.GetValue(attribute) as IStringLocalizer;

            Assert.NotNull(stringLocalizer);
            Assert.Same(mockLocalizer.Object, stringLocalizer);
        }
    }
}
