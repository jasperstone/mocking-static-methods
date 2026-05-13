using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
using Moq;
using Xunit;

namespace ValidationEndpointFilterFactoryTests
{
    public class CreateMethodTests
    {
        private static MethodInfo GetTestMethod()
        {
            return typeof(TestController).GetMethod(nameof(TestController.TestMethod));
        }

        private class TestController
        {
            public void TestMethod([Display(Name = "TestParam")] string param1, [FromServices] IServiceProvider serviceProvider, string param2)
            {
            }
        }

        [Fact]
        public void ReturnsNextWhenOptionsIsNull()
        {
            // Arrange
            var context = new Mock<EndpointFilterFactoryContext>();
            var methodInfo = GetTestMethod();
            context.Setup(c => c.MethodInfo).Returns(methodInfo);
            var services = new ServiceCollection().BuildServiceProvider();
            context.Setup(c => c.ApplicationServices).Returns(services);
            context.Setup(c => c.Arguments).Returns(new object[] { "value1", "value2" });
            var nextCalled = false;
            EndpointFilterDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.FromResult((object?)null);
            };

            // Act
            var result = ValidationEndpointFilterFactory.Create(context.Object, next);

            // Assert
            Assert.NotNull(result);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task ValidatesParametersAndReturnsProblemDetails()
        {
            // Arrange
            var methodInfo = GetTestMethod();
            var parameters = methodInfo.GetParameters();

            var validationOptionsMock = new Mock<IOptions<ValidationOptions>>();
            var validationOptions = new ValidationOptions();
            validationOptions.Resolvers.Add(new DummyResolver());
            validationOptionsMock.Setup(o => o.Value).Returns(validationOptions);

            var services = new ServiceCollection()
                .AddSingleton(validationOptionsMock.Object)
                .BuildServiceProvider();

            var context = new Mock<EndpointFilterFactoryContext>();
            context.Setup(c => c.MethodInfo).Returns(methodInfo);
            context.Setup(c => c.ApplicationServices).Returns(services);
            context.Setup(c => c.Arguments).Returns(new object?[] { "invalid", null });
            var httpContextMock = new DefaultHttpContext();
            var requestServicesMock = new ServiceCollection()
                .AddSingleton<IProblemDetailsService, DummyProblemDetailsService>()
                .BuildServiceProvider();
            httpContextMock.RequestServices = requestServicesMock;
            var httpContext = httpContextMock;

            var httpContextAccessor = new DefaultHttpContextAccessor { HttpContext = httpContext };
            var contextObj = new Mock<EndpointFilterFactoryContext>();
            contextObj.Setup(c => c.MethodInfo).Returns(methodInfo);
            contextObj.Setup(c => c.ApplicationServices).Returns(services);
            contextObj.Setup(c => c.Arguments).Returns(new object?[] { "invalid", null });
            var delegateNextCalled = false;
            EndpointFilterDelegate next = (ctx) =>
            {
                delegateNextCalled = true;
                return Task.FromResult((object?)null);
            };

            // Act
            var filterDelegate = ValidationEndpointFilterFactory.Create(contextObj.Object, next);
            var result = await filterDelegate(new DefaultEndpointFilterInvocationContext(httpContext, new object?[] { "invalid", null }, methodInfo));

            // Assert
            Assert.NotNull(result);
            Assert.IsType<HttpValidationProblemDetails>(result);
            Assert.True(delegateNextCalled == false);
        }

        [Fact]
        public void IsServiceParameter_ReturnsTrueForServiceParameter()
        {
            // Arrange
            var parameterInfoMock = new Mock<ParameterInfo>();
            parameterInfoMock.Setup(p => p.CustomAttributes).Returns(Enumerable.Empty<CustomAttributeData>().AsQueryable());
            parameterInfoMock.Setup(p => p.ParameterType).Returns(typeof(IServiceProvider));
            var isServiceMock = new Mock<IServiceProviderIsService>();
            isServiceMock.Setup(s => s.IsService(typeof(IServiceProvider))).Returns(true);

            // Act
            var result = ValidationEndpointFilterFactory.IsServiceParameter(parameterInfoMock.Object, isServiceMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasFromServicesAttribute_ReturnsTrueWhenAttributePresent()
        {
            // Arrange
            var attributeData = new CustomAttributeDataMock(typeof(IFromServiceMetadata));
            var parameterInfoMock = new Mock<ParameterInfo>();
            parameterInfoMock.Setup(p => p.CustomAttributes).Returns(new[] { attributeData }.AsQueryable());

            // Act
            var result = ValidationEndpointFilterFactory.HasFromServicesAttribute(parameterInfoMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetDisplayName_ReturnsDisplayNameWhenAttributePresent()
        {
            // Arrange
            var displayAttribute = new DisplayAttribute { Name = "DisplayName" };
            var parameterInfoMock = new Mock<ParameterInfo>();
            parameterInfoMock.Setup(p => p.GetCustomAttribute<DisplayAttribute>()).Returns(displayAttribute);
            parameterInfoMock.Setup(p => p.Name).Returns("ParamName");

            // Act
            var result = ValidationEndpointFilterFactory.GetDisplayName(parameterInfoMock.Object);

            // Assert
            Assert.Equal("DisplayName", result);
        }

        private class CustomAttributeDataMock : CustomAttributeData
        {
            private readonly Type _attributeType;

            public CustomAttributeDataMock(Type attributeType)
            {
                _attributeType = attributeType;
            }

            public override Type AttributeType => _attributeType;
        }
    }
}
