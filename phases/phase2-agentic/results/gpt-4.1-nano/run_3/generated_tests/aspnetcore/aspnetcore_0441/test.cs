using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace ControllerBaseTests
{
    public class ControllerBaseMock : ControllerBase
    {
        public ControllerBaseMock() { }
    }

    public class ControllerBaseUnitTests
    {
        private ControllerContext CreateControllerContextWithServices(IServiceProvider serviceProvider)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            return controllerContext;
        }

        [Fact]
        public void MetadataProvider_Should_Call_GetRequiredService_When_Null()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var controller = new ControllerBaseMock();
            controller.ControllerContext = CreateControllerContextWithServices(provider);

            // Act
            var metadataProvider = controller.MetadataProvider;

            // Assert
            Assert.NotNull(metadataProvider);
        }

        [Fact]
        public void ModelBinderFactory_Should_Call_GetRequiredService_When_Null()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var controller = new ControllerBaseMock();
            controller.ControllerContext = CreateControllerContextWithServices(provider);

            // Act
            var modelBinderFactory = controller.ModelBinderFactory;

            // Assert
            Assert.NotNull(modelBinderFactory);
        }

        [Fact]
        public void Url_Should_Call_GetRequiredService_When_Null()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var controller = new ControllerBaseMock();
            controller.ControllerContext = CreateControllerContextWithServices(provider);

            // Act
            var urlHelper = controller.Url;

            // Assert
            Assert.NotNull(urlHelper);
        }

        [Fact]
        public void ObjectValidator_Should_Call_GetRequiredService_When_Null()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var controller = new ControllerBaseMock();
            controller.ControllerContext = CreateControllerContextWithServices(provider);

            // Act
            var validator = controller.ObjectValidator;

            // Assert
            Assert.NotNull(validator);
        }

        [Fact]
        public void ProblemDetailsFactory_Should_Call_GetRequiredService_When_Null()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var controller = new ControllerBaseMock();
            controller.ControllerContext = CreateControllerContextWithServices(provider);

            // Act
            var factory = controller.ProblemDetailsFactory;

            // Assert
            Assert.NotNull(factory);
        }
    }
}
