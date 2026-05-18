using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RemoteAttributeBaseTests
    {
        [Fact]
        public void CheckForLocalizer_GetRequiredService_CallsGetRequiredService()
        {
            // Arrange
            var services = new Mock<IServiceProvider>();
            var options = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var factory = new Mock<IStringLocalizerFactory>();
            var provider = new Mock<Func<Type, IStringLocalizerFactory, IStringLocalizer>>();
            var context = new ClientModelValidationContext(new ActionContext(), new ModelMetadata(), new ViewContext(), new HtmlHelper());

            services.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>()).Returns(options.Object);
            services.Setup(s => s.GetService<IStringLocalizerFactory>()).Returns(factory.Object);
            options.Setup(o => o.Value.DataAnnotationLocalizerProvider).Returns(provider.Object);

            var attribute = new RemoteAttributeBase();

            // Act
            attribute.CheckForLocalizer(context);

            // Assert
            services.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
        }

        [Fact]
        public void CheckForLocalizer_GetService_CallsGetService()
        {
            // Arrange
            var services = new Mock<IServiceProvider>();
            var options = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var factory = new Mock<IStringLocalizerFactory>();
            var provider = new Mock<Func<Type, IStringLocalizerFactory, IStringLocalizer>>();
            var context = new ClientModelValidationContext(new ActionContext(), new ModelMetadata(), new ViewContext(), new HtmlHelper());

            services.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>()).Returns(options.Object);
            services.Setup(s => s.GetService<IStringLocalizerFactory>()).Returns(factory.Object);
            options.Setup(o => o.Value.DataAnnotationLocalizerProvider).Returns(provider.Object);

            var attribute = new RemoteAttributeBase();

            // Act
            attribute.CheckForLocalizer(context);

            // Assert
            services.Verify(s => s.GetService<IStringLocalizerFactory>(), Times.Once);
        }

        [Fact]
        public void CheckForLocalizer_ProviderNotNull_SetsStringLocalizer()
        {
            // Arrange
            var services = new Mock<IServiceProvider>();
            var options = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var factory = new Mock<IStringLocalizerFactory>();
            var provider = new Mock<Func<Type, IStringLocalizerFactory, IStringLocalizer>>();
            var context = new ClientModelValidationContext(new ActionContext(), new ModelMetadata(), new ViewContext(), new HtmlHelper());

            services.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>()).Returns(options.Object);
            services.Setup(s => s.GetService<IStringLocalizerFactory>()).Returns(factory.Object);
            options.Setup(o => o.Value.DataAnnotationLocalizerProvider).Returns(provider.Object);

            var attribute = new RemoteAttributeBase();

            // Act
            attribute.CheckForLocalizer(context);

            // Assert
            Assert.NotNull(attribute._stringLocalizer);
        }
    }
}
