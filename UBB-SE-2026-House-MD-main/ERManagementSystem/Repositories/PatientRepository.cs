using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;

namespace ERManagementSystem.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly SqlHelper sqlHelper;

        public PatientRepository(SqlHelper sqlHelper)
        {
            this.sqlHelper = sqlHelper;
        }

        public void Add(Patient patient)
        {
            const string query = @"
                INSERT INTO dbo.Patient
                    (Patient_ID, First_Name, Last_Name, Date_of_Birth,
                     Gender, Phone, Emergency_Contact, Transferred)
                VALUES
                    (@Patient_ID, @First_Name, @Last_Name, @Date_of_Birth,
                     @Gender, @Phone, @Emergency_Contact, @Transferred)";

            var parameters = new[]
            {
                new SqlParameter("@Patient_ID",        patient.Patient_ID),
                new SqlParameter("@First_Name",        patient.First_Name),
                new SqlParameter("@Last_Name",         patient.Last_Name),
                new SqlParameter("@Date_of_Birth",     patient.Date_of_Birth),
                new SqlParameter("@Gender",            patient.Gender),
                new SqlParameter("@Phone",             patient.Phone),
                new SqlParameter("@Emergency_Contact", patient.Emergency_Contact),
                new SqlParameter("@Transferred",       patient.Transferred)
            };

            try
            {
                sqlHelper.ExecuteNonQuery(query, parameters);
                Logger.Info($"Patient {patient.Patient_ID} ({patient.First_Name} {patient.Last_Name}) added to DB.");
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in PatientRepository.Add for Patient {patient.Patient_ID}.", ex);
                throw;
            }
        }

        public Patient? GetById(string id)
        {
            const string query = @"
                SELECT Patient_ID, First_Name, Last_Name, Date_of_Birth,
                       Gender, Phone, Emergency_Contact, Transferred
                FROM   dbo.Patient
                WHERE  Patient_ID = @Patient_ID";

            var parameters = new[]
            {
                new SqlParameter("@Patient_ID", id)
            };

            try
            {
                using var reader = sqlHelper.ExecuteReader(query, parameters);
                if (reader.Read())
                {
                    var patient = MapReaderToPatient(reader);
                    Logger.Info($"Patient {id} retrieved from DB.");
                    return patient;
                }

                Logger.Warning($"GetById: Patient {id} not found in DB.");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in PatientRepository.GetById for Patient {id}.", ex);
                throw;
            }
        }

        public void Update(Patient patient)
        {
            const string query = @"
                UPDATE dbo.Patient
                SET    First_Name        = @First_Name,
                       Last_Name         = @Last_Name,
                       Date_of_Birth     = @Date_of_Birth,
                       Gender            = @Gender,
                       Phone             = @Phone,
                       Emergency_Contact = @Emergency_Contact,
                       Transferred       = @Transferred
                WHERE  Patient_ID = @Patient_ID";

            var parameters = new[]
            {
                new SqlParameter("@Patient_ID",        patient.Patient_ID),
                new SqlParameter("@First_Name",        patient.First_Name),
                new SqlParameter("@Last_Name",         patient.Last_Name),
                new SqlParameter("@Date_of_Birth",     patient.Date_of_Birth),
                new SqlParameter("@Gender",            patient.Gender),
                new SqlParameter("@Phone",             patient.Phone),
                new SqlParameter("@Emergency_Contact", patient.Emergency_Contact),
                new SqlParameter("@Transferred",       patient.Transferred)
            };

            try
            {
                sqlHelper.ExecuteNonQuery(query, parameters);
                Logger.Info($"Patient {patient.Patient_ID} updated in DB.");
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in PatientRepository.Update for Patient {patient.Patient_ID}.", ex);
                throw;
            }
        }

        public void Delete(Patient patient)
        {
            const string query = @"
                DELETE FROM dbo.Patient
                WHERE Patient_ID = @Patient_ID";

            var parameters = new[]
            {
                new SqlParameter("@Patient_ID", patient.Patient_ID)
            };

            try
            {
                sqlHelper.ExecuteNonQuery(query, parameters);
                Logger.Info($"Patient {patient.Patient_ID} deleted from DB.");
            }
            catch (Exception ex)
            {
                Logger.Error($"DB error in PatientRepository.Delete for Patient {patient.Patient_ID}.", ex);
                throw;
            }
        }

        private Patient MapReaderToPatient(SqlDataReader reader)
        {
            return new Patient
            {
                Patient_ID = reader["Patient_ID"] as string ?? string.Empty,
                First_Name = reader["First_Name"] as string ?? string.Empty,
                Last_Name = reader["Last_Name"] as string ?? string.Empty,
                Date_of_Birth = Convert.ToDateTime(reader["Date_of_Birth"]),
                Gender = reader["Gender"] as string ?? string.Empty,
                Phone = reader["Phone"] as string ?? string.Empty,
                Emergency_Contact = reader["Emergency_Contact"] as string ?? string.Empty,
                Transferred = Convert.ToBoolean(reader["Transferred"])
            };
        }
    }
}