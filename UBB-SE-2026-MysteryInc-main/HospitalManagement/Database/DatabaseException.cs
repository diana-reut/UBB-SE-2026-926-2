using System;

namespace HospitalManagement.Database;

internal class DatabaseException : Exception
{
    public DatabaseException(string message)
        : base(message)
    {
    }

    public DatabaseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DatabaseException()
    {
    }
}
