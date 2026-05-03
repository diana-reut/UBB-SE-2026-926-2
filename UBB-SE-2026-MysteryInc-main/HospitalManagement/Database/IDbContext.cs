using System;
using System.Data.Common;

namespace HospitalManagement.Database;

internal interface IDbContext : IDisposable
{
    public void BeginTransaction();

    public void CommitTransaction();

    public new void Dispose();

    public void EnsureConnectionOpen();

    public int ExecuteNonQuery(string sql);

    public DbDataReader ExecuteQuery(string sql);

    public void RollbackTransaction();
}
