using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Automapper;
using Application.Validators;
using Moq;
using AutoMapper;
using Application.Interfaces.IValidator;
using Application.Services.FieldServices;
using Application.DTOS.Request;
using Domain.Entities;
using Application.DTOS.Responses;
using Application.Exceptions;
using FluentValidation;


namespace UnitTest.FieldServiceTest
{
    public class FieldPostServiceTest
    {
        [Fact]
        public async Task CreateField_Should_Return_CreatedField()
        {
            //ARRANGE
            var mockFieldQuery = new Mock<IFieldQuery>();
            var mockFieldTypeQuery = new Mock<IFieldTypeQuery>();
            var mockAvailabilityQuery = new Mock<IAvailabilityQuery>();
            var mockFieldCommand = new Mock<IFieldCommand>();
            var mockMapper = new Mock<IMapper>();
            var mockValidator = new Mock<IValidatorHandler<FieldRequest>>();

            var request = new FieldRequest
            {
                Name = "Test Field",
                Size = "Large",
                FieldType = 1
            };

            var fieldType = new FieldType
            {
                FieldTypeID = request.FieldType,
                Description = "Soccer Field"
            };

            var newField = new Field
            {
                FieldID = Guid.NewGuid(),
                Name = request.Name,
                Size = request.Size,
                IsActive = true,
                FieldTypeNavigator = fieldType
            };

            var response = new FieldResponse
            {
                Id = newField.FieldID,
                Name = newField.Name,
                Size = newField.Size,
                FieldType = new FieldTypeResponse
                {
                    Id = fieldType.FieldTypeID,
                    Description=fieldType.Description

                }
            };

            mockValidator
               .Setup(v => v.Validate(It.IsAny<FieldRequest>()))
               .Returns(Task.CompletedTask);

            mockFieldTypeQuery
                .Setup(q => q.GetFieldTypeById(request.FieldType))
                .ReturnsAsync(fieldType);

            mockMapper
                .Setup(m => m.Map<Field>(request))
                .Returns(newField);

            mockMapper
                .Setup(m => m.Map<FieldResponse>(newField))
                .Returns(response);

            mockFieldCommand
                .Setup(c => c.InsertField(It.IsAny<Field>()))
                .Returns(Task.CompletedTask);

            var service = new FieldPostServices(mockFieldCommand.Object,mockFieldQuery.Object,mockFieldTypeQuery.Object,mockAvailabilityQuery.Object,mockValidator.Object,mockMapper.Object);

            //ACT
            var result = await service.CreateField(request);

            //ASSERTS
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Size, result.Size);
            Assert.Equal(fieldType.Description, result.FieldType.Description);

            mockValidator.Verify(v => v.Validate(It.IsAny<FieldRequest>()), Times.Once);
            mockFieldTypeQuery.Verify(q => q.GetFieldTypeById(request.FieldType), Times.Once);
            mockMapper.Verify(m => m.Map<Field>(request), Times.Once);
            mockMapper.Verify(m => m.Map<FieldResponse>(newField), Times.Once);
            mockFieldCommand.Verify(c => c.InsertField(It.IsAny<Field>()), Times.Once);
        }

        [Fact]
        public async Task CreateField_Should_Throw_NotFoundException_When_FieldType_NotFound()
        {
            // ARRANGE
            var mockFieldTypeQuery = new Mock<IFieldTypeQuery>();
            var mockValidator = new Mock<IValidatorHandler<FieldRequest>>();
            var mockMapper = new Mock<IMapper>();
            var mockFieldCommand = new Mock<IFieldCommand>();

            var request = new FieldRequest
            {
                Name = "Test Field",
                Size = "Large",
                FieldType = 999
            };

            mockValidator.Setup(v => v.Validate(It.IsAny<FieldRequest>()))
                .Returns(Task.CompletedTask);

            mockFieldTypeQuery.Setup(q => q.GetFieldTypeById(request.FieldType))
                .ReturnsAsync((FieldType)null);

            var service = new FieldPostServices(mockFieldCommand.Object, null, mockFieldTypeQuery.Object, null, mockValidator.Object, mockMapper.Object);

            // ACT & ASSERT
            await Assert.ThrowsAsync<NotFoundException>(() => service.CreateField(request));

            mockFieldTypeQuery.Verify(q => q.GetFieldTypeById(request.FieldType), Times.Once);
            mockFieldCommand.Verify(c => c.InsertField(It.IsAny<Field>()), Times.Never);
        }

        [Fact]
        public async Task CreateField_Should_Throw_ValidationException_When_Name_Is_Null()
        {
            // ARRANGE
            var mockValidator = new Mock<IValidatorHandler<FieldRequest>>();
            var request = new FieldRequest
            {
                Name = null,
                Size = "5",
                FieldType = 1
            };

            mockValidator
                .Setup(v => v.Validate(It.IsAny<FieldRequest>()))
                .Throws(new ValidationException("The name is required."));

            var service = new FieldPostServices(
                Mock.Of<IFieldCommand>(),
                Mock.Of<IFieldQuery>(),
                Mock.Of<IFieldTypeQuery>(),
                Mock.Of<IAvailabilityQuery>(),
                mockValidator.Object,
                Mock.Of<IMapper>());

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<ValidationException>(() => service.CreateField(request));
            Assert.Equal("The name is required.", exception.Message);
            mockValidator.Verify(v => v.Validate(It.IsAny<FieldRequest>()), Times.Once);
        }

        [Fact]
        public async Task CreateField_Should_Throw_ValidationException_When_Size_Is_Null()
        {
            // ARRANGE
            var mockValidator = new Mock<IValidatorHandler<FieldRequest>>();
            var request = new FieldRequest
            {
                Name = "Test Field",
                Size = null,
                FieldType = 1
            };

            mockValidator
                .Setup(v => v.Validate(It.IsAny<FieldRequest>()))
                .Throws(new ValidationException("The size is required."));

            var service = new FieldPostServices(
                Mock.Of<IFieldCommand>(),
                Mock.Of<IFieldQuery>(),
                Mock.Of<IFieldTypeQuery>(),
                Mock.Of<IAvailabilityQuery>(),
                mockValidator.Object,
                Mock.Of<IMapper>());

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<ValidationException>(() => service.CreateField(request));
            Assert.Equal("The size is required.", exception.Message);
            mockValidator.Verify(v => v.Validate(It.IsAny<FieldRequest>()), Times.Once);
        }

        [Fact]
        public async Task CreateField_Should_Throw_ValidationException_When_Size_Is_Invalid()
        {
            // ARRANGE
            var mockValidator = new Mock<IValidatorHandler<FieldRequest>>();
            var request = new FieldRequest
            {
                Name = "Test Field",
                Size = "20",
                FieldType = 1
            };

            mockValidator
                .Setup(v => v.Validate(It.IsAny<FieldRequest>()))
                .Throws(new ValidationException("The size must be either 5, 7, or 11."));

            var service = new FieldPostServices(
                Mock.Of<IFieldCommand>(),
                Mock.Of<IFieldQuery>(),
                Mock.Of<IFieldTypeQuery>(),
                Mock.Of<IAvailabilityQuery>(),
                mockValidator.Object,
                Mock.Of<IMapper>());

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<ValidationException>(() => service.CreateField(request));
            Assert.Equal("The size must be either 5, 7, or 11.", exception.Message);
            mockValidator.Verify(v => v.Validate(It.IsAny<FieldRequest>()), Times.Once);
        }
    }
}
