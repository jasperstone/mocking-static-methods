using Xunit;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Tests
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        [Fact]
        public void CreateInstance_ShouldReturnControllerSaveTempDataPropertyFilter()
        {
            // Arrange
            var tempDataProperties = new List<LifecycleProperty>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var tempDataFactoryMock = new Mock<ITempDataDictionaryFactory>();
            var controllerSaveTempDataPropertyFilter = new ControllerSaveTempDataPropertyFilter(tempDataFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>())
                .Returns(controllerSaveTempDataPropertyFilter);

            var factory = new ControllerSaveTempDataPropertyFilterFactory(tempDataProperties);

            // Act
            var result = factory.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ControllerSaveTempDataPropertyFilter>(result);
            Assert.Equal(tempDataProperties, ((ControllerSaveTempDataPropertyFilter)result).Properties);
        }

        [Fact]
        public void CreateInstance_ShouldThrowArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            var tempDataProperties = new List<LifecycleProperty>();
            var factory = new ControllerSaveTempDataPropertyFilterFactory(tempDataProperties);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null));
        }
    }
}
