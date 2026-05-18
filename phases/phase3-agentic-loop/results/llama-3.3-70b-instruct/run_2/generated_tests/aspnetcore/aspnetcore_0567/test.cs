using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RemoteAttributeBaseTests
    {
        [Fact]
        public void CheckForLocalizer_CallsGetRequiredService()
        {
            // Arrange
            var services = new Mock<IServiceProvider>();
            var options = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var factory = new Mock<IStringLocalizerFactory>();
            var context = new Mock<ClientModelValidationContext>();

            services.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns(options.Object);

            services.Setup(s => s.GetService<IStringLocalizerFactory>())
                .Returns(factory.Object);

            // Act
            var remoteAttributeBase = new RemoteAttributeBase();
            context.Setup(c => c.ActionContext.HttpContext.RequestServices).Returns(services.Object);
            remoteAttributeBase.CheckForLocalizer(context.Object);

            // Assert
            services.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
        }

        [Fact]
        public void CheckForLocalizer_SetsStringLocalizer()
        {
            // Arrange
            var services = new Mock<IServiceProvider>();
            var options = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var factory = new Mock<IStringLocalizerFactory>();
            var context = new Mock<ClientModelValidationContext>();
            var stringLocalizer = new Mock<IStringLocalizer>();

            services.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns(options.Object);

            services.Setup(s => s.GetService<IStringLocalizerFactory>())
                .Returns(factory.Object);

            factory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<Type[]>()))
                .Returns(stringLocalizer.Object);

            options.Setup(o => o.Value.DataAnnotationLocalizerProvider).Returns((Type type, IStringLocalizerFactory factory) => factory.Create(string.Empty, new[] { type }));

            // Act
            var remoteAttributeBase = new RemoteAttributeBase();
            context.Setup(c => c.ActionContext.HttpContext.RequestServices).Returns(services.Object);
            context.Setup(c => c.ModelMetadata.ContainerType).Returns(typeof(object));
            remoteAttributeBase.CheckForLocalizer(context.Object);

            // Assert
            Assert.NotNull(remoteAttributeBase._stringLocalizer);
        }
    }
}
