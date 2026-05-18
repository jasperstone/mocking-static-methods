using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Infrastructure.EntityFramework.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.Enums;
using Bit.Core.SecretsManager.Models.Data;
using Bit.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SecretRepositoryTests
{
    public class SecretRepositoryTest
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<DbSet<Secret>> _dbSetMock;
        private readonly IMapper _mapper;

        public SecretRepositoryTest()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _dbContextMock = new Mock<DbContext>();
            _dbSetMock = new Mock<DbSet<Secret>>();

            _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
            _serviceScopeMock.Setup(s => s.Dispose());
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(Mock.Of<IServiceProvider>());

            var options = new DbContextOptionsBuilder<DbContext>().Options;
            _dbContextMock.As<IServiceProvider>().Setup(sp => sp.GetService(typeof(DbContext))).Returns(new DbContext(options));
        }

        [Fact]
        public async Task GetByIdAsync_Should_Call_CreateScope_And_Return_Mapped_Secret()
        {
            // Arrange
            var secretId = Guid.NewGuid();
            var secretEntity = new Secret { Id = secretId, DeletedDate = null };
            var secrets = new List<Secret> { secretEntity }.AsQueryable();

            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Provider).Returns(secrets.Provider);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.Expression).Returns(secrets.Expression);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.ElementType).Returns(secrets.ElementType);
            _dbSetMock.As<IQueryable<Secret>>().Setup(m => m.GetEnumerator()).Returns(secrets.GetEnumerator());

            _dbContextMock.Setup(c => c.Secret).Returns(_dbSetMock.Object);
            _dbContextMock.Setup(c => c.Set<Secret>()).Returns(_dbSetMock.Object);
            _dbContextMock.Setup(c => c.Secret).Returns(_dbSetMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Core.SecretsManager.Entities.Secret>(It.IsAny<Secret>()))
                      .Returns(new Core.SecretsManager.Entities.Secret { Id = secretId });

            var repo = new SecretRepository(_serviceScopeFactoryMock.Object, mapperMock.Object);

            // Act
            var result = await repo.GetByIdAsync(secretId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(secretId, result.Id);
            _serviceScopeFactoryMock.Verify(f => f.CreateScope(), Times.Once);
        }
    }
}
