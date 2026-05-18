using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Http.Validation
{
    public class ValidationEndpointFilterFactoryTests
    {
        [Fact]
        public async Task Create_Filter_WithValidationOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<ValidationOptions>()
                .Configure<ValidationOptions>(options =>
                {
                    options.Resolvers = new List<IValidationResolver>();
                })
                .BuildServiceProvider();

            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = serviceProvider,
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod)),
            };

            var next = new EndpointFilterDelegate(async context => await Task.CompletedTask);

            // Act
            var filter = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotNull(filter);
        }

        [Fact]
        public async Task Create_Filter_WithoutValidationOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = serviceProvider,
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod)),
            };

            var next = new EndpointFilterDelegate(async context => await Task.CompletedTask);

            // Act
            var filter = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotNull(filter);
        }

        [Fact]
        public async Task Create_Filter_WithValidationOptions_GetServiceCalled()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<ValidationOptions>()
                .Configure<ValidationOptions>(options =>
                {
                    options.Resolvers = new List<IValidationResolver>();
                })
                .BuildServiceProvider();

            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = serviceProvider,
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod)),
            };

            var next = new EndpointFilterDelegate(async context => await Task.CompletedTask);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IOptions<ValidationOptions>))).Returns(new OptionsWrapper<ValidationOptions>(new ValidationOptions()));

            context.ApplicationServices = mockServiceProvider.Object;

            // Act
            var filter = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            mockServiceProvider.Verify(x => x.GetService(typeof(IOptions<ValidationOptions>)), Times.Once);
        }

        private void TestMethod([FromServices] object obj)
        {
        }
    }
}
