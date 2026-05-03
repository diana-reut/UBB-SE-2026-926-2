using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ERManagementSystem.Helpers
{
    public static class Logger
    {
        private static readonly object SyncLock = new object();

        private static string LogDirectory =>
            Path.Combine(AppContext.BaseDirectory, "Logs");

        private static string LogFilePath =>
            Path.Combine(LogDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");

        public static void Info(string message)
        {
            Write(LogLevel.Info, message);
        }

        public static void Warning(string message)
        {
            Write(LogLevel.Warning, message);
        }

        public static void Error(string message, Exception? exception = null)
        {
            var fullMessage = exception == null
                ? message
                : $"{message}{Environment.NewLine}{exception}";

            Write(LogLevel.Error, fullMessage);
        }

        private static void Write(LogLevel level, string message)
        {
            try
            {
                lock (SyncLock)
                {
                    Directory.CreateDirectory(LogDirectory);

                    var logEntry = new StringBuilder()
                        .Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("] ")
                        .Append('[').Append(level.ToString().ToUpper()).Append("] ")
                        .AppendLine(message)
                        .ToString();

                    File.AppendAllText(LogFilePath, logEntry);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Logger] Failed to write log entry: {ex}");
                Trace.WriteLine($"[Logger] Failed to write log entry: {ex}");
            }
        }
    }
}
/* Usage examples:
Logger.Info($"Registering patient {patient.Patient_ID}");
Logger.Warning($"Visit {visitId} has no available matching room.");
Logger.Error("Database query failed in PatientRepository.GetById.", ex);
 */
