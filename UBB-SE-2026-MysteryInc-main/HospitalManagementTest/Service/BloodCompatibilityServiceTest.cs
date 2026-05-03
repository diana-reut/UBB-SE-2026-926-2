using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalManagement.Entity;
using HospitalManagement.Entity.Enums;
using HospitalManagement.Repository;
using HospitalManagement.Service;
using Moq;

namespace HospitalManagementTest.Tests.UnitTests;


[TestClass]
public class BloodCompatibilityServiceTest
{
    private Mock<IPatientRepository> _patientRepo = null!;
    private Mock<IMedicalHistoryRepository> _historyRepo = null!;

    private BloodCompatibilityService CreateService()
    {
        return new BloodCompatibilityService(_patientRepo.Object, _historyRepo.Object);
    }

    [TestInitialize]
    public void Setup()
    {
        _patientRepo = new Mock<IPatientRepository>();
        _historyRepo = new Mock<IMedicalHistoryRepository>();
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldReturnEmpty_WhenRecipientIsNull()
    {
        _patientRepo.Setup(x => x.GetById(1)).Returns((Patient?)null);
        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldReturnEmpty_WhenRecipientHistoryIsNull()
    {
        var recipient = new Patient { Id = 1 };
        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);
        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns((MedicalHistory?)null);
        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldReturnEmpty_WhenRecipientBloodTypeIsNull()
    {
        var recipient = new Patient { Id = 1 };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);
        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = null,
            Rh = Rh.Positive
        });

        var service = CreateService();

