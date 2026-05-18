using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Enums;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_ValidInput_ReturnsCount()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var mapperMock = new Mock<IMapper>();
            var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            // Act
            var result = await secretRepository.GetSecretsCountByOrganizationIdAsync(Guid.NewGuid(), Guid.NewGuid(), AccessClientType.NoAccessCheck);

            // Assert
            Assert.True(result >= 0);
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_InvalidInput_ThrowsException()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var mapperMock = new Mock<IMapper>();
            var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => secretRepository.GetSecretsCountByOrganizationIdAsync(Guid.Empty, Guid.NewGuid(), AccessClientType.NoAccessCheck));
        }
    }
}
