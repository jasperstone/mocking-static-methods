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
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            var mockUrlHelper = new Mock<IUrlHelper>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IUrlHelperFactory))).Returns(mockUrlHelperFactory.Object);
            mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(mockUrlHelper.Object);
            mockHttpContext.Setup(hc => hc.RequestServices).Returns(mockServiceProvider.Object);

            var controller = new TestController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Act
            var result = controller.Url;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockUrlHelper.Object, result);
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
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockModelBinderFactory = new Mock<IModelBinderFactory>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IModelBinderFactory))).Returns(mockModelBinderFactory.Object);
            mockHttpContext.Setup(hc => hc.RequestServices).Returns(mockServiceProvider.Object);

            var controller = new TestController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Act
            var result = controller.ModelBinderFactory;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockModelBinderFactory.Object, result);
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
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockObjectValidator = new Mock<IObjectModelValidator>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IObjectModelValidator))).Returns(mockObjectValidator.Object);
            mockHttpContext.Setup(hc => hc.RequestServices).Returns(mockServiceProvider.Object);

            var controller = new TestController
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object
                }
            };

            // Act
            var result = controller.ObjectValidator;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockObjectValidator.Object, result);
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
