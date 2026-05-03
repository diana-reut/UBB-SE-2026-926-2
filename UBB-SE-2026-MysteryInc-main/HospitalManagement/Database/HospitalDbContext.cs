using Microsoft.Data.SqlClient;
using HospitalManagement.Configuration;
using System.Data.Common;

namespace HospitalManagement.Database;

internal sealed partial class HospitalDbContext : IDbContext
{
    private readonly SqlConnection _connection;
    private SqlTransaction? _transaction;

    public HospitalDbContext()
    {
        if (string.IsNullOrWhiteSpace(Config.ConnectionString))
        {
            throw new DatabaseException("Connection string is not loaded.");
        }

        _connection = new SqlConnection
        {
            ConnectionString = Config.ConnectionString,
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

        // 1. If the connection forgot its string, re-assign it from Config
        if (string.IsNullOrEmpty(_connection.ConnectionString))
        {
            _connection.ConnectionString = Config.ConnectionString;
        }

        // 2. Check the state. If it's not open, try to open it.
        if (_connection.State != System.Data.ConnectionState.Open)
        {
            // 3. Double-check again just to be 100% sure we have a string now
            if (string.IsNullOrEmpty(_connection.ConnectionString))
            {
                throw new DatabaseException("CRITICAL: Connection string is still missing in EnsureConnectionOpen!");
            }

            _connection.Open();
        }
    }
}
