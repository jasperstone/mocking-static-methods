using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class ControllerBaseTests
    {
        [Fact]
        public void Url_Should_ReturnUrlHelper_When_HttpContextIsNotNull()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequestServices = new Mock<IServiceProvider>();
            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            var mockUrlHelper = new Mock<IUrlHelper>();

            mockRequestServices.Setup(x => x.GetRequiredService<IUrlHelperFactory>()).Returns(mockUrlHelperFactory.Object);
            mockUrlHelperFactory.Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>())).Returns(mockUrlHelper.Object);
            mockHttpContext.Setup(x => x.RequestServices).Returns(mockRequestServices.Object);

            var controller = new TestController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Act
            var url = controller.Url;

            // Assert
            Assert.NotNull(url);
            Assert.Equal(mockUrlHelper.Object, url);
        }

        [Fact]
        public void Url_Should_Throw_When_HttpContextIsNull()
        {
            // Arrange
            var controller = new TestController();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => controller.Url);
        }

        [Fact]
        public void ModelBinderFactory_Should_ReturnModelBinderFactory_When_HttpContextIsNotNull()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequestServices = new Mock<IServiceProvider>();
            var mockModelBinderFactory = new Mock<IModelBinderFactory>();

            mockRequestServices.Setup(x => x.GetRequiredService<IModelBinderFactory>()).Returns(mockModelBinderFactory.Object);
            mockHttpContext.Setup(x => x.RequestServices).Returns(mockRequestServices.Object);

            var controller = new TestController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Act
            var modelBinderFactory = controller.ModelBinderFactory;

            // Assert
            Assert.NotNull(modelBinderFactory);
            Assert.Equal(mockModelBinderFactory.Object, modelBinderFactory);
        }

        [Fact]
        public void ModelBinderFactory_Should_Throw_When_HttpContextIsNull()
        {
            // Arrange
            var controller = new TestController();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => controller.ModelBinderFactory);
        }

        [Fact]
        public void ObjectValidator_Should_ReturnObjectValidator_When_HttpContextIsNotNull()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequestServices = new Mock<IServiceProvider>();
            var mockObjectValidator = new Mock<IObjectModelValidator>();

            mockRequestServices.Setup(x => x.GetRequiredService<IObjectModelValidator>()).Returns(mockObjectValidator.Object);
            mockHttpContext.Setup(x => x.RequestServices).Returns(mockRequestServices.Object);

            var controller = new TestController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Act
            var objectValidator = controller.ObjectValidator;

            // Assert
            Assert.NotNull(objectValidator);
            Assert.Equal(mockObjectValidator.Object, objectValidator);
        }

        [Fact]
        public void ObjectValidator_Should_Throw_When_HttpContextIsNull()
        {
            // Arrange
            var controller = new TestController();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => controller.ObjectValidator);
        }

        private class TestController : ControllerBase
        {
        }
    }
}
