using System;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.Repositories;
using Bit.Core.Utilities;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Infrastructure.EntityFramework.Repositories;
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
        public async Task SetUserSignatureKeyPair_ShouldAddNewEntity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "RSA",
                WrappedSigningKey = "wrappedSigningKey",
                VerifyingKey = "verifyingKey"
            };

            var mockScope = new Mock<IServiceScope>();
            var mockDbContext = new Mock<DatabaseContext>();
            var mockDbSet = new Mock<DbSet<Models.UserSignatureKeyPair>>();

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
            mockScope.Setup(x => x.ServiceProvider.GetService(typeof(DatabaseContext))).Returns(mockDbContext.Object);
            mockDbContext.Setup(x => x.UserSignatureKeyPairs).Returns(mockDbSet.Object);

            // Act
            var updateAction = _repository.SetUserSignatureKeyPair(userId, signingKeys);
            await updateAction(null, null);

            // Assert
            mockDbSet.Verify(x => x.AddAsync(It.IsAny<Models.UserSignatureKeyPair>(), default), Times.Once);
            mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateForKeyRotation_ShouldUpdateExistingEntity()
        {
            // Arrange
            var grantorId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "RSA",
                WrappedSigningKey = "wrappedSigningKey",
                VerifyingKey = "verifyingKey"
            };

            var mockScope = new Mock<IServiceScope>();
            var mockDbContext = new Mock<DatabaseContext>();
            var mockDbSet = new Mock<DbSet<Models.UserSignatureKeyPair>>();
            var existingEntity = new Models.UserSignatureKeyPair { UserId = grantorId };

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
            mockScope.Setup(x => x.ServiceProvider.GetService(typeof(DatabaseContext))).Returns(mockDbContext.Object);
            mockDbContext.Setup(x => x.UserSignatureKeyPairs).Returns(mockDbSet.Object);
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Func<Models.UserSignatureKeyPair, bool>>(), default)).ReturnsAsync(existingEntity);

            // Act
            var updateAction = _repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateAction(null, null);

            // Assert
            mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
            Assert.Equal(signingKeys.SignatureAlgorithm, existingEntity.SignatureAlgorithm);
            Assert.Equal(signingKeys.WrappedSigningKey, existingEntity.SigningKey);
            Assert.Equal(signingKeys.VerifyingKey, existingEntity.VerifyingKey);
        }
    }
}
