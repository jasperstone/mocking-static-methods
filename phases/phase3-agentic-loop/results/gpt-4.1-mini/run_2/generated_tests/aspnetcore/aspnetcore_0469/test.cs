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
    public class ArrayModelBinderProviderTests
    {
        private class TestModelMetadata : ModelMetadata
        {
            public TestModelMetadata(Type modelType, ModelMetadata? elementMetadata = null)
                : base(new EmptyModelMetadataProvider(), null, null, modelType, null)
            {
                ElementMetadata = elementMetadata;
            }

            public override ModelMetadata? ElementMetadata { get; }

            public override IReadOnlyDictionary<object, object> AdditionalValues => new Dictionary<object, object>();

            public override ModelPropertyCollection Properties => ModelPropertyCollection.Empty;

            public override string? BinderModelName => null;

            public override BindingSource? BindingSource => null;

            public override bool ConvertEmptyStringToNull => true;

            public override string? DataTypeName => null;

            public override string? Description => null;

            public override string? DisplayFormatString => null;

            public override string? DisplayName => null;

            public override string? EditFormatString => null;

            public override ModelMetadata? ElementMetadataOverride => null;

            public override bool HasNonDefaultEditFormat => false;

            public override bool HideSurroundingHtml => false;

            public override bool HtmlEncode => true;

            public override bool IsBindingAllowed => true;

            public override bool IsBindingRequired => false;

            public override bool IsEnum => false;

            public override bool IsFlagsEnum => false;

            public override bool IsReadOnly => false;

            public override bool IsRequired => false;

            public override ModelBindingMessageProvider? ModelBindingMessageProvider => null;

            public override string? NullDisplayText => null;

            public override int Order => 0;

            public override string? Placeholder => null;

            public override ModelPropertyCollection PropertiesOverride => ModelPropertyCollection.Empty;

            public override bool ShowForDisplay => true;

            public override bool ShowForEdit => true;

            public override string? SimpleDisplayProperty => null;

            public override string? TemplateHint => null;

            public override bool ValidateChildren => true;
        }

        private class TestModelBinderProviderContext : ModelBinderProviderContext
        {
            public TestModelBinderProviderContext(ModelMetadata metadata, IServiceProvider services)
            {
                Metadata = metadata;
                Services = services;
            }

            public override ModelMetadata Metadata { get; }

            public override IServiceProvider Services { get; }

            public override IModelBinder CreateBinder(ModelMetadata metadata)
            {
                // Return a dummy binder for element binder
                return new SimpleTypeModelBinder(metadata.ModelType);
            }
        }

        [Fact]
        public void GetBinder_ReturnsArrayModelBinder_WhenModelTypeIsArray_AndServicesAreCalled()
        {
            // Arrange
            var elementType = typeof(int);
            var arrayType = elementType.MakeArrayType();

            var elementMetadata = new TestModelMetadata(elementType);
            var metadata = new TestModelMetadata(arrayType, elementMetadata);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<MvcOptions>))).Returns(optionsMock.Object);

            // The extension method GetRequiredService calls GetService internally, so we need to setup GetService
            // But since the code calls GetRequiredService, which throws if null, we must ensure non-null returns.

            // We will use the real extension method by setting up the service provider mock to call the real extension
            // But Moq does not support extension methods directly, so we setup GetService to return mocks.

            var context = new TestModelBinderProviderContext(metadata, serviceProviderMock.Object);

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType(typeof(ArrayModelBinder<int>), binder);

            // Verify that GetService was called for ILoggerFactory and IOptions<MvcOptions>
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce());
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<MvcOptions>)), Times.AtLeastOnce());
        }

        [Fact]
        public void GetBinder_ReturnsNull_WhenModelTypeIsNotArray()
        {
            // Arrange
            var modelType = typeof(int);
            var metadata = new TestModelMetadata(modelType);

            var serviceProviderMock = new Mock<IServiceProvider>();

            var context = new TestModelBinderProviderContext(metadata, serviceProviderMock.Object);

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }
    }
}
