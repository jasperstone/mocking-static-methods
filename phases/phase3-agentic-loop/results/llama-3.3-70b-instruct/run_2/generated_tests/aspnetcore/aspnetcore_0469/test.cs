using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ArrayModelType_ReturnsArrayModelBinder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions<MvcOptions>();
            var serviceProvider = services.BuildServiceProvider();
            var arrayModelBinderProvider = new ArrayModelBinderProvider();

            var modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(int[]));
            var context = new ModelBinderProviderContext(new ModelBindingContext
            {
                ModelMetadata = modelMetadata,
                ModelName = "test",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new CompositeValueProvider(new List<IValueProvider>()),
                ValidatorProvider = Mock.Of<IValidatorProvider>(),
                MetadataProvider = new EmptyModelMetadataProvider(),
                HttpContext = new DefaultHttpContext(),
                OperationBindingContext = new OperationBindingContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor())),
                BinderType = typeof(IModelBinder),
                BindingSource = BindingSource.Form,
                FieldName = "test",
                FieldNamePrefix = "",
                IsTopLevelObject = true,
                Model = null,
                ModelType = typeof(int[]),
                Result = ModelBindingResult.Success(null),
                ValidationState = new ValidationStateDictionary(),
            });

            context.Services = serviceProvider;

            // Act
            var binder = arrayModelBinderProvider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<ArrayModelBinder<int>>(binder);
        }

        [Fact]
        public void GetBinder_NonArrayModelType_ReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions<MvcOptions>();
            var serviceProvider = services.BuildServiceProvider();
            var arrayModelBinderProvider = new ArrayModelBinderProvider();

            var modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(int));
            var context = new ModelBinderProviderContext(new ModelBindingContext
            {
                ModelMetadata = modelMetadata,
                ModelName = "test",
                ModelState = new ModelStateDictionary(),
                ValueProvider = new CompositeValueProvider(new List<IValueProvider>()),
                ValidatorProvider = Mock.Of<IValidatorProvider>(),
                MetadataProvider = new EmptyModelMetadataProvider(),
                HttpContext = new DefaultHttpContext(),
                OperationBindingContext = new OperationBindingContext(new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor())),
                BinderType = typeof(IModelBinder),
                BindingSource = BindingSource.Form,
                FieldName = "test",
                FieldNamePrefix = "",
                IsTopLevelObject = true,
                Model = null,
                ModelType = typeof(int),
                Result = ModelBindingResult.Success(null),
                ValidationState = new ValidationStateDictionary(),
            });

            context.Services = serviceProvider;

            // Act
            var binder = arrayModelBinderProvider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }
    }
}
