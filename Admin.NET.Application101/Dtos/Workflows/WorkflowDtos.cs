using Admin.NET.Application101.Dtos.Common;

namespace Admin.NET.Application101.Dtos.Workflows;

public sealed class WorkflowPageQuery : PageQuery
{
    public string? Department { get; set; }
}

public sealed class WorkflowDto
{
    public Guid Id { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Process { get; set; } = string.Empty;
    public string Step { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string TemplateData { get; set; } = string.Empty;
    public string Post { get; set; } = string.Empty;
    public string CheckPost { get; set; } = string.Empty;
    public string Countersign { get; set; } = string.Empty;
    public string Confirmer { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool Enabled { get; set; }
}

public class CreateWorkflowInput
{
    [Required, RegularExpression(@".*\S.*")] public string Department { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    [Required, RegularExpression(@".*\S.*")] public string Process { get; set; } = string.Empty;
    [Required, RegularExpression(@".*\S.*")] public string Step { get; set; } = string.Empty;
    [RegularExpression(@"^(试验计划|人员准备|设备准备|文件准备|设备FMECA分析|工序FMEA分析|任务风险|试验总结)?$")] public string Template { get; set; } = string.Empty;
    public string TemplateData { get; set; } = string.Empty;
    public string Post { get; set; } = string.Empty;
    public string CheckPost { get; set; } = string.Empty;
    public string Countersign { get; set; } = string.Empty;
    public string Confirmer { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int Order { get; set; }
    public bool Enabled { get; set; } = true;
}

public sealed class UpdateWorkflowInput : CreateWorkflowInput;

public sealed class SelectWorkflowTemplateInput
{
    [Required, RegularExpression(@".*\S.*")] public string Department { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? Process { get; set; }
    public string? Step { get; set; }
    [Required, RegularExpression(@"^(试验计划|人员准备|设备准备|文件准备|设备FMECA分析|工序FMEA分析|任务风险|试验总结)$")]
    public string Template { get; set; } = string.Empty;
    public string TemplateData { get; set; } = string.Empty;
}

public sealed class WorkflowTreeNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public bool Selectable { get; set; }
    public IReadOnlyList<WorkflowTreeNodeDto> Children { get; set; } = Array.Empty<WorkflowTreeNodeDto>();
}
