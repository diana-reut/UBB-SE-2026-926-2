using Common.API.Services;
using Common.Data.Entity;
using Common.Data.Entity.Enums;
using Common.Data.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Common.Tests.Service;

[TestClass]
public sealed class TransplantServiceTests
{
    private readonly Mock<ITransplantRepository> _transplantRepo = new();
    private readonly Mock<IPatientRepository> _patientRepo = new();
    private readonly Mock<IMedicalRecordRepository> _recordRepo = new();
    private readonly Mock<IBloodCompatibilityService> _compatibilityService = new();
    private readonly Mock<IMedicalHistoryRepository> _historyRepo = new();

    private TransplantService CreateService()
    {
        return new TransplantService(
            _transplantRepo.Object,
            _patientRepo.Object,
            _recordRepo.Object,
            _compatibilityService.Object,
            _historyRepo.Object);
    }

    [TestMethod]
    public async Task GetAllAsync_ReturnsRepositoryResult()
    {
        TransplantService service = CreateService();
        var transplants = new List<Transplant> { CreateTransplant(1, 10) };
        _transplantRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(transplants);

        List<Transplant> result = await service.GetAllAsync();

        Assert.AreSame(transplants, result);
    }

    [TestMethod]
    public async Task CreateAsync_AddsAndReturnsTransplant()
    {
        TransplantService service = CreateService();
        Transplant transplant = CreateTransplant(1, 10);

        Transplant result = await service.CreateAsync(transplant);

        Assert.AreSame(transplant, result);
        _transplantRepo.Verify(x => x.AddAsync(transplant), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_ReturnsRepositoryResult()
    {
        TransplantService service = CreateService();
        Transplant transplant = CreateTransplant(1, 10);
        _transplantRepo.Setup(x => x.UpdateAsync(1, transplant)).ReturnsAsync(true);

        bool result = await service.UpdateAsync(1, transplant);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task DeleteAsync_ReturnsRepositoryResult()
    {
        TransplantService service = CreateService();
        _transplantRepo.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

        bool result = await service.DeleteAsync(1);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsRepositoryResult()
    {
        TransplantService service = CreateService();
        Transplant transplant = CreateTransplant(1, 10);
        _transplantRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(transplant);

        Transplant? result = await service.GetByIdAsync(1);

        Assert.AreSame(transplant, result);
    }

    [TestMethod]
    public async Task GetByReceiverIdAsync_ReturnsRepositoryResult()
    {
        TransplantService service = CreateService();
        var transplants = new List<Transplant> { CreateTransplant(1, 10) };
        _transplantRepo.Setup(x => x.GetByReceiverIdAsync(10)).ReturnsAsync(transplants);

        List<Transplant> result = await service.GetByReceiverIdAsync(10);

        Assert.AreSame(transplants, result);
    }

    [TestMethod]
    public async Task GetByDonorIdAsync_ReturnsRepositoryResult()
    {
        TransplantService service = CreateService();
        var transplants = new List<Transplant> { CreateTransplant(1, 10, donorId: 20) };
        _transplantRepo.Setup(x => x.GetByDonorIdAsync(20)).ReturnsAsync(transplants);

        List<Transplant> result = await service.GetByDonorIdAsync(20);

        Assert.AreSame(transplants, result);
    }

    [TestMethod]
    public async Task CreateWaitlistRequestAsync_WhenReceiverDoesNotExist_Throws()
    {
        TransplantService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync((Patient?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<ArgumentException>(() => service.CreateWaitlistRequestAsync(10, "Kidney"));
    }

    [TestMethod]
    public async Task CreateWaitlistRequestAsync_NormalizesOrganTypeAndAddsPendingRequest()
    {
        TransplantService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(10)).ReturnsAsync(CreatePatient(10));
        Transplant? added = null;
        _transplantRepo.Setup(x => x.AddAsync(It.IsAny<Transplant>()))
            .Callback<Transplant>(t => added = t)
            .Returns(Task.CompletedTask);

        await service.CreateWaitlistRequestAsync(10, "Lungs");

        Assert.IsTrue(
            added is not null
            && added.ReceiverId == 10
            && added.DonorId is null
            && added.OrganType == "Lung"
            && added.Status == TransplantStatus.Pending
            && added.CompatibilityScore == 0);
    }

    [TestMethod]
    public async Task AssignDonorAsync_DelegatesToRepository()
    {
        TransplantService service = CreateService();

        await service.AssignDonorAsync(1, 20, 95.5f);

        _transplantRepo.Verify(x => x.UpdateAsync(1, 20, 95.5f), Times.Once);
    }

    [DataTestMethod]
    [DataRow(false, true)]
    [DataRow(true, false)]
    public async Task GetTopMatchesForDonorAsync_WhenDonorIsNotEligible_Throws(bool isDeceased, bool isDonor)
    {
        TransplantService service = CreateService();
        Patient donor = CreatePatient(20, isDonor: isDonor, dod: isDeceased ? DateTime.UtcNow.AddDays(-1) : null);
        _patientRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(donor);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<InvalidOperationException>(() => service.GetTopMatchesForDonorAsync(20, "Kidney"));
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenDonorDoesNotExist_Throws()
    {
        TransplantService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync((Patient?)null);

        await Common.Tests.TestAssert.ThrowsExceptionAsync<InvalidOperationException>(() => service.GetTopMatchesForDonorAsync(20, "Kidney"));
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_FiltersScoresOrdersAndLimitsMatches()
    {
        TransplantService service = CreateService();
        Patient donor = CreatePatient(20, isDonor: true, dod: DateTime.UtcNow.AddDays(-1));
        _patientRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(donor);
        _historyRepo.Setup(x => x.GetByPatientIdAsync(20)).ReturnsAsync(CreateHistory(BloodType.O, Rh.Negative));
        List<Transplant> waitlist = Enumerable.Range(1, 8)
            .Select(i => CreateTransplant(i, i, organType: "Lung", requestDate: new DateTime(2024, 1, i)))
            .ToList();
        _transplantRepo.Setup(x => x.GetWaitingByOrganAsync("Lung")).ReturnsAsync(waitlist);

        for (int id = 1; id <= 6; id++)
        {
            int receiverId = id;
            _patientRepo.Setup(x => x.GetByIdAsync(receiverId)).ReturnsAsync(CreatePatient(receiverId));
            _historyRepo.Setup(x => x.GetByPatientIdAsync(receiverId)).ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));
            _compatibilityService.Setup(x => x.IsBloodMatch(BloodType.O, BloodType.A)).Returns(true);
            _compatibilityService.Setup(x => x.IsRhMatch(Rh.Negative, Rh.Positive)).Returns(true);
            _compatibilityService.Setup(x => x.CalculateScore(donor, It.Is<Patient>(p => p.Id == receiverId))).Returns(receiverId * 10);
            _recordRepo.Setup(x => x.GetERVisitCountAsync(receiverId, It.IsAny<DateTime>())).ReturnsAsync(receiverId == 6 ? 10 : 0);
        }

        _patientRepo.Setup(x => x.GetByIdAsync(7)).ReturnsAsync((Patient?)null);
        _patientRepo.Setup(x => x.GetByIdAsync(8)).ReturnsAsync(CreatePatient(8));
        _historyRepo.Setup(x => x.GetByPatientIdAsync(8)).ReturnsAsync(new MedicalHistory { BloodType = null, Rh = Rh.Positive });

        List<Transplant> result = await service.GetTopMatchesForDonorAsync(20, "Lungs");

        Assert.IsTrue(
            result.Select(t => t.TransplantId).SequenceEqual(new[] { 6, 5, 4, 3, 2 })
            && waitlist[5].CompatibilityScore == 80
            && waitlist[4].CompatibilityScore == 55);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_SkipsBloodAndRhIncompatibleReceivers()
    {
        TransplantService service = CreateService();
        Patient donor = CreatePatient(20, isDonor: true, dod: DateTime.UtcNow.AddDays(-1));
        _patientRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(donor);
        _historyRepo.Setup(x => x.GetByPatientIdAsync(20)).ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));
        _transplantRepo.Setup(x => x.GetWaitingByOrganAsync("Kidney"))
            .ReturnsAsync(new List<Transplant>
            {
                CreateTransplant(1, 1),
                CreateTransplant(2, 2),
                CreateTransplant(3, 3)
            });
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreatePatient(1));
        _patientRepo.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(CreatePatient(2));
        _patientRepo.Setup(x => x.GetByIdAsync(3)).ReturnsAsync(CreatePatient(3));
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(CreateHistory(BloodType.B, Rh.Positive));
        _historyRepo.Setup(x => x.GetByPatientIdAsync(2)).ReturnsAsync(CreateHistory(BloodType.A, Rh.Negative));
        _historyRepo.Setup(x => x.GetByPatientIdAsync(3)).ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));
        _compatibilityService.Setup(x => x.IsBloodMatch(BloodType.A, BloodType.B)).Returns(false);
        _compatibilityService.Setup(x => x.IsBloodMatch(BloodType.A, BloodType.A)).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(Rh.Positive, Rh.Negative)).Returns(false);
        _compatibilityService.Setup(x => x.IsRhMatch(Rh.Positive, Rh.Positive)).Returns(true);
        _compatibilityService.Setup(x => x.CalculateScore(donor, It.Is<Patient>(p => p.Id == 3))).Returns(70);
        _recordRepo.Setup(x => x.GetERVisitCountAsync(3, It.IsAny<DateTime>())).ReturnsAsync(0);

        List<Transplant> result = await service.GetTopMatchesForDonorAsync(20, "Kidney");

        Assert.IsTrue(result.Count == 1 && result[0].ReceiverId == 3);
    }

