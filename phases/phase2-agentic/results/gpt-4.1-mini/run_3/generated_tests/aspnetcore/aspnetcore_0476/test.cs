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
            var provider = new DictionaryModelBinderProvider();

            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForType(typeof(string));

            var context = new Mock<ModelBinderProviderContext>();
            context.Setup(c => c.Metadata).Returns(metadata);
            context.Setup(c => c.MetadataProvider).Returns(metadataProvider);
            context.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(Mock.Of<IModelBinder>());

            var services = new ServiceCollection().BuildServiceProvider();
            context.Setup(c => c.Services).Returns(services);

            var result = provider.GetBinder(context.Object);

            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_ReturnsDictionaryModelBinder_ForIDictionaryType()
        {
            var provider = new DictionaryModelBinderProvider();

            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForType(typeof(Dictionary<string, int>));

            var context = new Mock<ModelBinderProviderContext>();
            context.Setup(c => c.Metadata).Returns(metadata);
            context.Setup(c => c.MetadataProvider).Returns(metadataProvider);
            context.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(Mock.Of<IModelBinder>());

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            servicesMock.Setup(s => s.GetRequiredService(typeof(IOptions<MvcOptions>))).Returns(optionsMock.Object);

            context.Setup(c => c.Services).Returns(servicesMock.Object);

            var binder = provider.GetBinder(context.Object);

            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }
    }
}
