using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using static Infrastructure.EntityFramework.KeyManagement.Repositories.UserSignatureKeyPairRepository;

namespace Infrastructure.EntityFramework.KeyManagement.Tests.Repositories
{
    public class UserSignatureKeyPairRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
        private readonly Mock<IServiceScope> _scopeMock = new();
        private readonly Mock<IServiceProvider> _serviceProviderMock = new();
        private readonly Mock<UserSignatureKeyPairDbContext> _dbContextMock;

        public UserSignatureKeyPairRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<UserSignatureKeyPairDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContextMock = new Mock<UserSignatureKeyPairDbContext>(options);

            _scopeFactoryMock
                .Setup(factory => factory.CreateAsyncScope())
                .Returns(new AsyncServiceScope(_scopeMock.Object));

            _scopeMock
                .Setup(scope => scope.ServiceProvider)
                .Returns(_serviceProviderMock.Object);

            _serviceProviderMock
                .Setup(provider => provider.GetService(typeof(UserSignatureKeyPairDbContext)))
                .Returns(_dbContextMock.Object);
        }

        [Fact]
        public async Task UpdateForKeyRotation_UsesAsyncScope_FromScopeFactory()
        {
            var repository = new UserSignatureKeyPairRepository(_scopeFactoryMock.Object);
            var updateFunc = repository.UpdateForKeyRotation(Guid.NewGuid(), new SignatureKeyPairData());

            await updateFunc(default, default);

            _scopeFactoryMock.Verify(factory => factory.CreateAsyncScope(), Times.Once);
        }

        [Fact]
        public async Task SetUserSignatureKeyPair_UsesAsyncScope_FromScopeFactory()
        {
            var repository = new UserSignatureKeyPairRepository(_scopeFactoryMock.Object);
            var setter = repository.SetUserSignatureKeyPair(Guid.NewGuid(), new SignatureKeyPairData());

            await setter(default, default);

            _scopeFactoryMock.Verify(factory => factory.CreateAsyncScope(), Times.Once);
        }

        [Fact]
        public async Task UpdateForKeyRotation_SavesChanges_WhenEntityExists()
        {
            var userId = Guid.NewGuid();
            var existingEntity = new Models.UserSignatureKeyPair { UserId = userId };
            var dbSetMock = DbSetMockFactory.CreateMock(existingEntity);

            _dbContextMock.Setup(context => context.UserSignatureKeyPairs).Returns(dbSetMock.Object);
            _dbContextMock
                .Setup(context => context.SaveChangesAsync(default))
                .ReturnsAsync(1)
                .Verifiable();

            var repository = new UserSignatureKeyPairRepository(_scopeFactoryMock.Object);
            var signatureData = new SignatureKeyPairData
            {
                SignatureAlgorithm = "algo",
                WrappedSigningKey = Array.Empty<byte>(),
                VerifyingKey = Array.Empty<byte>()
            };

            var updateFunc = repository.UpdateForKeyRotation(userId, signatureData);
            await updateFunc(default, default);

            _dbContextMock.Verify(context => context.SaveChangesAsync(default), Times.Once);
        }
    }
}
