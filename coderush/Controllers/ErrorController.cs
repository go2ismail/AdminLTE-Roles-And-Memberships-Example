using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace coderush.Controllers
{
    //custom error controller for handling 500 and 404 

    [AllowAnonymous]
    [Route("Error")]
    public class ErrorController : Controller
    {
        [Route("500")]
        public IActionResult AppError()
        {
            // Get the details of the exception that occurred
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                // Get which route the exception occurred at
                string routeWhereExceptionOccurred = exceptionFeature.Path;

                // Get the exception that occurred
                Exception exceptionThatOccurred = exceptionFeature.Error;

                // Write the exception path to debug output
                System.Diagnostics.Debug.WriteLine(routeWhereExceptionOccurred);
            }
            return View();
        }

        [Route("404")]
        public IActionResult PageNotFound()
        {
            // Perform any action before serving the View
            return View();
        }
    }
}