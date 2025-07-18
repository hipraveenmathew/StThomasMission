using System;

namespace StThomasMission.Services.Exceptions
{
    // Custom exception for cleaner error handling when an entity is not found.
    public class NotFoundException : Exception
    {
        public NotFoundException(string entityName, object key)
            : base($"Entity \"{entityName}\" with key ({key}) was not found.")
        {
        }
    }
}