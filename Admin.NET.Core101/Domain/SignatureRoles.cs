using System.Collections.Frozen;

namespace Admin.NET.Core101.Domain;

public static class SignatureRoles
{
    public static readonly IReadOnlySet<string> Equipment =
        new[] { "配置人", "复核人", "负责人" }.ToFrozenSet();

    public static readonly IReadOnlySet<string> Operation =
        new[] { "操作岗", "检查岗", "检验员会签", "委托单位会签", "产保会签" }.ToFrozenSet();
}
