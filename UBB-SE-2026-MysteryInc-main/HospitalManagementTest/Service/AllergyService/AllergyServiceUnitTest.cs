using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using HospitalManagement.Entity;
using HospitalManagement.Repository;
using HospitalManagement.Service;
using System.Collections.Generic;
using System.Linq;

namespace HospitalManagement.Tests.ServiceTests
{
    [TestClass]
    public class AllergyServiceTests
    {
        private Mock<IAllergyRepository> _repoMock = null!;
        private AllergyService _sut = null!;

        [TestInitialize]
        public void Setup()
        {
            _repoMock = new Mock<IAllergyRepository>();
            _sut = new AllergyService(_repoMock.Object);
        }

        [TestMethod]
        public void GetAllergies_CallsRepositoryMethod()
        {
            _sut.GetAllergies();
            _repoMock.Verify(r => r.GetAllergies(), Times.Once);
        }

        [TestMethod]
        public void GetAllergies_ReturnsNonNullList()
        {
            _repoMock.Setup(r => r.GetAllergies()).Returns(new List<Allergy>());
            var result = _sut.GetAllergies();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetAllergies_ReturnsCorrectItemCount()
        {
            var data = new List<Allergy> { new Allergy(), new Allergy() };
            _repoMock.Setup(r => r.GetAllergies()).Returns(data);
            var result = _sut.GetAllergies();

            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void GetAllergies_MapsFirstItemNameCorrectly()
        {
            var data = new List<Allergy> { new Allergy { AllergyName = "Peanuts" } };
            _repoMock.Setup(r => r.GetAllergies()).Returns(data);
            var result = _sut.GetAllergies().First();

            Assert.AreEqual("Peanuts", result.AllergyName);
        }

        [TestMethod]
        public void GetAllergies_MapsFirstItemIdCorrectly()
        {
            var data = new List<Allergy> { new Allergy { AllergyId = 101 } };
            _repoMock.Setup(r => r.GetAllergies()).Returns(data);
            var result = _sut.GetAllergies().First();

            Assert.AreEqual(101, result.AllergyId);
        }

        [TestMethod]
        public void GetAllergies_ReturnsEmpty_WhenRepositoryIsEmpty()
        {
            _repoMock.Setup(r => r.GetAllergies()).Returns(new List<Allergy>());
            var result = _sut.GetAllergies();

            Assert.IsFalse(result.Any());
        }
    }
}