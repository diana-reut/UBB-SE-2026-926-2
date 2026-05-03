using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HospitalManagement.Entity;
using HospitalManagement.Repository;
using HospitalManagement.Service;
using HospitalManagement.Entity.Enums;

namespace HospitalManagementTest.Tests.UnitTests;

[TestClass]
public class BillingServiceTest
{
    private Mock<IMedicalHistoryRepository> _historyRepo = null!;
    private Mock<IMedicalRecordRepository> _recordRepo = null!;
    private Mock<IPrescriptionRepository> _prescriptionRepo = null!;
    private Mock<ITransplantRepository> _transplantRepo = null!;

    private BillingService CreateService()
    {
        return new BillingService(
            _historyRepo.Object,
            _recordRepo.Object,
            _prescriptionRepo.Object,
            _transplantRepo.Object);
    }

    [TestInitialize]
    public void Setup()
    {
        _historyRepo = new Mock<IMedicalHistoryRepository>();
        _recordRepo = new Mock<IMedicalRecordRepository>();
        _prescriptionRepo = new Mock<IPrescriptionRepository>();
        _transplantRepo = new Mock<ITransplantRepository>();
    }

    [TestMethod]
    public void ComputeBasePrice_ShouldReturnZero_WhenHistoryIsNull()
    {
        _recordRepo
            .Setup(x => x.GetById(10))
            .Returns(new MedicalRecord { Id = 10, SourceType = SourceType.ER });

        _prescriptionRepo
            .Setup(x => x.GetByRecordId(10))
            .Returns((Prescription?)null);

        _historyRepo
            .Setup(x => x.GetByPatientId(1))
            .Returns((MedicalHistory?)null);

        var service = CreateService();

        var result = service.ComputeBasePrice(1, 10);

        Assert.AreEqual(0m, result);
    }

    [TestMethod]
    public void ComputeBasePrice_ShouldReturnZero_WhenRecordIsNull()
    {
        _recordRepo
            .Setup(x => x.GetById(10))
            .Returns((MedicalRecord?)null);

        _prescriptionRepo
            .Setup(x => x.GetByRecordId(10))
            .Returns((Prescription?)null);

        _historyRepo
            .Setup(x => x.GetByPatientId(1))
            .Returns(new MedicalHistory { Id = 5 });

        var service = CreateService();

        var result = service.ComputeBasePrice(1, 10);

        Assert.AreEqual(0m, result);
    }

    [TestMethod]
    public void ComputeBasePrice_ShouldCalculatePrice_ForApp_WhenPrescriptionIsNull()
    {
        _recordRepo
            .Setup(x => x.GetById(10))
            .Returns(new MedicalRecord
            {
                Id = 10,
                SourceType = SourceType.App
            });

        _prescriptionRepo
               .Setup(x => x.GetByRecordId(10))
               .Returns((Prescription?)null);

        _historyRepo
            .Setup(x => x.GetByPatientId(1))
            .Returns(new MedicalHistory { Id = 5 });

        _historyRepo
            .Setup(x => x.GetChronicConditions(5))
            .Returns(new List<string> { "Hypertension" });

        _historyRepo
            .Setup(x => x.GetAllergiesByHistoryId(5))
            .Returns(new List<(Allergy Allergy, string SeverityLevel)>
            {
                    (new Allergy(), "moderate")
            });

        _transplantRepo
            .Setup(x => x.GetByReceiverId(1))
            .Returns(new List<Transplant>());

        var service = CreateService();

        var result = service.ComputeBasePrice(1, 10);
        Assert.AreEqual(320m, result);
    }

    [TestMethod]
    public void ComputeBasePrice_ShouldCalculatefullPrice_ForER_WithPrescriptionItems_ChronicConditions_SevereAllergy_AndTransplant()
    {
        _recordRepo
            .Setup(x => x.GetById(10))
            .Returns(new MedicalRecord
            {
                Id = 10,
                SourceType = SourceType.ER
            });

        _prescriptionRepo
            .Setup(x => x.GetByRecordId(10))
            .Returns(new Prescription
            {
                Id = 100,
                RecordId = 10
            });

        _prescriptionRepo
            .Setup(x => x.GetItems(100))
            .Returns(new List<PrescriptionItem>
            {
            new PrescriptionItem(),
            new PrescriptionItem()
            });

        _historyRepo
            .Setup(x => x.GetByPatientId(1))
            .Returns(new MedicalHistory
            {
                Id = 5
            });

        _historyRepo
            .Setup(x => x.GetChronicConditions(5))
            .Returns(new List<string>
            {
            "Diabetes",
            "Asthma"
            });

        _historyRepo
            .Setup(x => x.GetAllergiesByHistoryId(5))
            .Returns(new List<(Allergy Allergy, string SeverityLevel)>
            {
            (new Allergy(), "severe")
            });

        _transplantRepo
            .Setup(x => x.GetByReceiverId(1))
            .Returns(new List<Transplant>
            {
            new Transplant()
            });

        var service = CreateService();

        var result = service.ComputeBasePrice(1, 10);
        Assert.AreEqual(2900m, result);
    }

