using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        public void CheckForLocalizer_LocalizerFactoryIsNull_LocalizerIsNotSet()
        {
            // Arrange
            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = new ServiceCollection().BuildServiceProvider()
                    }
                }
            };

            var attribute = new TestRemoteAttributeBase();

            // Act
            attribute.CheckForLocalizer(context);

            // Assert
            Assert.Null(attribute._stringLocalizer);
        }

        [Fact]
        public void CheckForLocalizer_LocalizerFactoryIsNotNull_LocalizerIsSet()
        {
            // Arrange
            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            var localizerMock = new Mock<IStringLocalizer>();
            localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>(), It.IsAny<IStringLocalizerFactory>())).Returns(localizerMock.Object);

            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = new ServiceCollection()
                            .AddSingleton(localizerFactoryMock.Object)
                            .BuildServiceProvider()
                    }
                },
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(object))
            };

            var attribute = new TestRemoteAttributeBase();

            // Act
            attribute.CheckForLocalizer(context);

            // Assert
            Assert.NotNull(attribute._stringLocalizer);
        }

        [Fact]
        public void CheckForLocalizer_OptionsAreNotNull_LocalizerProviderIsCalled()
        {
            // Arrange
            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            var localizerMock = new Mock<IStringLocalizer>();
            localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>(), It.IsAny<IStringLocalizerFactory>())).Returns(localizerMock.Object);

            var optionsMock = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new MvcDataAnnotationsLocalizationOptions());

            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        RequestServices = new ServiceCollection()
                            .AddSingleton(localizerFactoryMock.Object)
                            .AddSingleton(optionsMock.Object)
                            .BuildServiceProvider()
                    }
                },
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(object))
            };

            var attribute = new TestRemoteAttributeBase();

            // Act
            attribute.CheckForLocalizer(context);

            // Assert
            optionsMock.Verify(o => o.Value, Times.Once);
        }

        private class TestRemoteAttributeBase : RemoteAttributeBase
        {
            public IStringLocalizer? _stringLocalizer { get; set; }
            public bool _checkedForLocalizer { get; set; }

            protected override string GetUrl(ClientModelValidationContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
