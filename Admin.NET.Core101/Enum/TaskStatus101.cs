using System.ComponentModel;
using System.Runtime.Serialization;

namespace Admin.NET.Core101.Enum;

public enum TaskStatus101
{
    [Description("进行中"), EnumMember(Value = "进行中")]
    InProgress,
    [Description("已完成"), EnumMember(Value = "已完成")]
    Completed,
    [Description("终止"), EnumMember(Value = "终止")]
    Terminated
}
