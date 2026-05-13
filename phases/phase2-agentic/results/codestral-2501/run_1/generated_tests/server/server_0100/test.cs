using System;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.Repositories;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.KeyManagement.Repositories.Tests
{
    public class UserSignatureKeyPairRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly UserSignatureKeyPairRepository _repository;

        public UserSignatureKeyPairRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _repository = new UserSignatureKeyPairRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task SetUserSignatureKeyPair_ShouldCreateScopeAndSaveChanges()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "RSA",
                WrappedSigningKey = "signingKey",
                VerifyingKey = "verifyingKey"
            };

            var serviceScopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<DatabaseContext>();
            var dbSetMock = new Mock<DbSet<Models.UserSignatureKeyPair>>();

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(DatabaseContext))).Returns(dbContextMock.Object);
            dbContextMock.Setup(x => x.UserSignatureKeyPairs).Returns(dbSetMock.Object);

            // Act
            var updateAction = _repository.SetUserSignatureKeyPair(userId, signingKeys);
            await updateAction(null, null);

            // Assert
            _serviceScopeFactoryMock.Verify(x => x.CreateAsyncScope(), Times.Once);
            dbSetMock.Verify(x => x.AddAsync(It.IsAny<Models.UserSignatureKeyPair>(), default), Times.Once);
            dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateForKeyRotation_ShouldCreateScopeAndSaveChanges()
        {
            // Arrange
            var grantorId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "RSA",
                WrappedSigningKey = "signingKey",
                VerifyingKey = "verifyingKey"
            };

            var serviceScopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<DatabaseContext>();
            var dbSetMock = new Mock<DbSet<Models.UserSignatureKeyPair>>();
            var entity = new Models.UserSignatureKeyPair { UserId = grantorId };

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(DatabaseContext))).Returns(dbContextMock.Object);
            dbContextMock.Setup(x => x.UserSignatureKeyPairs).Returns(dbSetMock.Object);
            dbSetMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Func<Models.UserSignatureKeyPair, bool>>(), default)).ReturnsAsync(entity);

            // Act
            var updateAction = _repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateAction(null, null);

            // Assert
            _serviceScopeFactoryMock.Verify(x => x.CreateAsyncScope(), Times.Once);
            dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }
    }
}
