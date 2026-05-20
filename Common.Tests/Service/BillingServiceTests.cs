using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class BillingServiceTests
{
    private readonly Mock<IMedicalHistoryRepository> _historyRepo = new();
    private readonly Mock<IMedicalRecordRepository> _recordRepo = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepo = new();
    private readonly Mock<ITransplantRepository> _transplantRepo = new();

    private BillingService CreateService()
    {
        return new BillingService(
            _historyRepo.Object,
            _recordRepo.Object,
            _prescriptionRepo.Object,
            _transplantRepo.Object);
    }

    [TestMethod]
    public async Task ApplyDiscountAsync_WithTenPercentDiscount_ReturnsDiscountedPrice()
    {
        BillingService service = CreateService();

        decimal result = await service.ApplyDiscountAsync(1000m, 10);

        Assert.AreEqual(900m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WhenRecordIsMissing_ReturnsZero()
    {
        BillingService service = CreateService();

        _recordRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((MedicalRecord?)null);

        _prescriptionRepo.Setup(x => x.GetByRecordIdAsync(1))
            .ReturnsAsync((Prescription?)null);

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(new MedicalHistory { Id = 10, PatientId = 1 });

        _historyRepo.Setup(x => x.GetChronicConditionsAsync(10))
            .ReturnsAsync(new List<string>());

        _historyRepo.Setup(x => x.GetAllergiesByHistoryIdAsync(10))
            .ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)>());

        _transplantRepo.Setup(x => x.GetByReceiverIdAsync(1))
            .ReturnsAsync(new List<Transplant>());

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(0m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WhenHistoryIsMissing_ReturnsZero()
    {
        BillingService service = CreateService();

        _recordRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new MedicalRecord { Id = 1, SourceType = SourceType.ER });

        _prescriptionRepo.Setup(x => x.GetByRecordIdAsync(1))
            .ReturnsAsync((Prescription?)null);

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync((MedicalHistory?)null);

        _transplantRepo.Setup(x => x.GetByReceiverIdAsync(1))
            .ReturnsAsync(new List<Transplant>());

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(0m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WithEmergencyRoomRecord_ReturnsEmergencyRoomBasePrice()
    {
        BillingService service = CreateService();

        SetupValidPatient(SourceType.ER);

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(500m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WithAppointmentRecord_ReturnsAppointmentBasePrice()
    {
        BillingService service = CreateService();

        SetupValidPatient(SourceType.App);

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(200m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WithTwoPrescriptionItems_AddsPrescriptionPrices()
    {
        BillingService service = CreateService();

        SetupValidPatient(SourceType.App);

        _prescriptionRepo.Setup(x => x.GetByRecordIdAsync(1))
            .ReturnsAsync(new Prescription { Id = 5 });

        _prescriptionRepo.Setup(x => x.GetItemsAsync(5))
            .ReturnsAsync(new List<PrescriptionItem>
            {
                new PrescriptionItem(),
                new PrescriptionItem()
            });

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(300m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WithTwoChronicConditions_AddsChronicConditionPrices()
    {
        BillingService service = CreateService();

        SetupValidPatient(SourceType.App);

        _historyRepo.Setup(x => x.GetChronicConditionsAsync(10))
            .ReturnsAsync(new List<string>
            {
                "Diabetes",
                "Asthma"
            });

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(400m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WithMildAllergy_AddsMildAllergyPrice()
    {
        BillingService service = CreateService();

        SetupValidPatient(SourceType.App);

        _historyRepo.Setup(x => x.GetAllergiesByHistoryIdAsync(10))
            .ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)>
            {
                (new Allergy(), "mild")
            });

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(220m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WithModerateAllergyIgnoringCase_AddsModerateAllergyPrice()
    {
        BillingService service = CreateService();

        SetupValidPatient(SourceType.App);

        _historyRepo.Setup(x => x.GetAllergiesByHistoryIdAsync(10))
            .ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)>
            {
                (new Allergy(), "MoDeRaTe")
            });

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(220m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WithSevereAllergy_AddsSevereAllergyPrice()
    {
        BillingService service = CreateService();

        SetupValidPatient(SourceType.App);

        _historyRepo.Setup(x => x.GetAllergiesByHistoryIdAsync(10))
            .ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)>
            {
                (new Allergy(), "severe")
            });

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(300m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WithAnaphylacticAllergyIgnoringCase_AddsSevereAllergyPrice()
    {
        BillingService service = CreateService();

        SetupValidPatient(SourceType.App);

        _historyRepo.Setup(x => x.GetAllergiesByHistoryIdAsync(10))
            .ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)>
            {
                (new Allergy(), "ANAPHYLACTIC")
            });

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(300m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WithUnknownAllergySeverity_DoesNotAddAllergyPrice()
    {
        BillingService service = CreateService();

        SetupValidPatient(SourceType.App);

        _historyRepo.Setup(x => x.GetAllergiesByHistoryIdAsync(10))
            .ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)>
            {
                (new Allergy(), "unknown")
            });

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(200m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WithAssociatedTransplant_AddsTransplantPrice()
    {
        BillingService service = CreateService();

        SetupValidPatient(SourceType.App);

        _transplantRepo.Setup(x => x.GetByReceiverIdAsync(1))
            .ReturnsAsync(new List<Transplant>
            {
                new Transplant()
            });

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(2200m, result);
    }

    [TestMethod]
    public async Task ComputeBasePriceAsync_WithCompleteBillingData_ReturnsCombinedPrice()
    {
        BillingService service = CreateService();

        SetupValidPatient(SourceType.ER);

        _prescriptionRepo.Setup(x => x.GetByRecordIdAsync(1))
            .ReturnsAsync(new Prescription { Id = 5 });

        _prescriptionRepo.Setup(x => x.GetItemsAsync(5))
            .ReturnsAsync(new List<PrescriptionItem>
            {
                new PrescriptionItem(),
                new PrescriptionItem()
            });

        _historyRepo.Setup(x => x.GetChronicConditionsAsync(10))
            .ReturnsAsync(new List<string>
            {
                "Diabetes",
                "Asthma"
            });

        _historyRepo.Setup(x => x.GetAllergiesByHistoryIdAsync(10))
            .ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)>
            {
                (new Allergy(), "mild"),
                (new Allergy(), "severe")
            });

        _transplantRepo.Setup(x => x.GetByReceiverIdAsync(1))
            .ReturnsAsync(new List<Transplant>
            {
                new Transplant()
            });

        decimal result = await service.ComputeBasePriceAsync(1, 1);

        Assert.AreEqual(2920m, result);
    }

    private void SetupValidPatient(SourceType sourceType)
    {
        _recordRepo.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new MedicalRecord
            {
                Id = 1,
                SourceType = sourceType
            });

        _prescriptionRepo.Setup(x => x.GetByRecordIdAsync(1))
            .ReturnsAsync((Prescription?)null);

        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(new MedicalHistory
            {
                Id = 10,
                PatientId = 1
            });

        _historyRepo.Setup(x => x.GetChronicConditionsAsync(10))
            .ReturnsAsync(new List<string>());

        _historyRepo.Setup(x => x.GetAllergiesByHistoryIdAsync(10))
            .ReturnsAsync(new List<(Allergy Allergy, string SeverityLevel)>());

        _transplantRepo.Setup(x => x.GetByReceiverIdAsync(1))
            .ReturnsAsync(new List<Transplant>());
    }
}