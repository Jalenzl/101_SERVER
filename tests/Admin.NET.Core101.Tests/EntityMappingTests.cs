using System.Reflection;
using Admin.NET.Core;
using Admin.NET.Core101.Entity;
using SqlSugar;

namespace Admin.NET.Core101.Tests;

public sealed class EntityMappingTests
{
    public static TheoryData<Type> EntityTypes => new()
    {
        typeof(Task101), typeof(Person101), typeof(Device101), typeof(Document101), typeof(StoredFile101),
        typeof(TaskPlan101), typeof(TaskPerson101), typeof(TaskDevice101), typeof(TaskTransferRecord101),
        typeof(TaskDocument101), typeof(WorkflowNode101), typeof(TaskWorkflow101), typeof(Operation101),
        typeof(OperationCheck101), typeof(OperationSignature101)
    };

    [Theory]
    [MemberData(nameof(EntityTypes))]
    public void Entity_HasExpectedTablePrimaryKeyAndSoftDelete(Type entityType)
    {
        var table = entityType.GetCustomAttribute<SugarTable>();
        Assert.NotNull(table);
        Assert.StartsWith("t101_", table!.TableName, StringComparison.Ordinal);
        Assert.True(typeof(IDeletedFilter).IsAssignableFrom(entityType));

        var id = entityType.GetProperty(nameof(Entity101Base.Id));
        Assert.NotNull(id);
        Assert.Equal(typeof(Guid), id!.PropertyType);
        Assert.True(id.GetCustomAttribute<SugarColumn>()?.IsPrimaryKey);
    }

    public static TheoryData<Type, string[]> UniqueIndexes => new()
    {
        { typeof(Device101), new[] { nameof(Device101.Code) } },
        { typeof(Document101), new[] { nameof(Document101.Code) } },
        { typeof(TaskPerson101), new[] { nameof(TaskPerson101.TaskId), nameof(TaskPerson101.PersonId), nameof(TaskPerson101.Kind), nameof(TaskPerson101.Role), nameof(TaskPerson101.System) } },
        { typeof(TaskDevice101), new[] { nameof(TaskDevice101.TaskId), nameof(TaskDevice101.DeviceId), nameof(TaskDevice101.System) } },
        { typeof(TaskDocument101), new[] { nameof(TaskDocument101.TaskId), nameof(TaskDocument101.DocumentId), nameof(TaskDocument101.DocumentType) } },
        { typeof(TaskWorkflow101), new[] { nameof(TaskWorkflow101.TaskId), nameof(TaskWorkflow101.WorkflowNodeId) } },
        { typeof(TaskTransferRecord101), new[] { nameof(TaskTransferRecord101.TaskId), nameof(TaskTransferRecord101.TableId), nameof(TaskTransferRecord101.OrderNo) } },
        { typeof(OperationCheck101), new[] { nameof(OperationCheck101.OperationId), nameof(OperationCheck101.OrderNo) } },
        { typeof(OperationSignature101), new[] { nameof(OperationSignature101.Scope), nameof(OperationSignature101.ScopeId), nameof(OperationSignature101.Role) } },
    };

    [Theory]
    [MemberData(nameof(UniqueIndexes))]
    public void Entity_HasRequiredUniqueIndex(Type entityType, string[] fields)
    {
        var index = entityType.GetCustomAttributes<SugarIndexAttribute>()
            .SingleOrDefault(item => item.IsUnique && item.IndexFields.Keys.ToHashSet().SetEquals(fields));

        Assert.NotNull(index);
    }
}
