using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Xunit;

namespace Tests
{
    public class ControllerBaseTests
    {
        [Fact]
        public void ModelBinderFactory_GetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var modelBinderFactoryMock = new Mock<IModelBinderFactory>();
            serviceProviderMock.Setup(p => p.GetRequiredService<IModelBinderFactory>()).Returns(modelBinderFactoryMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            var controllerBase = new ControllerBase
            {
                ControllerContext = controllerContext
            };

            // Act
            var modelBinderFactory = controllerBase.ModelBinderFactory;

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<IModelBinderFactory>(), Times.Once);
        }

        [Fact]
        public void Url_GetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            serviceProviderMock.Setup(p => p.GetRequiredService<IUrlHelperFactory>()).Returns(urlHelperFactoryMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            var controllerBase = new ControllerBase
            {
                ControllerContext = controllerContext
            };

            // Act
            var url = controllerBase.Url;

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<IUrlHelperFactory>(), Times.Once);
        }

        [Fact]
        public void ObjectValidator_GetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var objectModelValidatorMock = new Mock<IObjectModelValidator>();
            serviceProviderMock.Setup(p => p.GetRequiredService<IObjectModelValidator>()).Returns(objectModelValidatorMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            var controllerBase = new ControllerBase
            {
                ControllerContext = controllerContext
            };

            // Act
            var objectValidator = controllerBase.ObjectValidator;

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<IObjectModelValidator>(), Times.Once);
        }
    }
}
