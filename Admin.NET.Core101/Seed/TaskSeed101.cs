using Admin.NET.Core101.Entity;
using Admin.NET.Core101.Enum;
using Newtonsoft.Json.Linq;

namespace Admin.NET.Core101.Seed;

public static class TaskSeed101
{
    public static IReadOnlyList<Task101> Tasks { get; } = new[]
    {
        Task(SeedIds101.TaskGg1101, "GG-11_01发动机额定工况全程试验", "空间试验技术事业部", "姿轨控试验区", "GS-1台", "GS1", "GG-11", "性能验证试验", 14310, 18, "航天六院", "2022-03-21", TaskStatus101.Completed),
        Task(SeedIds101.TaskHf2203, "HF-22_03氢氧发动机高模校准试验", "运载试验技术事业部", "上面级试验区", "五号台", "005", "HF-22B", "高空模拟校准试验", 3548, 1, "航天六院", "2026-09-28", TaskStatus101.InProgress, new DateTime(2026, 9, 28, 10, 30, 0)),
        Task(SeedIds101.TaskVs6691, "VS-66_91火箭发动机启动试验", "空间试验技术事业部", "姿轨控试验区", "GS-2台", "GS2", "VS-66", "多次启动验证试验", 14310, 18, "深蓝火箭公司", "2026-10-04", TaskStatus101.InProgress),
        Task(SeedIds101.TaskVs6692, "VS-66_92火箭发动机启动试验", "运载试验技术事业部", "大推力一试验区", "三号台Ⅰ工位", "031", "VS-66", "多次启动验证试验", 14310, 18, "深蓝火箭公司", "2026-10-18", TaskStatus101.InProgress),
        Task(SeedIds101.TaskVs6611, "VS-66_11火箭发动机启动试验", "空间试验技术事业部", "姿轨控试验区", "GS-3台", "GS3", "VS-66", "多次启动验证试验", 14310, 18, "深蓝火箭公司", "2026-11-04", TaskStatus101.InProgress),
    };

    public static IReadOnlyList<TaskPlan101> Plans { get; } = new[]
    {
        Plan("plan-1", "2026-09-20", "完成试验大纲会签", "总体室", "工艺、控制、测量系统", "线上会签", 1),
        Plan("plan-2", "2026-09-24", "完成设备与测量链路检查", "测量系统", "设备保障组", "形成检查记录", 2),
        Plan("plan-3", "2026-09-27", "试验前综合评审", "试验指挥", "各系统", "问题闭环后转实施", 3),
    };

    public static IReadOnlyList<TaskPerson101> TaskPeople { get; } = new[]
    {
        TeamPerson(1, SeedIds101.Person1, "总体设计师", 1),
        TeamPerson(2, SeedIds101.Person5, "事业部领导", 2),
        TeamPerson(3, SeedIds101.Person7, "试验指挥", 3),
        TeamPerson(4, SeedIds101.Person4, "工艺负责人", 4),
        TeamPerson(5, SeedIds101.Person9, "控制负责人", 5),
        TeamPerson(6, SeedIds101.Person3, "测量负责人", 6),
        PostPerson(7, SeedIds101.Person4, SystemType101.Process, "推进剂供应岗", "GY-01", 7),
        PostPerson(8, SeedIds101.Person9, SystemType101.Control, "控制操作岗", "KZ-01", 8),
    };

    public static IReadOnlyList<TaskDevice101> TaskDevices { get; } = new[]
    {
        new TaskDevice101 { Id = SeedIds101.TaskDevices["task-device-1"], TaskId = SeedIds101.TaskHf2203, DeviceId = SeedIds101.Device4, System = SystemType101.Measurement, OrderNo = 1 },
        new TaskDevice101 { Id = SeedIds101.TaskDevices["task-device-2"], TaskId = SeedIds101.TaskHf2203, DeviceId = SeedIds101.Device6, System = SystemType101.Measurement, OrderNo = 2 },
        new TaskDevice101 { Id = SeedIds101.TaskDevices["task-device-3"], TaskId = SeedIds101.TaskHf2203, DeviceId = SeedIds101.Device7, System = SystemType101.Measurement, OrderNo = 3 },
    };

