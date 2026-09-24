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
    public string Post { get; set; } = string.Empty;
    public string CheckPost { get; set; } = string.Empty;
    public string Countersign { get; set; } = string.Empty;
    public string Confirmer { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int Order { get; set; }
    public bool Enabled { get; set; } = true;
}

public sealed class UpdateWorkflowInput : CreateWorkflowInput;

public sealed class WorkflowTreeNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public bool Selectable { get; set; }
    public IReadOnlyList<WorkflowTreeNodeDto> Children { get; set; } = Array.Empty<WorkflowTreeNodeDto>();
}
