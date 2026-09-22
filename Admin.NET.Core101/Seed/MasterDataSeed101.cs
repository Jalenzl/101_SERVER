using Admin.NET.Core101.Entity;
using Admin.NET.Core101.Enum;

namespace Admin.NET.Core101.Seed;

public static class MasterDataSeed101
{
    private const string Transport = "运载试验技术事业部";
    private const string Space = "空间试验技术事业部";

    public static IReadOnlyList<Person101> People { get; } = new[]
    {
        Person(SeedIds101.Person1, "夏广", "男", Transport, "上面级试验区", "主任", true, "2025-08-21", false, null, true, "2028-03-21", true, "18312232910", 50, true),
        Person(SeedIds101.Person2, "沈传兵", "男", Space, "姿轨控试验区", "试验员", true, "2025-09-22", true, null, false, null, true, "010-231122", 30, true),
        Person(SeedIds101.Person3, "王佳艺", "女", Transport, "上面级试验区", "试验员", false, null, true, null, true, "2025-09-22", true, "010-02312245", 183, true),
        Person(SeedIds101.Person4, "委重生", "男", Transport, "大推力一试验区", "工艺岗", true, "2028-03-28", true, "2026-06-23", true, "2026-03-01", true, "32441244@qq.com", 249, true),
        Person(SeedIds101.Person5, "吴照兵", "男", Transport, "大推力二试验区", "指挥", true, null, true, "2026-06-23", true, "2026-03-01", true, "43211@qq.com", 123, true),
        Person(SeedIds101.Person6, "王野", "男", Space, "姿轨控试验区", "试验员", true, "2024-02-14", true, "2026-06-23", false, null, true, "19802312344", 204, true),
        Person(SeedIds101.Person7, "石凯琪", "男", Transport, "上面级试验区", "岗位责任人", true, "2028-03-28", true, "2026-06-23", true, "2026-03-01", true, "599872431@qq.com", 19, true),
        Person(SeedIds101.Person8, "赵一诺", "女", Space, "姿轨控试验区", "试验员", true, "2028-03-28", true, "2026-06-23", true, "2026-03-01", true, "18003421652", 34, true),
        Person(SeedIds101.Person9, "孙佳欣", "男", Transport, "大推力二试验区", "岗位责任人", false, null, false, null, true, "2027-08-05", true, "3482127862@qq.com", 89, true),
        Person(SeedIds101.Person10, "吴倩", "女", Transport, "大推力二试验区", "岗位负责人", true, "2023-05-17", true, "2027-07-15", true, "2029-06-29", true, "19342318998", 18, false),
    };

    public static IReadOnlyList<StoredFile101> StoredFiles { get; } = new[]
    {
        File(SeedIds101.StoredFile1, "SHB005C00501_校检.pdf"),
        File(SeedIds101.StoredFile2, "SHB5G1维保.pdf"),
        File(SeedIds101.StoredFile3, "CHA01-C717-检定.pdf"),
        File(SeedIds101.StoredFile4, "SHB5C维保.pdf"),
        File(SeedIds101.StoredFile5, "CHA05-C851—检定.pdf"),
        File(SeedIds101.StoredFile6, "CHA07维保.pdf"),
        File(SeedIds101.StoredFile7, "CHA07-F341-检定.pdf"),
        File(SeedIds101.StoredFile8, "CHA01维保.pdf"),
        File(SeedIds101.StoredFile9, "CHA05维保.pdf"),
        File(SeedIds101.StoredFile10, "GJB2012-12航天装备冲击试验规范.pdf"),
    };

