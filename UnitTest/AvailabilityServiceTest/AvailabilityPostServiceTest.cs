using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.ICommand;
using Application.Interfaces.IValidator;
using Application.Automapper;
using Application.DTOS.Request;
using Application.DTOS.Responses;
using Application.Services.AvailabilityServices;
using AutoMapper;
using Domain.Entities;
using Moq;

namespace UnitTest.AvailabilityServiceTest
{
    public class AvailabilityPostServiceTest
    {
        [Fact]
        public async Task CreateAvailability_Should_Return_Availability_When_Create() {

            //ARRANGE
            var mockMapper = new Mock<IMapper>();
            var mockCommand = new Mock<IAvailabilityCommand>();
            var mockValidator = new Mock<IValidatorHandler<AvailabilityRequest>>();

            var fieldId = Guid.NewGuid();

            var request = new AvailabilityRequest
            {
                Day = "Monday",
                OpenHour = new TimeSpan(8, 0, 0),
                CloseHour = new TimeSpan(18, 0, 0)
            };

            var availability = new Availability
            {
                AvailabilityID = 1,
                FieldID = fieldId,
                DayName = request.Day,
                OpenHour = request.OpenHour,
                CloseHour = request.CloseHour
            };

            var expectedResponse = new AvailabilityResponse
            {
                Id = availability.AvailabilityID,
                Day = availability.DayName,
                OpenHour = availability.OpenHour,
                CloseHour = availability.CloseHour
            };

            mockValidator
                .Setup(v => v.Validate(request))
                .Returns(Task.CompletedTask);

            mockMapper
                .Setup(m => m.Map<Availability>(request))
                .Returns(availability);

            mockMapper
                .Setup(m => m.Map<AvailabilityResponse>(availability))
                .Returns(expectedResponse);

            mockCommand
                .Setup(c => c.InsertAvailability(availability))
                .Returns(Task.CompletedTask);

            var service = new AvailabilityPostServices(mockMapper.Object, mockCommand.Object, mockValidator.Object);

            //ACT
            var result = await service.CreateAvailability(fieldId, request);

            //ASSERT
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Id, result.Id);
            Assert.Equal(expectedResponse.Day, result.Day);
            Assert.Equal(expectedResponse.OpenHour, result.OpenHour);
            Assert.Equal(expectedResponse.CloseHour, result.CloseHour);

            mockValidator.Verify(v => v.Validate(request), Times.Once);
            mockMapper.Verify(m => m.Map<Availability>(request), Times.Once);
            mockMapper.Verify(m => m.Map<AvailabilityResponse>(availability), Times.Once);
            mockCommand.Verify(c => c.InsertAvailability(availability), Times.Once);
        }
    }
}
