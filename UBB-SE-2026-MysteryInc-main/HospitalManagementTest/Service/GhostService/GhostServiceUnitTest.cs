using HospitalManagement.Service;

namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class GhostServiceUnitTests
{
    private GhostService _service;

    [TestInitialize]
    public void Setup()
    {
        _service = new GhostService();
    }

    [TestMethod]
    public void IsExorcismTriggered_NoSightings_ShouldReturnFalse()
    {
        bool result = _service.IsExorcismTriggered();
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsExorcismTriggered_ExactlyThreeSightings_ShouldReturnFalse()
    {
        _service.SawAGhost();
        _service.SawAGhost();
        _service.SawAGhost();

        bool result = _service.IsExorcismTriggered();
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsExorcismTriggered_MoreThanThreeSightings_ShouldReturnTrue()
    {
        _service.SawAGhost();
        _service.SawAGhost();
        _service.SawAGhost();
        _service.SawAGhost();

        bool result = _service.IsExorcismTriggered();
        Assert.IsTrue(result);
    }


    [TestMethod]
    public void SawAGhost_BelowThreshold_ShouldNotFireEvent()
    {
        bool eventFired = false;
        _service.ExorcismTriggered += (s, e) => eventFired = true;

        _service.SawAGhost();
        _service.SawAGhost();
        _service.SawAGhost();

        Assert.IsFalse(eventFired);
    }

    public void SawAGhost_AboveThreshold_ShouldFireEvent()
    {
        bool eventFired = false;
        _service.ExorcismTriggered += (s, e) => eventFired = true;

        _service.SawAGhost();
        _service.SawAGhost();
        _service.SawAGhost();
        _service.SawAGhost();

        Assert.IsTrue(eventFired);
    }

    [TestMethod]
    public void SawAGhost_AboveThreshold_ShouldFireEventWithCorrectSender()
    {
        object? capturedSender = null;
        _service.ExorcismTriggered += (s, e) => capturedSender = s;

        _service.SawAGhost();
        _service.SawAGhost();
        _service.SawAGhost();
        _service.SawAGhost();

        Assert.AreSame(_service, capturedSender);
    }
}
