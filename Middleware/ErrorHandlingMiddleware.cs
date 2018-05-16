using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tokenaire.Service;
using Tokenaire.Service.Models;

namespace Tokenaire.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogService logService;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogService logService)
        {
            this.logService = logService;
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await this.logService.Error(new ServiceLogError() {
                    Message = ex.ToString()
                });

                throw;
            }
        }
    }
}