        var result = service.GetTopCompatibleDonors(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldReturnEmpty_WhenRecipientRhIsNull()
    {
        var recipient = new Patient { Id = 1 };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);
        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = null
        });

        var service = CreateService();

        var result = service.GetTopCompatibleDonors(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldSkipRecipient_isHimself()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);
        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive
        });

        _patientRepo.Setup(x => x.GetAll(false)).Returns(new List<Patient>
            {
                new Patient
                {
                    Id = 1,
                    IsDonor = true,
                    Dob = new DateTime(1990, 1, 1),
                    Sex = Sex.M
                }
            });

        var service = CreateService();

        var result = service.GetTopCompatibleDonors(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldSkip_WhenPatient_isNotDonor()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        var nonDonor = new Patient
        {
            Id = 2,
            IsDonor = false,
            Dob = new DateTime(1991, 1, 1),
            Sex = Sex.F
        };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);
        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive
        });

        _patientRepo.Setup(x => x.GetAll(false)).Returns(new List<Patient> { nonDonor });
        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldSkip_Whendonor_history_isNull()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        var donor = new Patient
        {
            Id = 2,
            IsDonor = true,
            Dob = new DateTime(1991, 1, 1),
            Sex = Sex.F
        };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);

        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive
        });

        _patientRepo.Setup(x => x.GetAll(false)).Returns(new List<Patient> { donor });
        _historyRepo.Setup(x => x.GetByPatientId(2)).Returns((MedicalHistory?)null);

        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldSkip_WhendonorbloodType_isNull()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        var donor = new Patient
        {
            Id = 2,
            IsDonor = true,
            Dob = new DateTime(1991, 1, 1),
            Sex = Sex.F
        };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);
        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive
        });
        _patientRepo.Setup(x => x.GetAll(false)).Returns(new List<Patient> { donor });
        _historyRepo.Setup(x => x.GetByPatientId(2)).Returns(new MedicalHistory
        {
            BloodType = null,
            Rh = Rh.Positive
        });

        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);
        Assert.AreEqual(0, result.Count);
    }



    [TestMethod]
    public void CalculateScore_ShouldReturnZero_WhenDonorIsNull()
    {
        var recipient = new Patient();
        var service = CreateService();
        var result = service.CalculateScore(null!, recipient);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_Should_Skip_Whendonor_Rh_IsNull()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        var donor = new Patient
        {
            Id = 2,
            IsDonor = true,
            Dob = new DateTime(1991, 1, 1),
            Sex = Sex.F
        };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);

        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive
        });

        _patientRepo.Setup(x => x.GetAll(false)).Returns(new List<Patient> { donor });

        _historyRepo.Setup(x => x.GetByPatientId(2)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = null
        });

        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldSkip_when_Blood_TypeDoesNotMatch()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        var donor = new Patient
        {
            Id = 2,
            IsDonor = true,
            Dob = new DateTime(1991, 1, 1),
            Sex = Sex.F
        };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);

        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive
        });

        _patientRepo.Setup(x => x.GetAll(false)).Returns(new List<Patient> { donor });

        _historyRepo.Setup(x => x.GetByPatientId(2)).Returns(new MedicalHistory
        {
            BloodType = BloodType.B,
            Rh = Rh.Positive
        });

        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);
        Assert.AreEqual(0, result.Count);
    }


    [TestMethod]
    public void CalculateScore_ShouldReturnZero_WhenRecipientIsNull()
    {
        var donor = new Patient();
        var service = CreateService();
        var result = service.CalculateScore(donor, null!);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void CalculateScore_ShouldReturnZero_Whensonor_historyisNull()
    {
        var donor = new Patient { MedicalHistory = null };
        var recipient = new Patient { MedicalHistory = new MedicalHistory() };
        var service = CreateService();
        var result = service.CalculateScore(donor, recipient);
        Assert.AreEqual(0, result);
    }



    [TestMethod]
    public void CalculateScore_ShouldNotAddNegativeAgePoints()
    {
        var donor = new Patient
        {
            Dob = new DateTime(1940, 1, 1),
            Sex = Sex.F,
            MedicalHistory = new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = Rh.Positive
            }
        };

        var recipient = new Patient
        {
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M,
            MedicalHistory = new MedicalHistory
            {
                BloodType = BloodType.O,
                Rh = Rh.Negative
            }
        };

        var service = CreateService();
        var result = service.CalculateScore(donor, recipient);
        
        Assert.AreEqual(35, result);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldSkip_WhenRhDoesNotMatch()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        var donor = new Patient
        {
            Id = 2,
            IsDonor = true,
            Dob = new DateTime(1991, 1, 1),
            Sex = Sex.F
        };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);
        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Negative
        });

        _patientRepo.Setup(x => x.GetAll(false)).Returns(new List<Patient> { donor });
        _historyRepo.Setup(x => x.GetByPatientId(2)).Returns(new MedicalHistory
        {
            BloodType = BloodType.O,
            Rh = Rh.Positive
        });

        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldSkip_WhenDonorHasChronicConditions()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        var donor = new Patient
        {
            Id = 2,
            IsDonor = true,
            Dob = new DateTime(1991, 1, 1),
            Sex = Sex.F
        };
        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);

        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive
        });

        _patientRepo.Setup(x => x.GetAll(false)).Returns(new List<Patient> { donor });

        _historyRepo.Setup(x => x.GetByPatientId(2)).Returns(new MedicalHistory
        {
            BloodType = BloodType.O,
            Rh = Rh.Positive,
            ChronicConditions = new List<string> { "Diabetes" }
        });
        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldSkip_WhenDonorHasAnaphylacticAllergy()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        var donor = new Patient
        {
            Id = 2,
            IsDonor = true,
            Dob = new DateTime(1991, 1, 1),
            Sex = Sex.F
        };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);

        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive
        });

        _patientRepo.Setup(x => x.GetAll(false)).Returns(new List<Patient> { donor });

        _historyRepo.Setup(x => x.GetByPatientId(2)).Returns(new MedicalHistory
        {
            BloodType = BloodType.O,
            Rh = Rh.Positive,
            ChronicConditions = new List<string>(),
            Allergies = new List<(Allergy Allergy, string SeverityLevel)>
                {
                    (new Allergy(), "Anaphylactic")
                }
        });

        var service = CreateService();

        var result = service.GetTopCompatibleDonors(1);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldReturnRankedCompatibleDonors()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        var donor1 = new Patient
        {
            Id = 2,
            IsDonor = true,
            Dob = new DateTime(1991, 1, 1),
            Sex = Sex.M
        };

        var donor2 = new Patient
        {
            Id = 3,
            IsDonor = true,
            Dob = new DateTime(1970, 1, 1),
            Sex = Sex.F
        };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);
        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive
        });
        _patientRepo.Setup(x => x.GetAll(false)).Returns(new List<Patient> { donor1, donor2 });
        _historyRepo.Setup(x => x.GetByPatientId(2)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive,
            ChronicConditions = new List<string>(),
            Allergies = new List<(Allergy Allergy, string SeverityLevel)>()
        });

        _historyRepo.Setup(x => x.GetByPatientId(3)).Returns(new MedicalHistory
        {
            BloodType = BloodType.O,
            Rh = Rh.Positive,
            ChronicConditions = new List<string>(),
            Allergies = new List<(Allergy Allergy, string SeverityLevel)>()
        });

        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(2, result[0].Id);
        Assert.AreEqual(3, result[1].Id);
    }

    [TestMethod]
    public void GetTopCompatibleDonors_ShouldTakeOnlyTop20()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        var donors = Enumerable.Range(2, 25)
            .Select(i => new Patient
            {
                Id = i,
                IsDonor = true,
                Dob = new DateTime(1990, 1, 1),
                Sex = Sex.M
            })
            .ToList();
        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);
        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive
        });
        _patientRepo.Setup(x => x.GetAll(false)).Returns(donors);

        foreach (var donor in donors)
        {
            _historyRepo.Setup(x => x.GetByPatientId(donor.Id)).Returns(new MedicalHistory
            {
                BloodType = BloodType.A,
                Rh = Rh.Positive,
                ChronicConditions = new List<string>(),
                Allergies = new List<(Allergy Allergy, string SeverityLevel)>()
            });
        }

        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);
        Assert.AreEqual(20, result.Count);
    }


    [TestMethod]
    public void IsBloodMatch_ShouldReturnFalse_WhenDonorIsNull()
    {
        var service = CreateService();
        var result = service.IsBloodMatch(null, BloodType.A);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsBloodMatch_ShouldReturnTrue_WhenDonorIsO()
    {
        var service = CreateService();
        Assert.IsTrue(service.IsBloodMatch(BloodType.O, BloodType.A));
    }

    [TestMethod]
    public void IsBloodMatch_ShouldReturnTrue_WhenDonorIsA_AndReceiverIsA()
    {
        var service = CreateService();
        Assert.IsTrue(service.IsBloodMatch(BloodType.A, BloodType.A));
    }

    [TestMethod]
    public void IsBloodMatch_ShouldReturnTrue_WhenDonorIsA_AndReceiverIsAB()
    {
        var service = CreateService();
        Assert.IsTrue(service.IsBloodMatch(BloodType.A, BloodType.AB));
    }

    [TestMethod]
    public void IsBloodMatch_ShouldReturnTrue_WhenDonorIsB_AndreceiverisB()
    {
        var service = CreateService();
        Assert.IsTrue(service.IsBloodMatch(BloodType.B, BloodType.B));
    }

    [TestMethod]
    public void IsBloodMatch_ShouldReturnFalse_WhenDonorIsA_AndReceiverIsIncompatible()
    {
        var service = CreateService();
        Assert.IsFalse(service.IsBloodMatch(BloodType.A, BloodType.B));
    }

    [TestMethod]
    public void IsBloodMatch_ShouldReturnTrue_WhenDonorIsB_AndreceiverisAB()
    {
        var service = CreateService();
        Assert.IsTrue(service.IsBloodMatch(BloodType.B, BloodType.AB));
    }

    [TestMethod]
    public void IsBloodMatch_ShouldReturnTrue_WhenDonorIsAB_AndReceiverIsAB()
    {
        var service = CreateService();
        Assert.IsTrue(service.IsBloodMatch(BloodType.AB, BloodType.AB));
    }

    [TestMethod]
    public void IsBloodMatch_ShouldReturnFalse_When_typesAreIncompatible()
    {
        var service = CreateService();
        Assert.IsFalse(service.IsBloodMatch(BloodType.AB, BloodType.A));
    }

    [TestMethod]
    public void IsRhMatch_ShouldReturnFalse_WhenDonorIsNull()
    {
        var service = CreateService();
        Assert.IsFalse(service.IsRhMatch(null, Rh.Positive));
    }

    [TestMethod]
    public void IsRhMatch_ShouldReturnTrue_WhenReceiverNegative_AnddonordsNegative()
    {
        var service = CreateService();
        Assert.IsTrue(service.IsRhMatch(Rh.Negative, Rh.Negative));
    }

    [TestMethod]
    public void IsRhMatch_ShouldReturnFalse_WhenReceiverNegative_AnddonordsPositive()
    {
        var service = CreateService();
        Assert.IsFalse(service.IsRhMatch(Rh.Positive, Rh.Negative));
    }

    [TestMethod]
    public void IsRhMatch_ShouldReturnTrue_WhenReceiverIsPositive()
    {
        var service = CreateService();
        Assert.IsTrue(service.IsRhMatch(Rh.Negative, Rh.Positive));
        Assert.IsTrue(service.IsRhMatch(Rh.Positive, Rh.Positive));
    }

    [TestMethod]
    public void GetTopCompatibleDonors_shouldallowDonor_WhenChronicConditions_AndAllergies_AreNull()
    {
        var recipient = new Patient
        {
            Id = 1,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        var donor = new Patient
        {
            Id = 2,
            IsDonor = true,
            Dob = new DateTime(1990, 1, 1),
            Sex = Sex.M
        };

        _patientRepo.Setup(x => x.GetById(1)).Returns(recipient);
        _historyRepo.Setup(x => x.GetByPatientId(1)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive
        });

        _patientRepo.Setup(x => x.GetAll(false)).Returns(new List<Patient> { donor });

        _historyRepo.Setup(x => x.GetByPatientId(2)).Returns(new MedicalHistory
        {
            BloodType = BloodType.A,
            Rh = Rh.Positive,
            ChronicConditions = null,
            Allergies = null
        });

        var service = CreateService();
        var result = service.GetTopCompatibleDonors(1);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(2, result[0].Id);
    }


}
