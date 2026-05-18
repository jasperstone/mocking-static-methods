using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Bit.Core.SecretsManager.Entities;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task RestoreManyByIdAsync_RestoresSecrets()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var mapperMock = new Mock<IMapper>();

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());

            serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

            var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            await secretRepository.RestoreManyByIdAsync(secretIds);

            // Assert
            // TODO: Add assertions here
        }
    }
}
