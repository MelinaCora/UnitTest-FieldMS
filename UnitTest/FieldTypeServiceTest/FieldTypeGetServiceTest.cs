using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.ICommand;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using Application.Interfaces.IValidator;
using Application.Validators;
using Application.Automapper;
using Application.Interfaces.IServices.IFieldTypeServices;
using AutoMapper;
using Application.Services.FieldTypeServices;
using Moq;
using Microsoft.VisualBasic;
using Application.DTOS.Responses;
using Domain.Entities;
using Microsoft.VisualBasic.FileIO;
using FieldType = Domain.Entities.FieldType;

namespace UnitTest.FieldTypeServiceTest
{
    public class FieldTypeGetServiceTest
    {
        [Fact]
        public async Task GetAll_ShouldReturnFieldTypeResponseList_WhenFieldTypesExist() 
        {
            //ARRANGE
            var mockFieldTypeQuery = new Mock<IFieldTypeQuery>();
            var mockMapper = new Mock<IMapper>();

            var fieldTypes = new List<FieldType>
            {
                new FieldType { FieldTypeID = 1, Description = "Type1" },
                new FieldType { FieldTypeID = 2, Description = "Type2" }
            };

            var fieldTypeResponses = new List<FieldTypeResponse>
            {
                new FieldTypeResponse { Id = 1, Description = "Type1" },
                new FieldTypeResponse { Id = 2, Description = "Type2" }
            };

            mockFieldTypeQuery
                .Setup(q => q.GetListFieldTypes())
                .ReturnsAsync(fieldTypes);

            mockMapper
                .Setup(m => m.Map<List<FieldTypeResponse>>(fieldTypes))
                .Returns(fieldTypeResponses);

            var service = new FieldTypeGetServices(mockFieldTypeQuery.Object,mockMapper.Object);

            //ACT
            var result = await service.GetAll();

            //ASSERTS
            Assert.NotNull(result);
            Assert.IsType<List<FieldTypeResponse>>(result);
            Assert.Equal(fieldTypeResponses.Count, result.Count);
        }

        [Fact]
        public async Task GetFieldTypeById_ShouldReturnFieldTypeResponse_WhenFieldTypeExists()
        {
            //ARRANGE
            var mockFieldTypeQuery = new Mock<IFieldTypeQuery>();
            var mockMapper = new Mock<IMapper>();

            var fieldTypeID = 1;

            var FieldType = new FieldType
            {
                FieldTypeID = fieldTypeID,
                Description = "Type 1"
            };

            var expectedResponse = new FieldTypeResponse
            {
                Id = fieldTypeID,
                Description = "Type 1"
            };

            mockFieldTypeQuery
                .Setup(f => f.GetFieldTypeById(fieldTypeID))
                .ReturnsAsync(FieldType);

            mockMapper
                .Setup(m => m.Map<FieldTypeResponse>(FieldType))
                .Returns(expectedResponse);

            var service = new FieldTypeGetServices(mockFieldTypeQuery.Object, mockMapper.Object);

            //ACT
            var result = await service.GetFieldTypeById(fieldTypeID);

            //ASSERTS
            Assert.NotNull(result);
            Assert.IsType<FieldTypeResponse>(result);
            Assert.Equal(fieldTypeID, result.Id);
        }
    }
}
