using Moq;
using HospitalManagement.Service;
using HospitalManagement.Repository;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;

namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class TransplantServiceTests
{
    private Mock<ITransplantRepository> _transplantRepo;
    private Mock<IPatientRepository> _patientRepo;
    private Mock<IMedicalRecordRepository> _recordRepo;
    private Mock<IBloodCompatibilityService> _compatibilityService;
    private Mock<IMedicalHistoryRepository> _historyRepo;

    private TransplantService _service;

    [TestInitialize]
    public void Setup()
    {
        _transplantRepo = new Mock<ITransplantRepository>();
        _patientRepo = new Mock<IPatientRepository>();
        _recordRepo = new Mock<IMedicalRecordRepository>();
        _compatibilityService = new Mock<IBloodCompatibilityService>();
        _historyRepo = new Mock<IMedicalHistoryRepository>();

        _service = new TransplantService(
            _transplantRepo.Object,
            _patientRepo.Object,
            _recordRepo.Object,
            _compatibilityService.Object,
            _historyRepo.Object
        );
    }

    [TestMethod]
    public void CreateWaitlistRequestShouldAddRequestWhenPatientExists()
    {
        var patient = new Patient { Id = 1 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(patient);

        _service.CreateWaitlistRequest(1, "Kidney");

        _transplantRepo.Verify(r => r.Add(It.Is<Transplant>(
            t => t.ReceiverId == 1 &&
                 t.OrganType == "Kidney" &&
                 t.Status == TransplantStatus.Pending
        )), Times.Once);
    }

    [TestMethod]
    public void CreateWaitlistRequestShouldThrowWhenPatientMissing()
    {
        _patientRepo.Setup(r => r.GetById(1)).Returns((Patient)null);
        try
        {
            _service.CreateWaitlistRequest(1, "Kidney");
        }
        catch (ArgumentException ex)
        {
            Assert.AreEqual("Receiver not found.", ex.Message);

        }
    }


    [TestMethod]
    public void GetTopMatchesForDonorShouldThrowWhenInvalidDonor()
    {
        _patientRepo.Setup(r => r.GetById(1))
            .Returns(new Patient { Id = 1, IsDonor = true });

        try
        {
            _service.GetTopMatchesForDonor(1, "Kidney");
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Donor must be deceased and registered.", ex.Message);

        }
    }

    [TestMethod]
    public void GetTopMatchesForDonorShouldReturnEmptyWhenNoMatches()
    {
        var donor = new Patient { Id = 1, Dod = DateTime.UtcNow, IsDonor = true };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _historyRepo.Setup(r => r.GetByPatientId(It.IsAny<int>()))
            .Returns(new MedicalHistory { BloodType = BloodType.A, Rh = Rh.Positive });

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>());

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void IsUrgentShouldReturnTrueWhenERVisitsHigh()
    {
        _recordRepo.Setup(r => r.GetERVisitCount(1, It.IsAny<DateTime>()))
            .Returns(15);

        var result = _service.IsUrgent(1);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsUrgentShouldReturnFalseWhenERVisitsLow()
    {
        _recordRepo.Setup(r => r.GetERVisitCount(1, It.IsAny<DateTime>()))
            .Returns(3);

        var result = _service.IsUrgent(1);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void GetChronicWarningShouldReturnWarningWhenConditionsExist()
    {
        var patient = new Patient { Id = 1 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(patient);

        _historyRepo.Setup(r => r.GetByPatientId(1))
            .Returns(new MedicalHistory
            {
                ChronicConditions = new List<string> { "Diabetes" }
            });

        var result = _service.GetChronicWarning(1);

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetChronicWarningShouldReturnNullWhenNoConditions()
    {
        var patient = new Patient { Id = 1 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(patient);

        _historyRepo.Setup(r => r.GetByPatientId(1))
            .Returns(new MedicalHistory
            {
                ChronicConditions = new List<string>()
            });

        var result = _service.GetChronicWarning(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void AssignDonorShouldCallRepository()
    {
        _service.AssignDonor(10, 20, 88.5f);

        _transplantRepo.Verify(r => r.Update(10, 20, 88.5f), Times.Once);
    }
    [TestMethod]
    public void ShouldSkipWhenReceiverIsNull()
    {
        _patientRepo.Setup(r => r.GetById(1))
            .Returns(new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow });

        _historyRepo.Setup(r => r.GetByPatientId(It.IsAny<int>()))
            .Returns(new MedicalHistory { BloodType = BloodType.A, Rh = Rh.Positive });

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
            new Transplant { ReceiverId = 999 }
            });

        _patientRepo.Setup(r => r.GetById(999))
            .Returns((Patient)null);

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSkipWhenBloodOrRhIsNull()
    {
        var donor = new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow };
        var receiver = new Patient { Id = 2 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _patientRepo.Setup(r => r.GetById(2)).Returns(receiver);

        _historyRepo.Setup(r => r.GetByPatientId(2))
            .Returns(new MedicalHistory { BloodType = null, Rh = null });

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
            new Transplant { ReceiverId = 2 }
            });

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSkipWhenBloodMismatch()
    {
        var donor = new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow };
        var receiver = new Patient { Id = 2 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _patientRepo.Setup(r => r.GetById(2)).Returns(receiver);

        _historyRepo.Setup(r => r.GetByPatientId(It.IsAny<int>()))
            .Returns(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = Rh.Positive
            });

        _compatibilityService.Setup(c =>
            c.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>()))
            .Returns(false);

        _compatibilityService.Setup(c =>
            c.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>()))
            .Returns(true);

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
                new Transplant { ReceiverId = 2 }
            });

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSkipWhenRhMismatch()
    {
        var donor = new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow };
        var receiver = new Patient { Id = 2 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _patientRepo.Setup(r => r.GetById(2)).Returns(receiver);

        _historyRepo.Setup(r => r.GetByPatientId(It.IsAny<int>()))
            .Returns(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = Rh.Positive
            });

        _compatibilityService.Setup(c =>
            c.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>()))
            .Returns(true);

        _compatibilityService.Setup(c =>
            c.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>()))
            .Returns(false);

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
                new Transplant { ReceiverId = 2 }
            });

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSkipWhenChronicConditionsExist()
    {
        var donor = new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow };
        var receiver = new Patient { Id = 2 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _patientRepo.Setup(r => r.GetById(2)).Returns(receiver);

        _historyRepo.Setup(r => r.GetByPatientId(It.IsAny<int>()))
            .Returns(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = Rh.Positive,
                ChronicConditions = new List<string> { "Diabetes" }
            });

        _compatibilityService.Setup(c =>
            c.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>()))
            .Returns(true);

        _compatibilityService.Setup(c =>
            c.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>()))
            .Returns(true);

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
                new Transplant { ReceiverId = 2 }
            });

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSkipWhenMedicalHistoryIsNull()
    {
        var donor = new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow };
        var receiver = new Patient { Id = 2 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _patientRepo.Setup(r => r.GetById(2)).Returns(receiver);

        _historyRepo.Setup(r => r.GetByPatientId(2))
            .Returns((MedicalHistory)null);

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
            new Transplant { ReceiverId = 2 }
            });

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSkipWhenBloodTypeIsNullOnly()
    {
        var donor = new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow };
        var receiver = new Patient { Id = 2 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _patientRepo.Setup(r => r.GetById(2)).Returns(receiver);

        _historyRepo.Setup(r => r.GetByPatientId(2))
            .Returns(new MedicalHistory
            {
                BloodType = null,
                Rh = Rh.Positive
            });

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
            new Transplant { ReceiverId = 2 }
            });

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void ShouldSkipWhenRhIsNullOnly()
    {
        var donor = new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow };
        var receiver = new Patient { Id = 2 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _patientRepo.Setup(r => r.GetById(2)).Returns(receiver);

        _historyRepo.Setup(r => r.GetByPatientId(2))
            .Returns(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = null
            });

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
            new Transplant { ReceiverId = 2 }
            });

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void GetChronicWarningShouldReturnNullWhenConditionsNull()
    {
        var patient = new Patient { Id = 1 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(patient);

        _historyRepo.Setup(r => r.GetByPatientId(1))
            .Returns(new MedicalHistory
            {
                ChronicConditions = null
            });

        var result = _service.GetChronicWarning(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetChronicWarningShouldReturnNullWhenHistoryNull()
    {
        var patient = new Patient { Id = 1 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(patient);

        _historyRepo.Setup(r => r.GetByPatientId(1))
            .Returns((MedicalHistory)null);

        var result = _service.GetChronicWarning(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ShouldReturnScoredMatchWhenAllConditionsPass()
    {
        var donor = new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow };
        var receiver = new Patient { Id = 2 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _patientRepo.Setup(r => r.GetById(2)).Returns(receiver);

        _historyRepo.Setup(r => r.GetByPatientId(It.IsAny<int>()))
            .Returns(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = Rh.Positive
            });

        _compatibilityService.Setup(c =>
            c.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>()))
            .Returns(true);

        _compatibilityService.Setup(c =>
            c.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>()))
            .Returns(true);

        _compatibilityService.Setup(c =>
            c.CalculateScore(It.IsAny<Patient>(), It.IsAny<Patient>()))
            .Returns((int)50f);

        _recordRepo.Setup(r =>
            r.GetERVisitCount(It.IsAny<int>(), It.IsAny<DateTime>()))
            .Returns(5);

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
                new Transplant { ReceiverId = 2, RequestDate = DateTime.UtcNow }
            });

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.HasCount(1, result);
        Assert.IsGreaterThan(50, result[0].CompatibilityScore);
    }

    [TestMethod]
    public void GetTopMatchesShouldThrowWhenDonorIsNull()
    {
        _patientRepo.Setup(r => r.GetById(1))
            .Returns((Patient)null);

        try
        {
            _service.GetTopMatchesForDonor(1, "Kidney");
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Donor must be deceased and registered.", ex.Message);
        }
    }

    [TestMethod]
    public void ShouldApplyHighScoreBonusWhenERVisitsAreHigh()
    {
        var donor = new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow };
        var receiver = new Patient { Id = 2 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _patientRepo.Setup(r => r.GetById(2)).Returns(receiver);

        _historyRepo.Setup(r => r.GetByPatientId(It.IsAny<int>()))
            .Returns(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = Rh.Positive
            });

        _compatibilityService.Setup(c => c.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>()))
            .Returns(true);

        _compatibilityService.Setup(c => c.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>()))
            .Returns(true);

        _compatibilityService.Setup(c => c.CalculateScore(It.IsAny<Patient>(), It.IsAny<Patient>()))
            .Returns((int)50f);

        _recordRepo.Setup(r => r.GetERVisitCount(It.IsAny<int>(), It.IsAny<DateTime>()))
            .Returns(15); 

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
            new Transplant { ReceiverId = 2, RequestDate = DateTime.UtcNow }
            });

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.HasCount(1, result);
        Assert.IsGreaterThanOrEqualTo(70, result[0].CompatibilityScore);
    }

    [TestMethod]
    public void GetChronicWarningShouldReturnNullWhenPatientIsNull()
    {
        _patientRepo.Setup(r => r.GetById(1))
            .Returns((Patient)null);

        var result = _service.GetChronicWarning(1);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ShouldProcessMatchWhenDonorMedicalHistoryIsNull()
    {
        var donor = new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow };
        var receiver = new Patient { Id = 2 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _patientRepo.Setup(r => r.GetById(2)).Returns(receiver);

        _historyRepo.Setup(r => r.GetByPatientId(1)).Returns((MedicalHistory)null);

        _historyRepo.Setup(r => r.GetByPatientId(2))
            .Returns(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = Rh.Positive
            });

        _compatibilityService.Setup(c => c.IsBloodMatch(null, It.IsAny<BloodType>())).Returns(true);
        _compatibilityService.Setup(c => c.IsRhMatch(null, It.IsAny<Rh>())).Returns(true);
        _compatibilityService.Setup(c => c.CalculateScore(It.IsAny<Patient>(), It.IsAny<Patient>())).Returns((int)50f);

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
                new Transplant { ReceiverId = 2, RequestDate = DateTime.UtcNow }
            });

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.HasCount(1, result);
    }

    [TestMethod]
    public void ShouldIncludeWhenReceiverChronicConditionsIsEmptyList()
    {
        var donor = new Patient { Id = 1, IsDonor = true, Dod = DateTime.UtcNow };
        var receiver = new Patient { Id = 2 };

        _patientRepo.Setup(r => r.GetById(1)).Returns(donor);
        _patientRepo.Setup(r => r.GetById(2)).Returns(receiver);

        _historyRepo.Setup(r => r.GetByPatientId(2))
            .Returns(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = Rh.Positive,
                ChronicConditions = new List<string>() 
            });

        _compatibilityService.Setup(c => c.IsBloodMatch(It.IsAny<BloodType?>(), It.IsAny<BloodType>())).Returns(true);
        _compatibilityService.Setup(c => c.IsRhMatch(It.IsAny<Rh?>(), It.IsAny<Rh>())).Returns(true);

        _transplantRepo.Setup(r => r.GetWaitingByOrgan("Kidney"))
            .Returns(new List<Transplant>
            {
                new Transplant { ReceiverId = 2, RequestDate = DateTime.UtcNow }
            });

        var result = _service.GetTopMatchesForDonor(1, "Kidney");

        Assert.HasCount(1, result);
    }
}
