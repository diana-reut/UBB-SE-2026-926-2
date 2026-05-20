using System;
using HospitalManagement.Service;

namespace Common.Tests.Service;

[TestClass]
public sealed class GhostServiceTests
{
    private GhostService _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _sut = new GhostService();
    }

    [TestMethod]
    public void IsExorcismTriggeredWithNoSightingsReturnsFalse()
    {
        Assert.IsFalse(_sut.IsExorcismTriggered());
    }

    [TestMethod]
    public void IsExorcismTriggeredWithThreeSightingsReturnsFalse()
    {
        _sut.SawAGhost();
        _sut.SawAGhost();
        _sut.SawAGhost();

        Assert.IsFalse(_sut.IsExorcismTriggered());
    }

    [TestMethod]
    public void IsExorcismTriggeredWithFourSightingsReturnsTrue()
    {
        _sut.SawAGhost();
        _sut.SawAGhost();
        _sut.SawAGhost();
        _sut.SawAGhost();

        Assert.IsTrue(_sut.IsExorcismTriggered());
    }

    [TestMethod]
    public void SawAGhostWhenCalledThreeTimesDoesNotFireExorcismEvent()
    {
        int eventCount = 0;
        _sut.ExorcismTriggered += (_, _) => eventCount++;

        _sut.SawAGhost();
        _sut.SawAGhost();
        _sut.SawAGhost();

        Assert.AreEqual(0, eventCount);
    }

    [TestMethod]
    public void SawAGhostWhenCalledFourTimesFiresExorcismEventOnce()
    {
        int eventCount = 0;
        _sut.ExorcismTriggered += (_, _) => eventCount++;

        _sut.SawAGhost();
        _sut.SawAGhost();
        _sut.SawAGhost();
        _sut.SawAGhost();

        Assert.AreEqual(1, eventCount);
    }

    [TestMethod]
    public void SawAGhostWhenCalledFiveTimesFiresExorcismEventTwice()
    {
        int eventCount = 0;
        _sut.ExorcismTriggered += (_, _) => eventCount++;

        _sut.SawAGhost();
        _sut.SawAGhost();
        _sut.SawAGhost();
        _sut.SawAGhost();
        _sut.SawAGhost();

        Assert.AreEqual(2, eventCount);
    }
}
