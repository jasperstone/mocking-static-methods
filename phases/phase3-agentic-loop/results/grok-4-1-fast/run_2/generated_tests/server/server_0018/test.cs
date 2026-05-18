using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private readonly Mock<IMapper> _mockMapper;
        private readonly SecretRepository _repository;

        public SecretRepositoryTests()
        {
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockMapper = new Mock<IMapper>();
            _repository = new SecretRepository(_mockServiceScopeFactory.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task RestoreManyByIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            
            var mockScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory
                .Setup(f => f.CreateAsyncScope())
                .ReturnsAsync(mockScope.Object);

            var mockDbContext = new Mock<DbContext>();
            mockScope
                .Setup(s => s.ServiceProvider.GetRequiredService<DbContext>())
                .Returns(mockDbContext.Object);

            var mockTransaction = new Mock<IDbContextTransaction>();
            mockDbContext
                .Setup(c => c.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            var mockSet = new Mock<DbSet<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>>();
            mockDbContext.Setup(c => c.Set<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>())
                .Returns(mockSet.Object);

            // Act
            await _repository.RestoreManyByIdAsync(secretIds);

            // Assert
            _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
            mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HardDeleteManyByIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var secretIds = new List<Guid> { Guid.NewGuid() };
            
            var mockScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory
                .Setup(f => f.CreateAsyncScope())
                .ReturnsAsync(mockScope.Object);

            var mockDbContext = new Mock<DbContext>();
            mockScope
                .Setup(s => s.ServiceProvider.GetRequiredService<DbContext>())
                .Returns(mockDbContext.Object);

            var mockTransaction = new Mock<IDbContextTransaction>();
            mockDbContext
                .Setup(c => c.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockTransaction.Object);

            var mockSet = new Mock<DbSet<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>>();
            mockDbContext.Setup(c => c.Set<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>())
                .Returns(mockSet.Object);

            // Act
            await _repository.HardDeleteManyByIdAsync(secretIds);

            // Assert
            _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
            mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetManyByOrganizationIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var orgId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var mockScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory
                .Setup(f => f.CreateAsyncScope())
                .ReturnsAsync(mockScope.Object);

            var mockDbContext = new Mock<DbContext>();
            mockScope
                .Setup(s => s.ServiceProvider.GetRequiredService<DbContext>())
                .Returns(mockDbContext.Object);

            var mockSet = new Mock<DbSet<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>>();
            mockDbContext.Setup(c => c.Set<Bit.Infrastructure.EntityFramework.SecretsManager.Models.Secret>())
                .Returns(mockSet.Object);

            // Act
            await _repository.GetManyByOrganizationIdAsync(orgId, userId, AccessClientType.User);

            // Assert
            _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
        }
    }
}
