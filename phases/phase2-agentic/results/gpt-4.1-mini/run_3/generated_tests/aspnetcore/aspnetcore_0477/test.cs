using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ReturnsNull_IfContextIsNull()
        {
            var provider = new DictionaryModelBinderProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
        }

        [Fact]
        public void GetBinder_ReturnsNull_IfModelTypeIsNotIDictionary()
        {
            var services = new ServiceCollection().BuildServiceProvider();

            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForType(typeof(string));

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.SetupGet(c => c.Metadata).Returns(metadata);
            contextMock.SetupGet(c => c.MetadataProvider).Returns(metadataProvider);
            contextMock.SetupGet(c => c.Services).Returns(services);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(Mock.Of<IModelBinder>());

            var provider = new DictionaryModelBinderProvider();

            var result = provider.GetBinder(contextMock.Object);

            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_ReturnsDictionaryModelBinder_ForIDictionary()
        {
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            var mvcOptions = new MvcOptions();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);
            services.AddSingleton(optionsMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            var metadataProvider = new EmptyModelMetadataProvider();
            var dictionaryType = typeof(Dictionary<string, int>);
            var metadata = metadataProvider.GetMetadataForType(dictionaryType);

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.SetupGet(c => c.Metadata).Returns(metadata);
            contextMock.SetupGet(c => c.MetadataProvider).Returns(metadataProvider);
            contextMock.SetupGet(c => c.Services).Returns(serviceProvider);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(Mock.Of<IModelBinder>());

            var provider = new DictionaryModelBinderProvider();

            var binder = provider.GetBinder(contextMock.Object);

            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }
    }
}
