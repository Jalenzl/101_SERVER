using Admin.NET.Core;
using Admin.NET.Core101.Entity;
using Admin.NET.Core101.Seed;
using Npgsql;
using SqlSugar;
using Xunit;

namespace Admin.NET.Application101.Tests.Integration;

public sealed class PostgreSqlInitializationTests
{
    private static readonly Type[] EntityTypes =
    {
        typeof(Task101), typeof(Person101), typeof(Device101), typeof(Document101), typeof(StoredFile101),
        typeof(TaskPlan101), typeof(TaskPerson101), typeof(TaskDevice101), typeof(TaskTransferRecord101),
        typeof(TaskDocument101), typeof(WorkflowNode101), typeof(TaskWorkflow101), typeof(Operation101),
        typeof(OperationCheck101), typeof(OperationSignature101)
    };

    [PostgreSqlFact]
    public async Task CodeFirstAndSeeds_AreIdempotent_AndTransferJsonUsesJsonb()
    {
        var password = Environment.GetEnvironmentVariable("TCP101_DB_PASSWORD");
        Assert.False(string.IsNullOrWhiteSpace(password), "TCP101_DB_PASSWORD is required when PostgreSQL tests are enabled.");

        var schema = "test_" + Guid.NewGuid().ToString("N");
        var adminConnection = $"Host=localhost;Port=5432;Username=postgres;Password={password};Database=tcp101";
        await using var connection = new NpgsqlConnection(adminConnection);
        await connection.OpenAsync();
        try
        {
            await ExecuteAsync(connection, $"CREATE SCHEMA \"{schema}\"");
            var config = new DbConnectionConfig
            {
                ConfigId = SqlSugarConst.MainConfigId,
                DbType = DbType.PostgreSQL,
                ConnectionString = adminConnection + $";Search Path={schema}",
                IsAutoCloseConnection = true,
                DbSettings = new DbSettings { EnableUnderLine = true },
                TableSettings = new TableSettings(), SeedSettings = new SeedSettings()
            };
            SqlSugarSetup.SetDbConfig(config);
            using var database = new SqlSugarClient(config);
            database.CodeFirst.InitTables(EntityTypes);

            Assert.Equal(EntityTypes.Length, await TableCountAsync(connection, schema));
            await SeedAsync(database);
            var before = Counts(database);
            await database.Updateable<Person101>().SetColumns(item => item.Name == "用户修改后姓名")
                .Where(item => item.Id == SeedIds101.Person1).ExecuteCommandAsync();
            await SeedAsync(database);
            Assert.Equal(before, Counts(database));
            Assert.Equal("用户修改后姓名", await database.Queryable<Person101>()
                .Where(item => item.Id == SeedIds101.Person1).Select(item => item.Name).FirstAsync());

            var indexes = await UniqueIndexesAsync(connection, schema);
            var expectedIndexes = new[] { "ux_t101_device_code", "ux_t101_document_code", "ux_t101_task_person",
                "ux_t101_task_device", "ux_t101_task_document", "ux_t101_task_workflow", "ux_t101_task_transfer",
                "ux_t101_operation_check", "ux_t101_operation_signature" };
            Assert.All(expectedIndexes, name => Assert.Contains(name, indexes));

            var transfer = new TaskTransferRecord101
            {
                TaskId = SeedIds101.TaskGg1101, TableId = "upper-thrust", OrderNo = 99,
                DataJson = """{"parameter":"jsonb-check","unit":"N"}"""
            };
            await database.Insertable(transfer).ExecuteCommandAsync();
            await using var typeCommand = new NpgsqlCommand(
                $"SELECT pg_typeof(data_json)::text FROM \"{schema}\".t101_task_transfer_record WHERE id = @id", connection);
            typeCommand.Parameters.AddWithValue("id", transfer.Id);
            Assert.Equal("jsonb", (string?)await typeCommand.ExecuteScalarAsync());
            Assert.Equal(transfer.DataJson, (await database.Queryable<TaskTransferRecord101>()
                .FirstAsync(item => item.Id == transfer.Id)).DataJson);

            var duplicate = new Device101 { Code = MasterDataSeed101.Devices[0].Code, Name = "重复编号" };
            await Assert.ThrowsAnyAsync<Exception>(() => database.Insertable(duplicate).ExecuteCommandAsync());
        }
        finally
        {
            if (!schema.StartsWith("test_", StringComparison.Ordinal) || schema.Length != 37)
                throw new InvalidOperationException("Refusing to drop an unexpected schema name.");
            await ExecuteAsync(connection, $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE");
        }
    }

    private static async Task SeedAsync(ISqlSugarClient database)
    {
        await InsertWhenEmpty(database, MasterDataSeed101.People);
        await InsertWhenEmpty(database, MasterDataSeed101.Devices);
        await InsertWhenEmpty(database, MasterDataSeed101.StoredFiles);
        await InsertWhenEmpty(database, MasterDataSeed101.Documents);
        await InsertWhenEmpty(database, MasterDataSeed101.WorkflowNodes);
        await InsertWhenEmpty(database, TaskSeed101.Tasks);
        await InsertWhenEmpty(database, TaskSeed101.Plans);
        await InsertWhenEmpty(database, TaskSeed101.TaskPeople);
        await InsertWhenEmpty(database, TaskSeed101.TaskDevices);
        await InsertWhenEmpty(database, TaskSeed101.TaskDocuments);
        await InsertWhenEmpty(database, TaskSeed101.TaskWorkflows);
        await InsertWhenEmpty(database, TaskSeed101.TransferRecords);
    }

    private static async Task InsertWhenEmpty<TEntity>(ISqlSugarClient database, IReadOnlyList<TEntity> rows)
        where TEntity : class, new()
    {
        if (!await database.Queryable<TEntity>().AnyAsync()) await database.Insertable(rows.ToList()).ExecuteCommandAsync();
    }

    private static string Counts(ISqlSugarClient database) => string.Join(',', new[]
    {
        database.Queryable<Task101>().Count(), database.Queryable<Person101>().Count(),
        database.Queryable<Device101>().Count(), database.Queryable<Document101>().Count(),
        database.Queryable<WorkflowNode101>().Count()
    });

    private static async Task<int> TableCountAsync(NpgsqlConnection connection, string schema)
    {
        await using var command = new NpgsqlCommand(
            "SELECT count(*) FROM information_schema.tables WHERE table_schema = @schema AND table_name LIKE 't101_%'", connection);
        command.Parameters.AddWithValue("schema", schema);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task<HashSet<string>> UniqueIndexesAsync(NpgsqlConnection connection, string schema)
    {
        await using var command = new NpgsqlCommand(
            "SELECT indexname FROM pg_indexes WHERE schemaname = @schema AND indexdef ILIKE '% UNIQUE %'", connection);
        command.Parameters.AddWithValue("schema", schema);
        await using var reader = await command.ExecuteReaderAsync();
        var result = new HashSet<string>(StringComparer.Ordinal);
        while (await reader.ReadAsync()) result.Add(reader.GetString(0));
        return result;
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("TCP101_RUN_POSTGRES_TESTS") != "1")
            Skip = "Set TCP101_RUN_POSTGRES_TESTS=1 to run PostgreSQL integration tests.";
    }
}
