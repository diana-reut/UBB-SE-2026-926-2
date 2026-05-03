using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Repository;
using HospitalManagement.Service;
using Moq;


namespace HospitalManagement.Tests.UnitTests;

[TestClass]
public class StatisticsServiceTests
{
    private Mock<IPatientRepository> _patientRepo;
    private Mock<IMedicalRecordRepository> _recordRepo;
    private Mock<IPrescriptionRepository> _prescriptionRepo;
    private StatisticsService _service;

    [TestInitialize]
    public void Setup()
    {
        _patientRepo = new Mock<IPatientRepository>();
        _recordRepo = new Mock<IMedicalRecordRepository>();
        _prescriptionRepo = new Mock<IPrescriptionRepository>();

        _service = new StatisticsService(
            _patientRepo.Object,
            _recordRepo.Object,
            _prescriptionRepo.Object
        );
    }

    [TestMethod]
    public void GetPatientsByBloodTypeShouldGroupCorrectly()
    {
        var patients = new List<Patient>
        {
            new Patient { MedicalHistory = new MedicalHistory { BloodType = BloodType.A } },
            new Patient { MedicalHistory = new MedicalHistory { BloodType = BloodType.A } },
            new Patient { MedicalHistory = new MedicalHistory { BloodType = BloodType.B } }
        };

        _patientRepo.Setup(r => r.GetAll(true)).Returns(patients);

        var result = _service.GetPatientsByBloodType();

        Assert.AreEqual(2, result["A"]);
        Assert.AreEqual(1, result["B"]);
    }

    [TestMethod]
    public void GetPatientsByRhShouldGroupCorrectly()
    {
        var patients = new List<Patient>
        {
            new Patient { MedicalHistory = new MedicalHistory { Rh = Rh.Positive } },
            new Patient { MedicalHistory = new MedicalHistory { Rh = Rh.Positive } },
            new Patient { MedicalHistory = new MedicalHistory { Rh = Rh.Negative } }
        };

        _patientRepo.Setup(r => r.GetAll(true)).Returns(patients);

        var result = _service.GetPatientsByRh();

        Assert.AreEqual(2, result["Positive"]);
        Assert.AreEqual(1, result["Negative"]);
    }

    [TestMethod]
    public void GetPatientGenderDistributionShouldCountCorrectly()
    {
        var patients = new List<Patient>
        {
            new Patient { Sex = Sex.M },
            new Patient { Sex = Sex.M },
            new Patient { Sex = Sex.F }
        };

        _patientRepo.Setup(r => r.GetAll(true)).Returns(patients);

        var result = _service.GetPatientGenderDistribution();

        Assert.AreEqual(2, result["M"]);
        Assert.AreEqual(1, result["F"]);
    }

    [TestMethod]
    public void GetConsultationDistributionShouldGroupCorrectly()
    {
        var records = new List<MedicalRecord>
        {
            new MedicalRecord { SourceType = SourceType.ER },
            new MedicalRecord { SourceType = SourceType.ER },
            new MedicalRecord { SourceType = SourceType.App }
        };

        _recordRepo.Setup(r => r.GetAll()).Returns(records);

        var result = _service.GetConsultationDistribution();

        Assert.AreEqual(2, result["ER"]);
        Assert.AreEqual(1, result["App"]);
    }

    [TestMethod]
    public void GetTopDiagnosesShouldNormalizeAndGroup()
    {
        var records = new List<MedicalRecord>
        {
            new MedicalRecord { Diagnosis = "Flu" },
            new MedicalRecord { Diagnosis = "flu " },
            new MedicalRecord { Diagnosis = "COVID" }
        };

        _recordRepo.Setup(r => r.GetAll()).Returns(records);

        var result = _service.GetTopDiagnoses();

        Assert.AreEqual(2, result["FLU"]);
        Assert.AreEqual(1, result["COVID"]);
    }

    [TestMethod]
    public void GetAgeDistributionShouldCategorizeCorrectly()
    {
        var patients = new List<Patient>
        {
            new Patient { Dob = DateTime.Today.AddYears(-10) }, 
            new Patient { Dob = DateTime.Today.AddYears(-30) }, 
            new Patient { Dob = DateTime.Today.AddYears(-70) }  
        };

        _patientRepo.Setup(r => r.GetAll(true)).Returns(patients);

        var result = _service.GetAgeDistribution();

        Assert.AreEqual(1, result["Pediatric (0-17)"]);
        Assert.AreEqual(1, result["Adult (18-64)"]);
        Assert.AreEqual(1, result["Geriatric (65+)"]);
    }

    [TestMethod]
    public void GetMostPrescribedMedsShouldAggregateCorrectly()
    {
        var prescriptions = new List<Prescription>
        {
            new Prescription
            {
                MedicationList = new List<PrescriptionItem>
                {
                    new PrescriptionItem { MedName = "Paracetamol" },
                    new PrescriptionItem { MedName = "paracetamol " }
                }
            }
        };

        _prescriptionRepo.Setup(r => r.GetAll()).Returns(prescriptions);

        var result = _service.GetMostPrescribedMeds();

        Assert.AreEqual(2, result["PARACETAMOL"]);
    }

    [TestMethod]
    public void GetActiveVsArchivedRatioShouldCountCorrectly()
    {
        var patients = new List<Patient>
        {
            new Patient { IsArchived = false },
            new Patient { IsArchived = false },
            new Patient { IsArchived = true }
        };

        _patientRepo.Setup(r => r.GetAll(true)).Returns(patients);

        var result = _service.GetActiveVsArchivedRatio();

        Assert.AreEqual(2, result["Active"]);
        Assert.AreEqual(1, result["Archived"]);
    }
}