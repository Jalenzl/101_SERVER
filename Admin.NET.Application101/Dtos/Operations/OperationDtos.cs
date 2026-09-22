namespace Admin.NET.Application101.Dtos.Operations;

public sealed record SaveTaskWorkflowInput(IReadOnlyList<Guid> WorkflowNodeIds);
public sealed record TaskWorkflowSelectionDto(IReadOnlyList<Guid> WorkflowNodeIds);

public sealed class OperationDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid WorkflowNodeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public string Process { get; set; } = string.Empty;
    public string Step { get; set; } = string.Empty;
    public string Post { get; set; } = string.Empty;
    public DateOnly? Date { get; set; }
    public string QualityRequirement { get; set; } = string.Empty;
    public string OperationRequirement { get; set; } = string.Empty;
    public string Attention { get; set; } = string.Empty;
    public OperationStatus101 Status { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IReadOnlyList<OperationCheckDto> Checks { get; set; } = Array.Empty<OperationCheckDto>();
    public IReadOnlyList<OperationSignatureDto> Signatures { get; set; } = Array.Empty<OperationSignatureDto>();
}

public sealed class UpdateOperationInput
{
    public string Code { get; set; } = string.Empty;
    public DateOnly? Date { get; set; }
    public string QualityRequirement { get; set; } = string.Empty;
    public string OperationRequirement { get; set; } = string.Empty;
    public string Attention { get; set; } = string.Empty;
    public OperationStatus101 Status { get; set; }
}

public sealed record OperationCheckInput(Guid? Id, string Item, string Requirement, string Actual, string Remark, int Order);
public sealed record SaveOperationChecksInput(IReadOnlyList<OperationCheckInput> Rows);
public sealed record OperationCheckDto(Guid Id, string Item, string Requirement, string Actual, string Remark, int Order);
public sealed record CreateSignatureInput(string Role);
public sealed record OperationSignatureDto(Guid Id, string Role, long SignerUserId, string SignerName, DateTime SignedAt);