    public static IReadOnlyList<Device101> Devices { get; } = new[]
    {
        Device(SeedIds101.Device1, "SHB5G10001", "精密压力表", "KP15149", "北京开平", "JDW-432", "", "2025-07-21", "在用", false, "2025-08-01", "3个月", "2026-07-31", "正常", "JL202406-3655", SeedIds101.StoredFile1, "2024-08-02", "指示不准矫正", "1年", null, SeedIds101.StoredFile2, 1000, 878, 16, null, Space, "姿轨控试验区", "GS-1台", "GS1", SystemType101.Process, "吊装"),
        Device(SeedIds101.Device2, "SHB5C00501", "电荷放大器", "YZ591409", "扬州电子", "G45BD21", "", "2025-12-20", "在用", true, "2024-12-23", "24个月", "2026-11-19", "超期", "JL202506-0018", SeedIds101.StoredFile3, "2023-03-14", "线路更换", "2年", "2025-03-13", SeedIds101.StoredFile4, 800, 345, 20, null, Space, "姿轨控试验区", "GS-2台", "GS2", SystemType101.Measurement, "电荷放大器"),
        Device(SeedIds101.Device3, "SHB5K52619", "直流稳压电源", "DYU1857", "安劲电源", "FG-003", "", "2026-01-03", "在用", false, "2025-04-09", "6个月", "2027-08-22", "正常", "JL202506-0019", SeedIds101.StoredFile5, "2025-11-30", "更换螺栓", "2年", "2026-11-29", SeedIds101.StoredFile6, 600, 231, 20, SeedIds101.Person2, Transport, "涞源试验中心", "C101台", "C101", SystemType101.Control, "程控"),
        Device(SeedIds101.Device4, "RDS-K00322", "采集装置", "IBM00221", "IBM中国", "JDW-432", "", "2023-09-25", "注销", false, "2026-01-23", "3个月", "2027-12-03", "超期", "JL202506-0021", SeedIds101.StoredFile7, "2023-08-25", "线圈更换", "2年", "2025-08-24", SeedIds101.StoredFile8, 1000, 549, 15, SeedIds101.Person2, Transport, "上面级试验区", "五号台", "005", SystemType101.Measurement, "采集装置"),
        Device(SeedIds101.Device5, "CHA01-C717", "真空传感器", "YER10203", "扬州二电", "HEW-978", "", "2024-08-02", "注销", true, "2025-08-01", "36个月", "2026-07-31", "正常", "JL202406-3655", SeedIds101.StoredFile1, "2023-03-14", "压力调整", "3年", "2026-03-13", SeedIds101.StoredFile9, 500, 212, 10, null, Transport, "上面级试验区", "六号台", "006", SystemType101.Measurement, "传感器"),
        Device(SeedIds101.Device6, "CHA05-C851", "流量传感器", "YER10203", "扬州二电", "JDW-200", "", "2025-04-29", "在用", true, "2024-12-23", "24个月", "2026-11-19", "超期", "JL202506-0018", SeedIds101.StoredFile3, "2025-11-30", "更换螺栓", "2年", "2026-11-29", SeedIds101.StoredFile6, 600, 231, 20, SeedIds101.Person6, Transport, "上面级试验区", "五号台", "005", SystemType101.Measurement, "传感器"),
        Device(SeedIds101.Device7, "tuiliA05-C851", "推力传感器", "tuili0203", "扬州二电", "JDW-200", "", "2025-04-29", "在用", true, "2024-12-23", "24个月", "2026-11-19", "超期", "JL202506-0018", SeedIds101.StoredFile3, "2025-11-30", "更换螺栓", "2年", "2026-11-29", SeedIds101.StoredFile6, 600, 231, 20, SeedIds101.Person6, Transport, "上面级试验区", "五号台", "005", SystemType101.Measurement, "传感器"),
        Device(SeedIds101.Device8, "CHA07-F341", "压力传感器", "BG109132", "北京光电", "JDW-300", "", "2025-04-29", "在用", true, "2025-04-09", "6个月", "2027-08-22", "正常", "JL202506-0019", SeedIds101.StoredFile5, "2023-03-14", "压力调整", "3年", "2026-03-13", SeedIds101.StoredFile9, 1000, 878, 16, null, Space, "姿轨控试验区", "GS-1台", "GS1", SystemType101.Measurement, "传感器"),
    };

