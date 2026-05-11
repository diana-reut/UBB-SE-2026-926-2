using System;

namespace Common.Data;

public class MyNotImplementedException : Exception
{
    public MyNotImplementedException()
    {
    }

    public MyNotImplementedException(string message)
        : base(message)
    {
    }

    public MyNotImplementedException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
