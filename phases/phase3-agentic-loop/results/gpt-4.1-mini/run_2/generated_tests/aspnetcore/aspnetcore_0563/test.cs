using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Filters
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        private class TestController
        {
            public string TempDataProperty { get; set; }
        }

        // We create a derived class to add a public setter for Properties to test the factory sets it.
        private class TestControllerSaveTempDataPropertyFilter : ControllerSaveTempDataPropertyFilter
        {
            public TestControllerSaveTempDataPropertyFilter() : base(Mock.Of<ITempDataDictionaryFactory>())
            {
            }

            public new IReadOnlyList<LifecycleProperty>? Properties { get; set; }
        }

        [Fact]
        public void CreateInstance_CallsGetRequiredServiceAndSetsProperties()
        {
            // Arrange
            var propertyInfo = typeof(TestController).GetProperty(nameof(TestController.TempDataProperty));
            var lifecycleProperty = new LifecycleProperty(propertyInfo!, "TempDataPropertyKey");
            var lifecycleProperties = new List<LifecycleProperty> { lifecycleProperty };

            var filter = new TestControllerSaveTempDataPropertyFilter();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>())
                .Returns(filter);

            var factory = new ControllerSaveTempDataPropertyFilterFactory(lifecycleProperties);

            // Act
            var result = factory.CreateInstance(mockServiceProvider.Object);

            // Assert
            Assert.Same(filter, result);
            Assert.Equal(lifecycleProperties, filter.Properties);
        }
    }
}
