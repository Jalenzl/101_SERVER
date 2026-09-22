using Admin.NET.Core101.Entity;

namespace Admin.NET.Core101.Seed;

public static partial class SeedIds101
{
    public static readonly Guid Person1 = Guid.Parse("10110000-0000-0000-0000-000000000001");
    public static readonly Guid Person2 = Guid.Parse("10110000-0000-0000-0000-000000000002");
    public static readonly Guid Person3 = Guid.Parse("10110000-0000-0000-0000-000000000003");
    public static readonly Guid Person4 = Guid.Parse("10110000-0000-0000-0000-000000000004");
    public static readonly Guid Person5 = Guid.Parse("10110000-0000-0000-0000-000000000005");
    public static readonly Guid Person6 = Guid.Parse("10110000-0000-0000-0000-000000000006");
    public static readonly Guid Person7 = Guid.Parse("10110000-0000-0000-0000-000000000007");
    public static readonly Guid Person8 = Guid.Parse("10110000-0000-0000-0000-000000000008");
    public static readonly Guid Person9 = Guid.Parse("10110000-0000-0000-0000-000000000009");
    public static readonly Guid Person10 = Guid.Parse("10110000-0000-0000-0000-000000000010");

    public static readonly Guid Device1 = Guid.Parse("10120000-0000-0000-0000-000000000001");
    public static readonly Guid Device2 = Guid.Parse("10120000-0000-0000-0000-000000000002");
    public static readonly Guid Device3 = Guid.Parse("10120000-0000-0000-0000-000000000003");
    public static readonly Guid Device4 = Guid.Parse("10120000-0000-0000-0000-000000000004");
    public static readonly Guid Device5 = Guid.Parse("10120000-0000-0000-0000-000000000005");
    public static readonly Guid Device6 = Guid.Parse("10120000-0000-0000-0000-000000000006");
    public static readonly Guid Device7 = Guid.Parse("10120000-0000-0000-0000-000000000007");
    public static readonly Guid Device8 = Guid.Parse("10120000-0000-0000-0000-000000000008");

    public static readonly Guid StoredFile1 = Guid.Parse("10130000-0000-0000-0000-000000000001");
    public static readonly Guid StoredFile2 = Guid.Parse("10130000-0000-0000-0000-000000000002");
    public static readonly Guid StoredFile3 = Guid.Parse("10130000-0000-0000-0000-000000000003");
    public static readonly Guid StoredFile4 = Guid.Parse("10130000-0000-0000-0000-000000000004");
    public static readonly Guid StoredFile5 = Guid.Parse("10130000-0000-0000-0000-000000000005");
    public static readonly Guid StoredFile6 = Guid.Parse("10130000-0000-0000-0000-000000000006");
    public static readonly Guid StoredFile7 = Guid.Parse("10130000-0000-0000-0000-000000000007");
    public static readonly Guid StoredFile8 = Guid.Parse("10130000-0000-0000-0000-000000000008");
    public static readonly Guid StoredFile9 = Guid.Parse("10130000-0000-0000-0000-000000000009");
    public static readonly Guid StoredFile10 = Guid.Parse("10130000-0000-0000-0000-000000000010");

    public static readonly Guid Document1 = Guid.Parse("10140000-0000-0000-0000-000000000001");
    public static readonly Guid Document2 = Guid.Parse("10140000-0000-0000-0000-000000000002");
    public static readonly Guid Document3 = Guid.Parse("10140000-0000-0000-0000-000000000003");
    public static readonly Guid Document4 = Guid.Parse("10140000-0000-0000-0000-000000000004");
    public static readonly Guid Document5 = Guid.Parse("10140000-0000-0000-0000-000000000005");
    public static readonly Guid Document6 = Guid.Parse("10140000-0000-0000-0000-000000000006");
    public static readonly Guid Document7 = Guid.Parse("10140000-0000-0000-0000-000000000007");

    public static readonly Guid WorkflowModel0001 = Guid.Parse("10150000-0000-0000-0000-000000000001");

