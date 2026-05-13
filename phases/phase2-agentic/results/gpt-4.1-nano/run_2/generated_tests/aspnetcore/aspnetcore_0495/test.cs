using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace RequestFormLimitsAttributeTests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_Should_Call_GetRequiredService_And_Set_FormOptions()
        {
            // Arrange
            var mockFilter = new Mock<RequestFormLimitsFilter>();
            var services = new ServiceCollection();
            services.AddTransient(_ => mockFilter.Object);
            var serviceProvider = services.BuildServiceProvider();

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var result = attribute.CreateInstance(serviceProvider);

            // Assert
            mockFilter.Verify(f => f.FormOptions = It.IsAny<FormOptions>(), Times.Once);
            Assert.Equal(mockFilter.Object, result);
        }

        [Fact]
        public void CreateInstance_Should_Use_ServiceProvider_To_Get_RequestFormLimitsFilter()
        {
            // Arrange
            var mockFilter = new Mock<RequestFormLimitsFilter>();
            var services = new ServiceCollection();
            services.AddTransient(_ => mockFilter.Object);
            var serviceProvider = services.BuildServiceProvider();

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var filterInstance = attribute.CreateInstance(serviceProvider);

            // Assert
            Assert.Same(mockFilter.Object, filterInstance);
        }

        [Fact]
        public void CreateInstance_Should_Throw_When_Service_Not_Registered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var attribute = new RequestFormLimitsAttribute();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => attribute.CreateInstance(serviceProvider));
        }
    }
}
