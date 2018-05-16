using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tokenaire.Controllers.Models;
using Tokenaire.Service;
using Tokenaire.Service.Models;

namespace tokenaire_backend.Controllers
{
    public static class ControllerExtensions
    {
        public static BadRequestObjectResult BadRequestFromErrors(this Controller controller, List<ServiceGenericError> errors)
        {
            var dtoGenericErrors = errors
            .Select(x => new DtoGenericError()
            {
                Code = (int)x.Code,
                Message = x.Message
            })
            .ToList();

            return controller.BadRequest(new DtoGenericErrorResponse()
            {
                Errors = dtoGenericErrors
            });
        }
    }
}