    public static IReadOnlyList<TaskDocument101> TaskDocuments { get; } = new[]
    {
        new TaskDocument101 { Id = SeedIds101.TaskDocuments["task-document-1"], TaskId = SeedIds101.TaskHf2203, DocumentId = SeedIds101.Document1, DocumentType = "依据性文件", OrderNo = 1 },
        new TaskDocument101 { Id = SeedIds101.TaskDocuments["task-document-2"], TaskId = SeedIds101.TaskHf2203, DocumentId = SeedIds101.Document4, DocumentType = "作业文件", OrderNo = 2 },
    };

    public static IReadOnlyList<TaskWorkflow101> TaskWorkflows { get; } = Enumerable.Range(1, 5)
        .Select(index => new TaskWorkflow101
        {
            Id = SeedIds101.TaskWorkflows[$"task-workflow-{index}"],
            TaskId = SeedIds101.TaskHf2203,
            WorkflowNodeId = SeedIds101.Workflows[$"workflow-model-{index:0000}"],
            OrderNo = index
        }).ToArray();

    public static IReadOnlyList<TaskTransferRecord101> TransferRecords { get; } = new[]
    {
        new TaskTransferRecord101
        {
            Id = SeedIds101.TransferRecord1,
            TaskId = SeedIds101.TaskHf2203,
            TableId = "upper-thrust",
            OrderNo = 1,
            DataJson = JObject.Parse("""{"parameter":"F1","cableFront":"F1","cableBack":"F-1","daqDevice":"RDS-K00322","daqChannel":"0：1：0","daqGain":"1","daqFilter":"300","daqFormula":"2","sensorDevice":"tuiliA05-C851","sensorModel":"JDW-200","sensorValidUntil":"2026-12-22","unit":"N","remark":""}""")
        }
    };

    private static Task101 Task(Guid id, string name, string department, string area, string rig, string rigCode,
        string engineModel, string testType, int duration, int count, string client, string plannedDate,
        TaskStatus101 status, DateTime? ignitionTime = null) => new()
    {
        Id = id, Name = name, Department = department, Area = area, Rig = rig, RigCode = rigCode,
        EngineModel = engineModel, TestType = testType, IgnitionDuration = duration, IgnitionCount = count,
        Client = client, PlannedDate = DateOnly.Parse(plannedDate), IgnitionTime = ignitionTime, Status = status
    };

    private static TaskPlan101 Plan(string key, string completedDate, string content, string owner, string support,
        string remark, int orderNo) => new()
    {
        Id = SeedIds101.TaskPlans[key], TaskId = SeedIds101.TaskHf2203,
        CompletedDate = DateOnly.Parse(completedDate), Content = content, OwnerUnit = owner,
        SupportUnit = support, Remark = remark, OrderNo = orderNo
    };

    private static TaskPerson101 TeamPerson(int id, Guid personId, string role, int orderNo) => new()
    {
        Id = SeedIds101.TaskPeople[$"task-person-{id}"], TaskId = SeedIds101.TaskHf2203,
        PersonId = personId, Kind = "团队", Role = role, Rig = "五号台", OrderNo = orderNo
    };

    private static TaskPerson101 PostPerson(int id, Guid personId, SystemType101 system, string postName,
        string postCode, int orderNo) => new()
    {
        Id = SeedIds101.TaskPeople[$"task-person-{id}"], TaskId = SeedIds101.TaskHf2203,
        PersonId = personId, Kind = "岗位", Role = "岗位人员", System = system, Rig = "五号台",
        PostName = postName, PostCode = postCode, OrderNo = orderNo
    };
}
