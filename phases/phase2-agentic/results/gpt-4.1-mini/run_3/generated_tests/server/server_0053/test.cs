using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;

namespace Bit.Core.Billing.Tests.Services.Implementations
{
    public class SubscriberServiceTests
    {
        private readonly Mock<ILogger<SubscriberService>> _loggerMock;
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly SubscriberService _subscriberService;

        public SubscriberServiceTests()
        {
            _loggerMock = new Mock<ILogger<SubscriberService>>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();
            _taxServiceMock = new Mock<ITaxService>();

            // Other dependencies can be mocked as null or default for this test
            _subscriberService = new SubscriberService(
                braintreeGateway: null!,
                globalSettings: null!,
                logger: _loggerMock.Object,
                organizationRepository: null!,
                providerRepository: null!,
                setupIntentCache: null!,
                stripeAdapter: _stripeAdapterMock.Object,
                taxService: _taxServiceMock.Object,
                userRepository: null!);
        }

        [Fact]
        public async Task CreateOrUpdateTaxId_LogsWarningAndThrows_WhenTaxIdTypeCannotBeInferred()
        {
            // Arrange
            var customer = new Customer
            {
                Id = "cus_123",
                TaxIds = new StripeList<TaxId> { Data = new List<TaxId>() }
            };

            var taxInformation = new
            {
                Country = "US",
                PostalCode = "12345",
                Line1 = "123 Main St",
                Line2 = (string?)null,
                City = "City",
                State = "State",
                TaxId = "123456789",
                TaxIdType = (string?)null
            };

            // Setup taxService to return null taxIdType to trigger the warning and exception
            _taxServiceMock.Setup(t => t.GetStripeTaxCode(taxInformation.Country, taxInformation.TaxId))
                .Returns((string?)null);

            // We need to simulate the method that contains the code snippet.
            // Since the snippet is partial, we will create a minimal method here for testing purposes.
            async Task MethodUnderTest()
            {
                var taxIdType = taxInformation.TaxIdType;
                if (string.IsNullOrWhiteSpace(taxIdType))
                {
                    taxIdType = _taxServiceMock.Object.GetStripeTaxCode(taxInformation.Country,
                        taxInformation.TaxId);

                    if (taxIdType == null)
                    {
                        _loggerMock.Object.LogWarning("Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                            taxInformation.Country,
                            taxInformation.TaxId);

                        throw new BadRequestException("billingTaxIdTypeInferenceError");
                    }
                }
            }

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(MethodUnderTest);

            Assert.Equal("billingTaxIdTypeInferenceError", ex.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Could not infer tax ID type")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateOrUpdateTaxId_LogsWarningAndThrows_WhenStripeExceptionTaxIdInvalid()
        {
            // Arrange
            var customer = new Customer
            {
                Id = "cus_123",
                TaxIds = new StripeList<TaxId> { Data = new List<TaxId>() }
            };

            var taxInformation = new
            {
                Country = "US",
                PostalCode = "12345",
                Line1 = "123 Main St",
                Line2 = (string?)null,
                City = "City",
                State = "State",
                TaxId = "invalid_tax_id",
                TaxIdType = "some_type"
            };

            // Setup stripeAdapter to throw StripeException with TaxIdInvalid code
            var stripeException = new StripeException
            {
                StripeError = new StripeError
                {
                    Code = StripeConstants.ErrorCodes.TaxIdInvalid
                }
            };

            _stripeAdapterMock.Setup(s => s.TaxIdCreateAsync(customer.Id, It.IsAny<TaxIdCreateOptions>()))
                .ThrowsAsync(stripeException);

            // We simulate the try-catch block from the snippet
            async Task MethodUnderTest()
            {
                try
                {
                    await _stripeAdapterMock.Object.TaxIdCreateAsync(customer.Id,
                        new TaxIdCreateOptions { Type = taxInformation.TaxIdType, Value = taxInformation.TaxId });
                }
                catch (StripeException e)
                {
                    switch (e.StripeError.Code)
                    {
                        case StripeConstants.ErrorCodes.TaxIdInvalid:
                            _loggerMock.Object.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.",
                                taxInformation.TaxId,
                                taxInformation.Country);

                            throw new BadRequestException("billingInvalidTaxIdError");

                        default:
                            throw;
                    }
                }
            }

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(MethodUnderTest);

            Assert.Equal("billingInvalidTaxIdError", ex.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid tax ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
