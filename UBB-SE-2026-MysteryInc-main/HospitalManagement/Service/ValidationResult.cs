using System.Collections.Generic;

internal class ValidationResult
{
    public bool IsValid { get; set; } = true;

    public List<string> Errors { get; set; } = [];

    public void AddError(string message)
    {
        IsValid = false;
        Errors.Add(message);
    }
}