    [TestMethod]
    public async Task GetTopMatchesForDonorAsync_WhenScoresTie_OrdersByOldestRequestDate()
    {
        TransplantService service = CreateService();
        Patient donor = CreatePatient(20, isDonor: true, dod: DateTime.UtcNow.AddDays(-1));
        Patient firstReceiver = CreatePatient(1);
        Patient secondReceiver = CreatePatient(2);
        _patientRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(donor);
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(firstReceiver);
        _patientRepo.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(secondReceiver);
        _historyRepo.Setup(x => x.GetByPatientIdAsync(It.IsAny<int>())).ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));
        _transplantRepo.Setup(x => x.GetWaitingByOrganAsync("Kidney"))
            .ReturnsAsync(new List<Transplant>
            {
                CreateTransplant(1, 1, requestDate: new DateTime(2024, 2, 1)),
                CreateTransplant(2, 2, requestDate: new DateTime(2024, 1, 1))
            });
        _compatibilityService.Setup(x => x.IsBloodMatch(BloodType.A, BloodType.A)).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(Rh.Positive, Rh.Positive)).Returns(true);
        _compatibilityService.Setup(x => x.CalculateScore(It.IsAny<Patient>(), It.IsAny<Patient>())).Returns(50);
        _recordRepo.Setup(x => x.GetERVisitCountAsync(It.IsAny<int>(), It.IsAny<DateTime>())).ReturnsAsync(0);

        List<Transplant> result = await service.GetTopMatchesForDonorAsync(20, "Kidney");

        CollectionAssert.AreEqual(new[] { 2, 1 }, result.Select(t => t.TransplantId).ToArray());
    }

    [TestMethod]
    public async Task GetTopMatchesAsDisplayModelsAsync_MapsMatchesWithReceiverDetails()
    {
        TransplantService service = CreateService();
        Patient donor = CreatePatient(20, isDonor: true, dod: DateTime.UtcNow.AddDays(-1));
        Patient receiver = CreatePatient(1, firstName: "Ana", lastName: "Pop");
        _patientRepo.Setup(x => x.GetByIdAsync(20)).ReturnsAsync(donor);
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(receiver);
        _historyRepo.Setup(x => x.GetByPatientIdAsync(20)).ReturnsAsync(CreateHistory(BloodType.A, Rh.Positive));
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(CreateHistory(BloodType.AB, Rh.Positive));
        _transplantRepo.Setup(x => x.GetWaitingByOrganAsync("Kidney"))
            .ReturnsAsync(new List<Transplant>
            {
                CreateTransplant(7, 1, requestDate: DateTime.UtcNow.AddDays(-3))
            });
        _compatibilityService.Setup(x => x.IsBloodMatch(BloodType.A, BloodType.AB)).Returns(true);
        _compatibilityService.Setup(x => x.IsRhMatch(Rh.Positive, Rh.Positive)).Returns(true);
        _compatibilityService.Setup(x => x.CalculateScore(donor, receiver)).Returns(80);
        _recordRepo.Setup(x => x.GetERVisitCountAsync(1, It.IsAny<DateTime>())).ReturnsAsync(0);

        List<TransplantMatch> result = await service.GetTopMatchesAsDisplayModelsAsync(20, "Kidney");

        Assert.IsTrue(
            result.Count == 1
            && result[0].TransplantId == 7
            && result[0].ReceiverName == "Ana Pop"
            && result[0].BloodType == "AB"
            && result[0].CompatibilityScore == 85
            && result[0].WaitingDays >= 2);
    }

    [TestMethod]
    public async Task IsUrgentAsync_ReturnsTrueWhenVisitsMeetThreshold()
    {
        TransplantService service = CreateService();
        _recordRepo.Setup(x => x.GetERVisitCountAsync(1, It.IsAny<DateTime>())).ReturnsAsync(10);
        _recordRepo.Setup(x => x.GetERVisitCountAsync(2, It.IsAny<DateTime>())).ReturnsAsync(9);

        Assert.IsTrue(await service.IsUrgentAsync(1) && !await service.IsUrgentAsync(2));
    }

    [TestMethod]
    public async Task GetChronicWarningAsync_WhenPatientDoesNotExist_ReturnsNull()
    {
        TransplantService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Patient?)null);

        string? result = await service.GetChronicWarningAsync(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetChronicWarningAsync_WhenPatientHasNoChronicConditions_ReturnsNull()
    {
        TransplantService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreatePatient(1));
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1)).ReturnsAsync(new MedicalHistory { ChronicConditions = [] });

        string? result = await service.GetChronicWarningAsync(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetChronicWarningAsync_WhenPatientHasChronicConditions_ReturnsWarning()
    {
        TransplantService service = CreateService();
        _patientRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreatePatient(1));
        _historyRepo.Setup(x => x.GetByPatientIdAsync(1))
            .ReturnsAsync(new MedicalHistory { ChronicConditions = ["Asthma"] });

        string? result = await service.GetChronicWarningAsync(1);

        Assert.AreEqual("Patient has underlying conditions that may affect transplant success.", result);
    }

    private static Transplant CreateTransplant(
        int id,
        int receiverId,
        int? donorId = null,
        string organType = "Kidney",
        DateTime? requestDate = null)
    {
        return new Transplant
        {
            TransplantId = id,
            ReceiverId = receiverId,
            DonorId = donorId,
            OrganType = organType,
            RequestDate = requestDate ?? new DateTime(2024, 1, 1),
            Status = TransplantStatus.Pending,
        };
    }

    private static Patient CreatePatient(
        int id,
        string firstName = "First",
        string lastName = "Last",
        bool isDonor = false,
        DateTime? dod = null)
    {
        return new Patient
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Dob = new DateTime(1990, 1, 1),
            Dod = dod,
            IsDonor = isDonor,
        };
    }

    private static MedicalHistory CreateHistory(BloodType? bloodType, Rh? rh)
    {
        return new MedicalHistory
        {
            BloodType = bloodType,
            Rh = rh,
        };
    }
}
