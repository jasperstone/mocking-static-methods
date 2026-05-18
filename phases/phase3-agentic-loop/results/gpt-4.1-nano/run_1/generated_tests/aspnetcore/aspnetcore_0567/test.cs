using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    public class RemoteAttributeBaseTests
    {
        private class DummyLocalizer : IStringLocalizer
        {
            public LocalizedString this[string name, params object[] arguments] => new LocalizedString(name, $"Localized: {name}");
            public LocalizedString this[string name] => new LocalizedString(name, $"Localized: {name}");
            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();
            public IStringLocalizer WithCulture(System.Globalization.CultureInfo culture) => this;
        }

        [Fact]
        public void CheckForLocalizer_Should_Call_GetRequiredService_And_Set_Localizer()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var requestServicesMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            var localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            var localizerProvider = new Func<Type?, IStringLocalizerFactory?, IStringLocalizer>((type, factory) => new DummyLocalizer());

            var localizationOptions = new MvcDataAnnotationsLocalizationOptions
            {
                DataAnnotationLocalizerProvider = localizerProvider
            };

            var optionsWrapper = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            optionsWrapper.Setup(o => o.Value).Returns(localizationOptions);

            var requestServices = new ServiceCollection()
                .AddSingleton(optionsWrapper.Object)
                .AddSingleton(localizerFactoryMock.Object)
                .BuildServiceProvider();

            var contextMock = new Mock<ClientModelValidationContext>();
            var actionContextMock = new Mock<ActionContext>();
            var httpContextMock = new Mock<HttpContext>();
            var requestServicesMockObj = new Mock<IServiceProvider>();
            requestServicesMockObj.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns(optionsWrapper.Object);
            requestServicesMockObj.Setup(s => s.GetService<IStringLocalizerFactory>())
                .Returns(localizerFactoryMock.Object);

            var modelMetadataMock = new Mock<ModelMetadata>();
            modelMetadataMock.Setup(m => m.ContainerType).Returns(typeof(object));
            modelMetadataMock.Setup(m => m.ModelType).Returns(typeof(object));
            modelMetadataMock.Setup(m => m.PropertyName).Returns("PropertyName");

            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = requestServicesMockObj.Object
                }
            };

            var context = new ClientModelValidationContext
            {
                ActionContext = actionContext,
                ModelMetadata = modelMetadataMock.Object,
                Attributes = new Dictionary<string, string>()
            };

            var remoteAttribute = new RemoteAttributeBase();

            // Act
            remoteAttribute.GetType().GetMethod("CheckForLocalizer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(remoteAttribute, new object[] { context });

            // Assert
            Assert.True(remoteAttribute.GetType().GetField("_checkedForLocalizer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(remoteAttribute) as bool? == true);
            Assert.NotNull(remoteAttribute.GetType().GetField("_stringLocalizer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(remoteAttribute));
        }
    }
}
