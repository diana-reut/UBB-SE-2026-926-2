using System.Collections.Generic;
using System.Data.Common;
using HospitalManagement.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HospitalManagement.Database;

internal sealed partial class HospitalDbContext : IDbContext
{
    private readonly SqlConnection _connection;
    private SqlTransaction? _transaction;
    private readonly string _connectionString;

    public HospitalDbContext()
        : this(BuildCompatibilityConfiguration())
    {
    }

    public HospitalDbContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new DatabaseException("DefaultConnection is missing from appsettings.json.");

        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new DatabaseException("Connection string is not loaded.");
        }

        _connection = new SqlConnection
        {
            ConnectionString = _connectionString,
        };
        _connection.Open();
    }

    public DbDataReader ExecuteQuery(string sql)
    {
        EnsureConnectionOpen();
        var command = new SqlCommand(sql, _connection);

        if (_transaction is not null)
        {
            command.Transaction = _transaction;
        }

        try
        {
            return command.ExecuteReader();
        }
        catch
        {
            command.Dispose();
            throw;
        }
    }

    public int ExecuteNonQuery(string sql)
    {
        EnsureConnectionOpen();
        var command = new SqlCommand(sql, _connection);

        if (_transaction is not null)
        {
            command.Transaction = _transaction;
        }

        return command.ExecuteNonQuery();
    }

    public void BeginTransaction()
    {
        _transaction ??= _connection.BeginTransaction();
    }

    public void CommitTransaction()
    {
        if (_transaction is not null)
        {
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void RollbackTransaction()
    {
        if (_transaction is not null)
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _transaction = null;

        if (_connection.State == System.Data.ConnectionState.Open)
        {
            _connection.Close();
        }

        _connection.Dispose();
    }

    public void EnsureConnectionOpen()
    {
        if (_connection.ConnectionString is null)
        {
            return;
        }

        // Re-assign the configured value if the connection loses its string.
        if (string.IsNullOrEmpty(_connection.ConnectionString))
        {
            _connection.ConnectionString = _connectionString;
        }

        // Re-open the connection when a repository call needs it again.
        if (_connection.State != System.Data.ConnectionState.Open)
        {
            if (string.IsNullOrEmpty(_connection.ConnectionString))
            {
                throw new DatabaseException("CRITICAL: Connection string is still missing in EnsureConnectionOpen!");
            }

            _connection.Open();
        }
    }

    private static IConfiguration BuildCompatibilityConfiguration()
    {
        if (string.IsNullOrWhiteSpace(Config.ConnectionString))
        {
            Config.Load();
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("ConnectionStrings:DefaultConnection", Config.ConnectionString),
            ])
            .Build();
    }
}
