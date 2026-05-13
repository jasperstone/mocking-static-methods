using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class DictionaryModelBinderProviderTests
    {
        private class TestModelMetadata : ModelMetadata
        {
            public TestModelMetadata(Type modelType) : base(ModelMetadataIdentity.ForType(modelType))
            {
            }
        }

        private class TestModelBinderProviderContext : ModelBinderProviderContext
        {
            public TestModelBinderProviderContext(Type modelType, IServiceProvider services)
            {
                Metadata = new TestModelMetadata(modelType);
                MetadataProvider = new EmptyModelMetadataProvider();
                Services = services;
            }

            public override ModelMetadata Metadata { get; }

            public override IModelMetadataProvider MetadataProvider { get; }

            public override IServiceProvider Services { get; }

            public override IModelBinder CreateBinder(ModelMetadata metadata)
            {
                // Return a dummy binder for testing
                return new NoOpBinder();
            }
        }

        private class NoOpBinder : IModelBinder
        {
            public System.Threading.Tasks.Task BindModelAsync(ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void GetBinder_ReturnsNull_ForNonDictionaryType()
        {
            // Arrange
            var services = new Mock<IServiceProvider>(MockBehavior.Strict);
            var context = new TestModelBinderProviderContext(typeof(string), services.Object);
            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ReturnsDictionaryModelBinder_ForDictionaryType()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            servicesMock.Setup(s => s.GetRequiredService(typeof(IOptions<MvcOptions>))).Returns(optionsMock.Object);

            var modelType = typeof(Dictionary<string, int>);
            var context = new TestModelBinderProviderContext(modelType, servicesMock.Object);
            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }
    }
}
