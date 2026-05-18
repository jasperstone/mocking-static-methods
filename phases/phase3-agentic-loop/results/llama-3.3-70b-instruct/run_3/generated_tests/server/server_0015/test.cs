using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SecretRepository _secretRepository;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidSecret_CreatesSecret()
        {
            // Arrange
            var secret = new Core.SecretsManager.Entities.Secret();
            var dbContextMock = new Mock<DbContext>();
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContextMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

            // Act
            await _secretRepository.CreateAsync(secret);

            // Assert
            dbContextMock.Verify(d => d.AddAsync(It.IsAny<Secret>()), Times.Once);
            dbContextMock.Verify(d => d.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ValidSecret_UpdatesSecret()
        {
            // Arrange
            var secret = new Core.SecretsManager.Entities.Secret();
            var dbContextMock = new Mock<DbContext>();
            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContextMock.Object);
            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

            // Act
            await _secretRepository.UpdateAsync(secret);

            // Assert
            dbContextMock.Verify(d => d.SaveChangesAsync(), Times.Once);
        }
    }
}
