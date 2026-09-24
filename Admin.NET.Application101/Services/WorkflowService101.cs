using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Workflows;

namespace Admin.NET.Application101.Services;

public sealed class WorkflowService101(SqlSugarRepository<WorkflowNode101> repository) : ITransient
{
    public async Task<PageResult<WorkflowDto>> PageAsync(WorkflowPageQuery input)
    {
        var keyword = input.Keyword?.Trim();
        var query = repository.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), item => item.Department.Contains(keyword!) ||
                item.Area.Contains(keyword!) || item.Process.Contains(keyword!) || item.Step.Contains(keyword!) ||
                item.Post.Contains(keyword!))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Department), item => item.Department == input.Department)
            .OrderBy(item => item.OrderNo);
        RefAsync<int> total = 0;
        var items = await query.Select(item => new WorkflowDto
        {
            Id = item.Id, Department = item.Department, Area = item.Area, Process = item.Process,
            Step = item.Step, Post = item.Post, CheckPost = item.CheckPost ?? string.Empty,
            Countersign = item.Countersign ?? string.Empty, Confirmer = item.Confirmer ?? string.Empty,
            Remark = item.Remark ?? string.Empty,
            Order = item.OrderNo, Enabled = item.Enabled
        }).ToPageListAsync(input.Page, input.PageSize, total);
        return new PageResult<WorkflowDto> { Items = items, Page = input.Page, PageSize = input.PageSize, Total = total };
    }

    public async Task<IReadOnlyList<WorkflowTreeNodeDto>> TreeAsync(string? department)
    {
        var rows = await repository.AsQueryable().Where(item => item.Enabled)
            .WhereIF(!string.IsNullOrWhiteSpace(department), item => item.Department == department)
            .OrderBy(item => item.OrderNo).ToListAsync();
        return rows.GroupBy(item => item.Department).Select(departmentGroup => Group(
            "department:" + departmentGroup.Key, departmentGroup.Key, "department",
            departmentGroup.GroupBy(item => item.Area).Select(areaGroup => Group(
                "area:" + departmentGroup.Key + ":" + areaGroup.Key, areaGroup.Key, "area",
                areaGroup.GroupBy(item => item.Process).Select(processGroup => Group(
                    "process:" + departmentGroup.Key + ":" + areaGroup.Key + ":" + processGroup.Key,
                    processGroup.Key, "process",
                    processGroup.Select(item => new WorkflowTreeNodeDto
                    {
                        Id = item.Id.ToString(), Label = item.Step, Kind = "step", Selectable = true
                    }).ToArray())).ToArray())).ToArray())).ToArray();
    }

    public async Task<Guid> CreateAsync(CreateWorkflowInput input)
    {
        var item = Map(input, new WorkflowNode101());
        await repository.InsertAsync(item);
        return item.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateWorkflowInput input)
    {
        var item = await FindAsync(id);
        Map(input, item); item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).ExecuteCommandAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await FindAsync(id);
        if (await repository.Context.Queryable<TaskWorkflow101>().AnyAsync(row => row.WorkflowNodeId == id))
            throw Oops.Oh("流程节点已被任务引用，不能删除。").StatusCode(409);
        item.IsDelete = true; item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).UpdateColumns(row => new { row.IsDelete, row.UpdateTime }).ExecuteCommandAsync();
    }

    private async Task<WorkflowNode101> FindAsync(Guid id) =>
        await repository.GetFirstAsync(item => item.Id == id) ?? throw Oops.Oh("记录不存在。").StatusCode(404);

    private static WorkflowNode101 Map(CreateWorkflowInput input, WorkflowNode101 item)
    {
        item.Department = input.Department.Trim(); item.Area = input.Area.Trim(); item.Process = input.Process.Trim();
        item.Step = input.Step.Trim(); item.Post = input.Post.Trim();
        item.CheckPost = input.CheckPost?.Trim() ?? string.Empty;
        item.Countersign = input.Countersign?.Trim() ?? string.Empty;
        item.Confirmer = input.Confirmer?.Trim() ?? string.Empty;
        item.Remark = input.Remark?.Trim() ?? string.Empty;
        item.OrderNo = input.Order; item.Enabled = input.Enabled;
        return item;
    }

    private static WorkflowTreeNodeDto Group(string id, string label, string kind,
        IReadOnlyList<WorkflowTreeNodeDto> children) => new()
        { Id = id, Label = label, Kind = kind, Selectable = false, Children = children };
}
