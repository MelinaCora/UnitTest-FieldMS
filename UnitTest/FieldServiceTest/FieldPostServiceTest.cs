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



            var service = new FieldPostServices(mockFieldCommand.Object,mockFieldQuery.Object,mockFieldTypeQuery.Object,mockAvailabilityQuery.Object,mockValidator.Object,mockMapper.Object);

            //ACT


            //ASSERTS

        }
    }
}