    public static IReadOnlyList<Document101> Documents { get; } = new[]
    {
        Document(SeedIds101.Document1, "GJB2012-12", "航天装备冲击试验规范", "依据性文件", Transport, null, "2012-03-21", SeedIds101.StoredFile10),
        Document(SeedIds101.Document2, "QBJ4058-01", "发动机点火冲击试验作业指导书", "依据性文件", Space, null, "2012-09-21", null),
        Document(SeedIds101.Document3, "QBJ4058-12", "发动机振动试验作业指导书", "依据性文件", Space, null, "2015-08-23", null),
        Document(SeedIds101.Document4, "CJ-JS-0021", "CJ-90-1发动机点火冲击试验任务", "作业文件", Transport, null, "2012-03-21", null),
        Document(SeedIds101.Document5, "CJ-BG-0131", "CJ-90-1发动机点火冲击试验报告", "作业文件", Transport, SeedIds101.Person6, "2025-08-18", null),
        Document(SeedIds101.Document6, "BGMB-0921", "测量数据传表模板", "作业文件", Space, SeedIds101.Person1, "2021-04-21", null),
        Document(SeedIds101.Document7, "BGJC-1204", "试验前检查表模板", "作业文件", Space, SeedIds101.Person5, "2023-11-05", null),
    };

    public static IReadOnlyList<WorkflowNode101> WorkflowNodes => WorkflowSeed101.Items;

    private static Person101 Person(Guid id, string name, string gender, string department, string area, string title,
        bool specialOps, string? specialUntil, bool inspector, string? inspectorUntil, bool calibrator,
        string? calibratorUntil, bool assurance, string contact, int testCount, bool examPassed) => new()
    {
        Id = id, Name = name, Gender = gender, Department = department, Area = area, Title = title,
        SpecialOps = specialOps, SpecialOpsValidUntil = Date(specialUntil), Inspector = inspector,
        InspectorValidUntil = Date(inspectorUntil), Calibrator = calibrator,
        CalibratorValidUntil = Date(calibratorUntil), ProductAssurance = assurance, Contact = contact,
        TestCount = testCount, ExamPassed = examPassed
    };

    private static Device101 Device(Guid id, string code, string name, string factoryCode, string manufacturer,
        string model, string range, string? enabledDate, string status, bool measuring, string? calibrationDate,
        string calibrationCycle, string? validUntil, string calibrationStatus, string certificateNo,
        Guid? certificateFileId, string? lastMaintenance, string maintenanceContent, string maintenanceCycle,
        string? nextMaintenance, Guid? maintenanceFileId, int? suggestedUses, int usedCount, int? suggestedYears,
        Guid? ownerId, string department, string area, string rig, string rigCode, SystemType101 system,
        string subsystem) => new()
    {
        Id = id, Code = code, Name = name, FactoryCode = factoryCode, Manufacturer = manufacturer, Model = model,
        Range = range, EnabledDate = Date(enabledDate), UsageStatus = status, IsMeasuring = measuring,
        CalibrationDate = Date(calibrationDate), CalibrationCycle = calibrationCycle, ValidUntil = Date(validUntil),
        CalibrationStatus = calibrationStatus, CertificateNo = certificateNo,
        CertificateStoredFileId = certificateFileId, LastMaintenance = Date(lastMaintenance),
        MaintenanceContent = maintenanceContent, MaintenanceCycle = maintenanceCycle,
        NextMaintenance = Date(nextMaintenance), MaintenanceStoredFileId = maintenanceFileId,
        SuggestedUses = suggestedUses, UsedCount = usedCount, SuggestedYears = suggestedYears,
        OwnerPersonId = ownerId, Department = department, Area = area, Rig = rig, RigCode = rigCode,
        System = system, Subsystem = subsystem
    };

    private static StoredFile101 File(Guid id, string name) => new()
    {
        Id = id, OriginalName = name, StorageName = id.ToString("N") + ".pdf", RelativePath = "seed/" + name,
        Extension = ".pdf", ContentType = "application/pdf", Size = 0, Sha256 = string.Empty,
        UploaderUserId = 0, UploaderName = "系统种子"
    };

    private static Document101 Document(Guid id, string code, string name, string type, string department,
        Guid? authorId, string publishedAt, Guid? storedFileId) => new()
    {
        Id = id, Code = code, Name = name, Type = type, Department = department, AuthorPersonId = authorId,
        PublishedAt = DateOnly.Parse(publishedAt), CurrentStoredFileId = storedFileId
    };

    private static DateOnly? Date(string? value) =>
        DateOnly.TryParse(value, out var parsed) ? parsed : null;
}
