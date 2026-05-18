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
    // Concrete subclass for testing
    public class TestableRemoteAttribute : RemoteAttributeBase
    {
        protected override string GetUrl(ClientModelValidationContext context)
        {
            return "http://example.com";
        }
    }

    public class RemoteAttributeBaseTests
    {
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

            var actionContextMock = new Mock<ActionContext>();
            var httpContextMock = new Mock<HttpContext>();
            var requestServicesMock = new Mock<IServiceProvider>();

            requestServicesMock
                .Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns(optionsMock.Object);

            requestServicesMock
                .Setup(s => s.GetService<IStringLocalizerFactory>())
                .Returns(factoryMock.Object);

            httpContextMock
                .Setup(ctx => ctx.RequestServices)
                .Returns(requestServicesMock.Object);

            actionContextMock
                .Setup(ctx => ctx.HttpContext)
                .Returns(httpContextMock.Object);

            var modelMetadataMock = new Mock<ModelMetadata>();
            var modelMetadataProviderMock = new Mock<IModelMetadataProvider>();
            modelMetadataProviderMock
                .Setup(p => p.GetMetadataForType(typeof(string)))
                .Returns(modelMetadataMock.Object);

            var contextMock = new Mock<ClientModelValidationContext>();
            contextMock
                .Setup(c => c.ActionContext)
                .Returns(actionContextMock.Object);
            contextMock
                .Setup(c => c.ModelMetadata)
                .Returns(modelMetadataMock.Object);

            var attribute = new TestableRemoteAttribute();

            // Act
            attribute.CheckForLocalizer(contextMock.Object);

            // Assert
            servicesMock.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
        }
    }
}