    [TestMethod]
    public void ComputeBasePrice_ShouldIgnoreUnknownSeverity_AndOtherSourceType()
    {
        _recordRepo
            .Setup(x => x.GetById(10))
            .Returns(new MedicalRecord
            {
                Id = 10,
                SourceType = SourceType.Admin
            });

        _prescriptionRepo
            .Setup(x => x.GetByRecordId(10))
            .Returns(new Prescription
            {
                Id = 100,
                RecordId = 10
            });

        _prescriptionRepo
            .Setup(x => x.GetItems(100))
            .Returns(new List<PrescriptionItem>
            {
                    new PrescriptionItem()
            });

        _historyRepo
            .Setup(x => x.GetByPatientId(1))
            .Returns(new MedicalHistory { Id = 5 });

        _historyRepo
            .Setup(x => x.GetChronicConditions(5))
            .Returns(new List<string>());

        _historyRepo
            .Setup(x => x.GetAllergiesByHistoryId(5))
            .Returns(new List<(Allergy Allergy, string SeverityLevel)>
            {
                    (new Allergy(), "unknown")
            });

        _transplantRepo
            .Setup(x => x.GetByReceiverId(1))
            .Returns(new List<Transplant>());

        var service = CreateService();

        var result = service.ComputeBasePrice(1, 10);

        Assert.AreEqual(50m, result);
    }


    [TestMethod]
    public void ComputeBasePrice_ShouldTreatSeverityCaseInsensitively_ForAnaphylactic()
    {
        _recordRepo
            .Setup(x => x.GetById(10))
            .Returns(new MedicalRecord
            {
                Id = 10,
                SourceType = SourceType.App
            });

        _prescriptionRepo
               .Setup(x => x.GetByRecordId(10))
               .Returns((Prescription?)null);

        _historyRepo
            .Setup(x => x.GetByPatientId(1))
            .Returns(new MedicalHistory { Id = 5 });


        _historyRepo
               .Setup(x => x.GetByPatientId(1))
               .Returns(new MedicalHistory { Id = 5 });

        _historyRepo
               .Setup(x => x.GetChronicConditions(5))
               .Returns(new List<string>());
        _historyRepo
                .Setup(x => x.GetAllergiesByHistoryId(5))
                .Returns(new List<(Allergy Allergy, string SeverityLevel)>
                {
                    (new Allergy(), "AnApHyLaCtIc")
                });


        _transplantRepo
            .Setup(x => x.GetByReceiverId(1))
            .Returns(new List<Transplant>());

        var service = CreateService();
        var result = service.ComputeBasePrice(1, 10);

        
        Assert.AreEqual(300m, result);
    }

    [TestMethod]
    public void ComputeBasePrice_ShouldAddTwenty_WhenAllergySeverityIsMild_IDK()
    {
        _recordRepo
            .Setup(x => x.GetById(10))
            .Returns(new MedicalRecord
            {
                Id = 10,
                SourceType = SourceType.Admin
            });

        _prescriptionRepo
            .Setup(x => x.GetByRecordId(10))
            .Returns((Prescription?)null);

        _historyRepo
            .Setup(x => x.GetByPatientId(1))
            .Returns(new MedicalHistory
            {
                Id = 5
            });

        _historyRepo
            .Setup(x => x.GetChronicConditions(5))
            .Returns(new List<string>());

        _historyRepo
            .Setup(x => x.GetAllergiesByHistoryId(5))
            .Returns(new List<(Allergy Allergy, string SeverityLevel)>
            {
            (new Allergy(), "mild")
            });

        _transplantRepo
            .Setup(x => x.GetByReceiverId(1))
            .Returns(new List<Transplant>());

        var service = CreateService();

        var result = service.ComputeBasePrice(1, 10);

        Assert.AreEqual(20m, result);
    }




    [TestMethod]
    public void ApplyDiscount_ShouldReducePriceByPercentage()
    {
        var service = CreateService();

        var result = service.ApplyDiscount(1000m, 25);

        Assert.AreEqual(750m, result);
    }




}
