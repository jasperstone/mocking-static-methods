using System.Threading.Tasks;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
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
            var mockDbContext = new Mock<SecretsManagerDbContext>();
            mockScopeFactory.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
            mockScope.Setup(s => s.ServiceProvider.GetService(typeof(SecretsManagerDbContext))).Returns(mockDbContext.Object);

            var mockMapper = new Mock<IMapper>();
            var secretRepository = new SecretRepository(mockScopeFactory.Object, mockMapper.Object);

            var secret = new Secret { Id = Guid.NewGuid() };

            // Act
            await secretRepository.UpdateAsync(secret);

            // Assert
            mockScopeFactory.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}
