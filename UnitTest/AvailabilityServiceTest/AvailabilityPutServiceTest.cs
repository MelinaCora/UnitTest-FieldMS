using System;
using System.Threading.Tasks;
using Application.DTOS.Request;
using Application.DTOS.Responses;
using Application.Exceptions;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IValidator;
using Application.Services.AvailabilityServices;
using AutoMapper;
using Domain.Entities;
using Moq;
using Xunit;

namespace UnitTest.AvailabilityServiceTest
{
    public class AvailabilityPutServiceTest
    {
        [Fact]
        public async Task UpdateAvailability_Should_Return_Modified_Availability_When_Exist()
        {
            //ARRANGE
            var mockQuery = new Mock<IAvailabilityQuery>();
            var mockCommand = new Mock<IAvailabilityCommand>();
            var mockValidator = new Mock<IValidatorHandler<AvailabilityRequest>>();
            var mockMapper = new Mock<IMapper>();

            int availabilityId = 1;

            var request = new AvailabilityRequest
            {
                Day = "Monday",
                OpenHour = TimeSpan.FromHours(8),
                CloseHour = TimeSpan.FromHours(18)
            };

            var existingAvailability = new Availability
            {
                AvailabilityID = availabilityId,
                FieldID = Guid.NewGuid(),
                DayName = "Sunday",
                OpenHour = TimeSpan.FromHours(9),
                CloseHour = TimeSpan.FromHours(17)
            };

            var updatedAvailability = new Availability
            {
                AvailabilityID = existingAvailability.AvailabilityID,
                FieldID = existingAvailability.FieldID,
                DayName = request.Day,
                OpenHour = request.OpenHour,
                CloseHour = request.CloseHour
            };

            var expectedResponse = new AvailabilityResponse
            {
                Id = updatedAvailability.AvailabilityID,
                Day = request.Day,
                OpenHour = request.OpenHour,
                CloseHour = request.CloseHour
            };

            mockValidator
                .Setup(v => v.Validate(It.IsAny<AvailabilityRequest>()))
                .Returns(Task.CompletedTask);

            mockQuery
                .Setup(q => q.GetAvailabilityByID(availabilityId))
                .ReturnsAsync(existingAvailability);

            mockMapper
               .Setup(m => m.Map(It.IsAny<AvailabilityRequest>(), It.IsAny<Availability>()))
               .Returns((AvailabilityRequest req, Availability entity) =>
               {
                   entity.DayName = req.Day;
                   entity.OpenHour = req.OpenHour;
                   entity.CloseHour = req.CloseHour;
                   return entity;
               });

            mockCommand
                .Setup(c => c.UpdateAvailability(It.IsAny<Availability>()))
                .Returns(Task.CompletedTask);           

            mockMapper
                .Setup(m => m.Map<AvailabilityResponse>(It.IsAny<Availability>()))
                .Returns(expectedResponse);

            var service = new AvailabilityPutServices(mockMapper.Object, mockCommand.Object, mockQuery.Object, mockValidator.Object);

            //ACT

            var result = await service.UpdateAvailability(availabilityId, request);

            //ASSERT

            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Equal(expectedResponse.Day, result.Day);
            Assert.Equal(expectedResponse.OpenHour, result.OpenHour);
            Assert.Equal(expectedResponse.CloseHour, result.CloseHour);

            mockValidator.Verify(v => v.Validate(request), Times.Once, "El validador debe llamarse exactamente una vez.");
            mockQuery.Verify(q => q.GetAvailabilityByID(availabilityId), Times.Once, "Debe buscarse la disponibilidad por ID exactamente una vez.");
            mockCommand.Verify(c => c.UpdateAvailability(It.IsAny<Availability>()), Times.Once, "Debe actualizarse la disponibilidad exactamente una vez.");
        }

        [Fact]
        public async Task UpdateAvailability_Should_Throw_Exception_When_Validation_Fails()
        {
            // Arrange
            var mockQuery = new Mock<IAvailabilityQuery>();
            var mockCommand = new Mock<IAvailabilityCommand>();
            var mockValidator = new Mock<IValidatorHandler<AvailabilityRequest>>();
            var mockMapper = new Mock<IMapper>();

            int validId = 1;

            var request = new AvailabilityRequest
            {
                Day = "InvalidDay",
                OpenHour = TimeSpan.FromHours(8),
                CloseHour = TimeSpan.FromHours(18)
            };

            mockValidator
                .Setup(v => v.Validate(request))
                .ThrowsAsync(new ArgumentException("Validation failed"));

            var service = new AvailabilityPutServices(mockMapper.Object, mockCommand.Object, mockQuery.Object, mockValidator.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAvailability(validId, request));
            mockValidator.Verify(v => v.Validate(request), Times.Once);
        }
    }
}
