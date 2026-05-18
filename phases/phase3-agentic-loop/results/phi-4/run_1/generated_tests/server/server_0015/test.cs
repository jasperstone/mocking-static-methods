using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using bitwarden_license.src.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using bitwarden_license.src.Commercial.Infrastructure.EntityFramework.SecretsManager.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Bitwarden.Tests
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task UpdateAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbContext = new Mock<SecretsManagerDbContext>();
            var mockMapper = new Mock<IMapper>();

            mockScopeFactory.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<SecretsManagerDbContext>()).Returns(mockDbContext.Object);

            var secretRepository = new SecretRepository(mockScopeFactory.Object, mockMapper.Object);
            var secret = new Secret { Id = Guid.NewGuid() };

            // Act
            await secretRepository.UpdateAsync(secret);

            // Assert
            mockScopeFactory.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}
