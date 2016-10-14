using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;

namespace JabbRCore.Data.InMemory
{
public class TestInMemoryTransactionManager : InMemoryTransactionManager
{
    private IDbContextTransaction _currentTransaction;

    public TestInMemoryTransactionManager(ILogger<InMemoryTransactionManager> logger)
        : base(logger)
    {
    }

    public override IDbContextTransaction CurrentTransaction => _currentTransaction;

    public override IDbContextTransaction BeginTransaction()
    {
        _currentTransaction = new TestInMemoryTransaction(this);
        return _currentTransaction;
    }

    public override Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        _currentTransaction = new TestInMemoryTransaction(this);
        return Task.FromResult(_currentTransaction);
    }

    public override void CommitTransaction() => CurrentTransaction.Commit();

    public override void RollbackTransaction() => CurrentTransaction.Rollback();

    private class TestInMemoryTransaction : IDbContextTransaction
    {
        public TestInMemoryTransaction(TestInMemoryTransactionManager transactionManager)
        {
            TransactionManager = transactionManager;
        }

        private TestInMemoryTransactionManager TransactionManager { get; }

        public void Dispose()
        {
            TransactionManager._currentTransaction = null;
        }

        public void Commit()
        {
            TransactionManager._currentTransaction = null;
        }

        public void Rollback()
        {
            TransactionManager._currentTransaction = null;
        }
    }
}
}
