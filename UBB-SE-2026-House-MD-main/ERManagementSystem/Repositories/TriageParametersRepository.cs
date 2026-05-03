using System;
using ERManagementSystem.Helpers;
using ERManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace ERManagementSystem.Repositories
{
    public class TriageParametersRepository : ITriageParametersRepository
    {
        private readonly SqlHelper sqlHelper;

        public TriageParametersRepository(SqlHelper sqlHelper)
        {
            this.sqlHelper = sqlHelper;
        }

        /// <summary>
        /// Inserts a new Triage_Parameters record into the database.
        /// </summary>
        public void Add(Triage_Parameters parameters)
        {
            string sql = @"INSERT INTO Triage_Parameters (Triage_ID, Consciousness, Breathing, Bleeding, Injury_Type, Pain_Level)
                           VALUES (@Triage_ID, @Consciousness, @Breathing, @Bleeding, @Injury_Type, @Pain_Level)";

            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@Triage_ID", parameters.Triage_ID),
                new SqlParameter("@Consciousness", parameters.Consciousness),
                new SqlParameter("@Breathing", parameters.Breathing),
                new SqlParameter("@Bleeding", parameters.Bleeding),
                new SqlParameter("@Injury_Type", parameters.Injury_Type),
                new SqlParameter("@Pain_Level", parameters.Pain_Level)
            };

            Logger.Info($"[TriageParametersRepository] Adding parameters for triage {parameters.Triage_ID}");

            try
            {
                sqlHelper.ExecuteNonQuery(sql, sqlParams);
                Logger.Info($"[TriageParametersRepository] Parameters saved for triage {parameters.Triage_ID}");
            }
            catch (Exception ex)
            {
                Logger.Error($"[TriageParametersRepository] Failed to insert parameters for triage {parameters.Triage_ID}", ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves Triage_Parameters by Triage_ID.
        /// Returns null if no record is found.
        /// </summary>
        public Triage_Parameters? GetByTriageId(int triageId)
        {
            string sql = "SELECT Triage_ID, Consciousness, Breathing, Bleeding, Injury_Type, Pain_Level FROM Triage_Parameters WHERE Triage_ID = @Triage_ID";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Triage_ID", triageId)
            };

            Logger.Info($"[TriageParametersRepository] Fetching parameters for triage {triageId}");

            try
            {
                using var reader = sqlHelper.ExecuteReader(sql, parameters);

                if (reader.Read())
                {
                    Logger.Info($"[TriageParametersRepository] Found parameters for triage {triageId}");

                    return new Triage_Parameters
                    {
                        Triage_ID = reader.GetInt32(reader.GetOrdinal("Triage_ID")),
                        Consciousness = reader.GetInt32(reader.GetOrdinal("Consciousness")),
                        Breathing = reader.GetInt32(reader.GetOrdinal("Breathing")),
                        Bleeding = reader.GetInt32(reader.GetOrdinal("Bleeding")),
                        Injury_Type = reader.GetInt32(reader.GetOrdinal("Injury_Type")),
                        Pain_Level = reader.GetInt32(reader.GetOrdinal("Pain_Level"))
                    };
                }

                Logger.Warning($"[TriageParametersRepository] No parameters found for triage {triageId}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error($"[TriageParametersRepository] Error fetching parameters for triage {triageId}", ex);
                throw;
            }
        }

        public void Delete(Triage_Parameters parameters)
        {
            if (parameters == null || parameters.Triage_ID <= 0)
            {
                throw new ArgumentException("Invalid Triage_Parameters object.");
            }

            string sql = "DELETE FROM Triage_Parameters WHERE Triage_ID = @Triage_ID";

            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@Triage_ID", parameters.Triage_ID)
            };

            sqlHelper.ExecuteNonQuery(sql, sqlParams);
        }
    }
}
