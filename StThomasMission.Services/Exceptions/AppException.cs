using System;
using System.Globalization;

namespace StThomasMission.Services.Exceptions
{
    // Custom exception for application-level errors (like validation failures from Identity).
    public class AppException : Exception
    {
        public AppException() : base() { }

        public AppException(string message) : base(message) { }

        public AppException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }
}