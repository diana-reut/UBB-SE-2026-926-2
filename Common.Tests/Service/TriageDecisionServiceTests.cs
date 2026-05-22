using Common.API.Services;
using Common.Data.Models;

namespace Common.Tests.Service;

[TestClass]
public sealed class TriageDecisionServiceTests
{
    private readonly TriageDecisionService _sut = new();

    private static Triage_Parameters MakeParameters(
        int consciousness = 1,
        int breathing = 1,
        int bleeding = 1,
        int injuryType = 1,
        int painLevel = 1) =>
        new()
        {
            Consciousness = consciousness,
            Breathing = breathing,
            Bleeding = bleeding,
            Injury_Type = injuryType,
            Pain_Level = painLevel
        };

    [TestMethod]
    public void CalculateTriageLevel_WhenCriticalBreathing_ReturnsLevelOne()
    {
        int result = _sut.CalculateTriageLevel(MakeParameters(breathing: 3));

        Assert.AreEqual(1, result);
    }

    [TestMethod]
    public void CalculateTriageLevel_WhenSeverityScoreIsTwenty_ReturnsLevelTwo()
    {
        int result = _sut.CalculateTriageLevel(MakeParameters(2, 2, 2, 2, 2));

        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public void CalculateTriageLevel_WhenSeverityScoreIsSixteen_ReturnsLevelThree()
    {
        int result = _sut.CalculateTriageLevel(MakeParameters(2, 2, 1, 1, 2));

        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void CalculateTriageLevel_WhenSeverityScoreIsTwelve_ReturnsLevelFour()
    {
        int result = _sut.CalculateTriageLevel(MakeParameters(1, 1, 1, 1, 2));

        Assert.AreEqual(4, result);
    }

    [TestMethod]
    public void CalculateTriageLevel_WhenSeverityScoreIsBelowTwelve_ReturnsLevelFive()
    {
        int result = _sut.CalculateTriageLevel(MakeParameters(1, 1, 1, 1, 1));

        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public void DetermineSpecialization_WhenInjuryTypeIsCritical_ReturnsGeneralSurgery()
    {
        string result = _sut.DetermineSpecialization(MakeParameters(injuryType: 3));

        Assert.AreEqual("General Surgery", result);
    }

    [TestMethod]
    public void DetermineSpecialization_WhenInjuryTypeIsModerate_ReturnsOrthopedics()
    {
        string result = _sut.DetermineSpecialization(MakeParameters(injuryType: 2));

        Assert.AreEqual("Orthopedics", result);
    }

    [TestMethod]
    public void DetermineSpecialization_WhenBreathingIsModerate_ReturnsPulmonology()
    {
        string result = _sut.DetermineSpecialization(MakeParameters(breathing: 2));

        Assert.AreEqual("Pulmonology", result);
    }

    [TestMethod]
    public void DetermineSpecialization_WhenConsciousnessIsModerate_ReturnsNeurology()
    {
        string result = _sut.DetermineSpecialization(MakeParameters(consciousness: 2));

        Assert.AreEqual("Neurology", result);
    }

    [TestMethod]
    public void DetermineSpecialization_WhenParametersAreMild_ReturnsEmergencyMedicine()
    {
        string result = _sut.DetermineSpecialization(MakeParameters());

        Assert.AreEqual("Emergency Medicine", result);
    }
}
