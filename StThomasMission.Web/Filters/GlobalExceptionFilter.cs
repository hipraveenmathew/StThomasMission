using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StThomasMission.Web.Models;

namespace StThomasMission.Web.Filters
{
    /// <summary>
    /// A custom filter to catch all unhandled exceptions globally, log them,
    /// and display a user-friendly error page.
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (!context.ExceptionHandled)
            {
                var exception = context.Exception;
                _logger.LogError(exception, "An unhandled exception occurred: {ErrorMessage}", exception.Message);

                var errorModel = new ErrorViewModel
                {
                    // In production, you might want a more generic message.
                    Message = "An unexpected error occurred. Please try again later.",
                    RequestId = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier
                };

                var result = new ViewResult
                {
                    ViewName = "Error", // Points to /Views/Shared/Error.cshtml
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                        new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                        context.ModelState)
                    {
                        Model = errorModel
                    }
                };

                context.Result = result;
                context.ExceptionHandled = true;
            }
        }
    }
}