    public static readonly Guid TaskGg1101 = Guid.Parse("10160000-0000-0000-0000-000000000001");
    public static readonly Guid TaskHf2203 = Guid.Parse("10160000-0000-0000-0000-000000000002");
    public static readonly Guid TaskVs6691 = Guid.Parse("10160000-0000-0000-0000-000000000003");
    public static readonly Guid TaskVs6692 = Guid.Parse("10160000-0000-0000-0000-000000000004");
    public static readonly Guid TaskVs6611 = Guid.Parse("10160000-0000-0000-0000-000000000005");

    public static IReadOnlyDictionary<string, Guid> TaskPlans { get; } = new Dictionary<string, Guid>
    {
        ["plan-1"] = Guid.Parse("10170000-0000-0000-0000-000000000001"),
        ["plan-2"] = Guid.Parse("10170000-0000-0000-0000-000000000002"),
        ["plan-3"] = Guid.Parse("10170000-0000-0000-0000-000000000003"),
    };

    public static IReadOnlyDictionary<string, Guid> TaskPeople { get; } = new Dictionary<string, Guid>
    {
        ["task-person-1"] = Guid.Parse("10180000-0000-0000-0000-000000000001"),
        ["task-person-2"] = Guid.Parse("10180000-0000-0000-0000-000000000002"),
        ["task-person-3"] = Guid.Parse("10180000-0000-0000-0000-000000000003"),
        ["task-person-4"] = Guid.Parse("10180000-0000-0000-0000-000000000004"),
        ["task-person-5"] = Guid.Parse("10180000-0000-0000-0000-000000000005"),
        ["task-person-6"] = Guid.Parse("10180000-0000-0000-0000-000000000006"),
        ["task-person-7"] = Guid.Parse("10180000-0000-0000-0000-000000000007"),
        ["task-person-8"] = Guid.Parse("10180000-0000-0000-0000-000000000008"),
    };

    public static IReadOnlyDictionary<string, Guid> TaskDevices { get; } = new Dictionary<string, Guid>
    {
        ["task-device-1"] = Guid.Parse("10190000-0000-0000-0000-000000000001"),
        ["task-device-2"] = Guid.Parse("10190000-0000-0000-0000-000000000002"),
        ["task-device-3"] = Guid.Parse("10190000-0000-0000-0000-000000000003"),
    };

    public static IReadOnlyDictionary<string, Guid> TaskDocuments { get; } = new Dictionary<string, Guid>
    {
        ["task-document-1"] = Guid.Parse("101a0000-0000-0000-0000-000000000001"),
        ["task-document-2"] = Guid.Parse("101a0000-0000-0000-0000-000000000002"),
    };

    public static IReadOnlyDictionary<string, Guid> TaskWorkflows { get; } = new Dictionary<string, Guid>
    {
        ["task-workflow-1"] = Guid.Parse("101b0000-0000-0000-0000-000000000001"),
        ["task-workflow-2"] = Guid.Parse("101b0000-0000-0000-0000-000000000002"),
        ["task-workflow-3"] = Guid.Parse("101b0000-0000-0000-0000-000000000003"),
        ["task-workflow-4"] = Guid.Parse("101b0000-0000-0000-0000-000000000004"),
        ["task-workflow-5"] = Guid.Parse("101b0000-0000-0000-0000-000000000005"),
    };
    public static readonly Guid TransferRecord1 = Guid.Parse("101c0000-0000-0000-0000-000000000001");

    public static IEnumerable<Guid> All => SeedEntities().Select(item => item.Id);

    private static IEnumerable<Entity101Base> SeedEntities() =>
        MasterDataSeed101.People.Cast<Entity101Base>()
            .Concat(MasterDataSeed101.Devices)
            .Concat(MasterDataSeed101.StoredFiles)
            .Concat(MasterDataSeed101.Documents)
            .Concat(MasterDataSeed101.WorkflowNodes)
            .Concat(TaskSeed101.Tasks)
            .Concat(TaskSeed101.Plans)
            .Concat(TaskSeed101.TaskPeople)
            .Concat(TaskSeed101.TaskDevices)
            .Concat(TaskSeed101.TaskDocuments)
            .Concat(TaskSeed101.TaskWorkflows)
            .Concat(TaskSeed101.TransferRecords);

}
