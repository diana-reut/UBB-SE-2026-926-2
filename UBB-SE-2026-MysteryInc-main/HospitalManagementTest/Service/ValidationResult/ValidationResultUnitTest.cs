
namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class ValidationResultTests
{

    [TestMethod]
    public void ValidationResultShouldBeValidByDefault()
    {
        var result = new ValidationResult();

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void AddErrorShouldSetIsValidToFalse()
    {
        var result = new ValidationResult();

        result.AddError("Error 1");

        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void AddErrorShouldAddErrorToList()
    {
        var result = new ValidationResult();

        result.AddError("Error 1");

        Assert.HasCount(1, result.Errors);
        Assert.AreEqual("Error 1", result.Errors[0]);
    }

    [TestMethod]
    public void AddErrorShouldAccumulateMultipleErrors()
    {
        var result = new ValidationResult();

        result.AddError("Error 1");
        result.AddError("Error 2");

        Assert.HasCount(2, result.Errors);
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void IsValidShouldRemainFalseAfterErrors()
    {
        var result = new ValidationResult();

        result.AddError("Error 1");
        result.AddError("Error 2");

        Assert.IsFalse(result.IsValid);
    }
}