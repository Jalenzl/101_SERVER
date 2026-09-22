using System.ComponentModel;
using System.Runtime.Serialization;

namespace Admin.NET.Core101.Enum;

public enum OperationStatus101
{
    [Description("未开始"), EnumMember(Value = "未开始")]
    NotStarted,
    [Description("进行中"), EnumMember(Value = "进行中")]
    InProgress,
    [Description("已完成"), EnumMember(Value = "已完成")]
    Completed
}
