using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests;

internal static class TestAssert
{
    public static async Task<TException> ThrowsExceptionAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException exception)
        {
            return exception;
        }
        catch (Exception exception)
        {
            Assert.Fail($"Expected exception of type {typeof(TException).Name}, but got {exception.GetType().Name}.");
        }

        Assert.Fail($"Expected exception of type {typeof(TException).Name}, but no exception was thrown.");
        throw new InvalidOperationException("Unreachable assertion path.");
    }

    public static async Task ThrowsExceptionWithMessageAsync<TException>(Func<Task> action, string expectedMessagePart)
        where TException : Exception
    {
        TException exception = await ThrowsExceptionAsync<TException>(action);

        StringAssert.Contains(exception.Message, expectedMessagePart);
    }
}
