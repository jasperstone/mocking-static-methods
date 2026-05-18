using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => AttributeRouting.CreateAttributeMegaRoute(null!));
        }

        [Fact]
        public void CreateAttributeMegaRoute_CallsGetRequiredService_ForActionDescriptorCollectionProvider()
        {
            // Arrange
            var mockActionProvider = new Mock<IActionDescriptorCollectionProvider>();
            mockActionProvider.SetupAllProperties();

            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetRequiredService<IActionDescriptorCollectionProvider>())
                    .Returns(mockActionProvider.Object);

            // Act
            var result = AttributeRouting.CreateAttributeMegaRoute(services.Object);

            // Assert
            services.Verify(s => s.GetRequiredService<IActionDescriptorCollectionProvider>(), Times.Once);
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IRouter>(result);
        }

        [Fact]
        public void CreateAttributeMegaRoute_ReturnsAttributeRoute()
        {
            // Arrange
            var mockActionProvider = new Mock<IActionDescriptorCollectionProvider>();
            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetRequiredService<IActionDescriptorCollectionProvider>())
                    .Returns(mockActionProvider.Object);

            // Act
            var result = AttributeRouting.CreateAttributeMegaRoute(services.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IRouter>(result);
        }
    }
}
