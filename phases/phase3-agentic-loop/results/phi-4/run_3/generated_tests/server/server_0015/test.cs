using System;
using System.Threading.Tasks;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Models;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task UpdateAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockScope = new Mock<IServiceScope>();
            var mockAsyncScope = new Mock<IServiceScope>();
            mockScopeFactory.Setup(sf => sf.CreateScope()).Returns(mockScope.Object);
            mockScopeFactory.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(mockAsyncScope.Object);

            var mockMapper = new Mock<IMapper>();
            var repository = new SecretRepository(mockScopeFactory.Object, mockMapper.Object);

            var secret = new Secret { Id = Guid.NewGuid() };

            // Act
            await repository.UpdateAsync(secret);

            // Assert
            mockScopeFactory.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}
