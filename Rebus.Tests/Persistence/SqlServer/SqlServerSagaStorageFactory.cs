﻿using NUnit.Framework;
using Rebus.Persistence.SqlServer;
using Rebus.Sagas;
using Rebus.Tests.Contracts.Sagas;

namespace Rebus.Tests.Persistence.SqlServer
{
    [TestFixture, Category(Categories.SqlServer)]
    public class BasicLoadAndSaveAndFindOperations : BasicLoadAndSaveAndFindOperations<SqlServerSagaStorageFactory> { }

    [TestFixture, Category(Categories.SqlServer)]
    public class ConcurrencyHandling : ConcurrencyHandling<SqlServerSagaStorageFactory> { }

    [TestFixture, Category(Categories.SqlServer)]
    public class SagaIntegrationTests : SagaIntegrationTests<SqlServerSagaStorageFactory> { }

    public class SqlServerSagaStorageFactory : ISagaStorageFactory
    {
        const string IndexTableName = "RebusSagaIndex";
        const string DataTableName = "RebusSagaData";

        public ISagaStorage GetSagaStorage()
        {
            var storage = new SqlServerSagaStorage(new DbConnectionProvider(SqlTestHelper.ConnectionString), DataTableName, IndexTableName);

            storage.EnsureTablesAreCreated();

            return storage;
        }

        public void CleanUp()
        {
            SqlTestHelper.DropTable(IndexTableName);
            SqlTestHelper.DropTable(DataTableName);
        }
    }
}