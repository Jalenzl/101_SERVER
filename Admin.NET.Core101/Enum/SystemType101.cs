using System.ComponentModel;
using System.Runtime.Serialization;

namespace Admin.NET.Core101.Enum;

public enum SystemType101
{
    [Description("工艺系统"), EnumMember(Value = "工艺系统")]
    Process,
    [Description("控制系统"), EnumMember(Value = "控制系统")]
    Control,
    [Description("测量系统"), EnumMember(Value = "测量系统")]
    Measurement
}
