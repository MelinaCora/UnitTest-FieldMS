using Application.DTOS.Request;
using Application.DTOS.Responses;
using Application.Exceptions;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices.IAvailabilityServices;
using Application.Interfaces.IValidator;
using Application.Interfaces.ICommand;
using Application.Services.FieldServices;
using AutoMapper;
using Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTest.FieldServiceTest
{
    public class FieldPutServiceTest
    {
        private readonly Mock<IFieldCommand> _mockFieldCommand;
        private readonly Mock<IFieldQuery> _mockFieldQuery;
        private readonly Mock<IFieldTypeQuery> _mockFieldTypeQuery;
        private readonly Mock<IAvailabilityPostServices> _mockAvailabilityPostServices;
        private readonly Mock<IAvailabilityGetServices> _mockAvailabilityGetServices;
        private readonly Mock<IAvailabilityPutServices> _mockAvailabilityPutServices;
        private readonly Mock<IAvailabilityDeleteService> _mockAvailabilityDeleteService;
        private readonly Mock<IValidatorHandler<FieldRequest>> _mockFieldValidator;
        private readonly Mock<IMapper> _mockMapper;
        private readonly FieldPutServices _fieldPutServices;

        public FieldPutServiceTest()
        {
            // Inicialización de mocks
            _mockFieldCommand = new Mock<IFieldCommand>();
            _mockFieldQuery = new Mock<IFieldQuery>();
            _mockFieldTypeQuery = new Mock<IFieldTypeQuery>();
            _mockAvailabilityPostServices = new Mock<IAvailabilityPostServices>();
            _mockAvailabilityGetServices = new Mock<IAvailabilityGetServices>();
            _mockAvailabilityPutServices = new Mock<IAvailabilityPutServices>();
            _mockAvailabilityDeleteService = new Mock<IAvailabilityDeleteService>();
            _mockFieldValidator = new Mock<IValidatorHandler<FieldRequest>>();
            _mockMapper = new Mock<IMapper>();

            // Inicialización del servicio a probar
            _fieldPutServices = new FieldPutServices(
                _mockFieldCommand.Object,
                _mockFieldQuery.Object,
                _mockFieldTypeQuery.Object,
                _mockAvailabilityPostServices.Object,
                _mockAvailabilityGetServices.Object,
                _mockAvailabilityPutServices.Object,
                _mockAvailabilityDeleteService.Object,
                _mockFieldValidator.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task CreateAvailability_ShouldReturnSuccess()
        {
            // Arrange
            var fieldId = Guid.NewGuid();
            var request = new AvailabilityRequest
            {
                Day = "Monday",
                OpenHour = new TimeSpan(8, 0, 0),
                CloseHour = new TimeSpan(10, 0, 0),
            };

            var field = new Field
            {
                Availabilities = new List<Availability>()
            };

            _mockFieldQuery.Setup(f => f.GetFieldById(fieldId)).ReturnsAsync(field);
            _mockAvailabilityPostServices.Setup(a => a.CreateAvailability(fieldId, request)).ReturnsAsync(new AvailabilityResponse());

            // Act
            var result = await _fieldPutServices.CreateAvailability(fieldId, request);

            // Assert
            Assert.NotNull(result);
            _mockAvailabilityPostServices.Verify(a => a.CreateAvailability(fieldId, request), Times.Once);
        }

        [Fact]
        public async Task CreateAvailability_ShouldThrowInvalidOperationException_WhenAvailabilityIsOverlapping()
        {
            // Arrange
            var fieldId = Guid.NewGuid();
            var request = new AvailabilityRequest
            {
                Day = "Monday",
                OpenHour = new TimeSpan(9, 0, 0),
                CloseHour = new TimeSpan(11, 0, 0)
            };

            var field = new Field
            {
                Availabilities = new List<Availability>
                {
                    new Availability { DayName = "Monday", OpenHour = new TimeSpan(8, 0, 0), CloseHour = new TimeSpan(10, 0, 0) }
                }
            };

            _mockFieldQuery.Setup(f => f.GetFieldById(fieldId)).ReturnsAsync(field);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _fieldPutServices.CreateAvailability(fieldId, request));
        }

        [Fact]
        public async Task UpdateAvailability_ShouldReturnSuccess()
        {
            // Arrange
            var availabilityId = 1;
            var request = new AvailabilityRequest
            {
                Day = "Monday",
                OpenHour = new TimeSpan(8, 0, 0),
                CloseHour = new TimeSpan(10, 0, 0)
            };

            var existingAvailability = new Availability
            {
                AvailabilityID = availabilityId,
                DayName = "Monday",
                OpenHour = new TimeSpan(7, 0, 0),
                CloseHour = new TimeSpan(9, 0, 0)
            };

            var field = new Field
            {
                Availabilities = new List<Availability> { existingAvailability }
            };

            _mockAvailabilityGetServices.Setup(a => a.GetAvailabilityById(availabilityId)).ReturnsAsync(existingAvailability);
            _mockFieldQuery.Setup(f => f.GetFieldById(existingAvailability.FieldID)).ReturnsAsync(field);
            _mockAvailabilityPutServices.Setup(a => a.UpdateAvailability(availabilityId, request)).ReturnsAsync(new AvailabilityResponse());

            // Act
            var result = await _fieldPutServices.UpdateAvailability(availabilityId, request);

            // Assert
            Assert.NotNull(result);
            _mockAvailabilityPutServices.Verify(a => a.UpdateAvailability(availabilityId, request), Times.Once);
        }

        [Fact]
        public async Task UpdateField_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new FieldRequest { Name = "" };
            _mockFieldValidator.Setup(v => v.Validate(request)).Throws(new InvalidOperationException("Validation failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _fieldPutServices.UpdateField(id, request));
        }

        [Fact]
        public async Task UpdateField_ShouldReturnFieldResponse_WhenFieldIsUpdated()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new FieldRequest { Name = "New Field Name", FieldType = 1 };
            var existingField = new Field { Name = "Old Field Name", FieldTypeNavigator = new FieldType() };
            var fieldType = new FieldType();

            _mockFieldQuery.Setup(q => q.GetFieldById(id)).ReturnsAsync(existingField);
            _mockFieldTypeQuery.Setup(q => q.GetFieldTypeById(request.FieldType)).ReturnsAsync(fieldType);
            _mockMapper.Setup(m => m.Map(request, existingField)).Verifiable();
            _mockMapper.Setup(m => m.Map<FieldResponse>(existingField)).Returns(new FieldResponse());

            // Act
            var result = await _fieldPutServices.UpdateField(id, request);

            // Assert
            Assert.NotNull(result);
            _mockMapper.Verify();
            _mockFieldCommand.Verify(c => c.UpdateField(existingField), Times.Once);
        }
    }
}