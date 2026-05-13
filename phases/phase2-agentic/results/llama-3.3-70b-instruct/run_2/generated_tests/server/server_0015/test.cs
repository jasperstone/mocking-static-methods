using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task UpdateAsync_CreatesAsyncScope()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var mapperMock = new Mock<IMapper>();
            var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            var secret = new Core.SecretsManager.Entities.Secret();

            // Act
            await secretRepository.UpdateAsync(secret);

            // Assert
            serviceScopeFactoryMock.Verify(ssf => ssf.CreateAsyncScope(), Times.Once);
        }
    }
}
