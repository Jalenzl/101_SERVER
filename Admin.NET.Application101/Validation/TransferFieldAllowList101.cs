using System.Text.Json;

namespace Admin.NET.Application101.Validation;

public static class TransferFieldAllowList101
{
    public static readonly IReadOnlySet<string> TableIds = new HashSet<string>(StringComparer.Ordinal)
    {
        "space-thrust", "space-vacuum", "space-pressure", "space-temperature", "space-flow", "space-vibration",
        "upper-thrust", "upper-vacuum", "upper-pressure", "upper-temperature", "upper-flow",
        "laiyuan-thrust", "laiyuan-vacuum", "laiyuan-pressure", "laiyuan-temperature", "laiyuan-flow"
    };

    public static readonly IReadOnlySet<string> Fields = new HashSet<string>(StringComparer.Ordinal)
    {
        "parameter", "cableFront", "cableBack", "instrumentChannel", "instrumentLine", "instrumentGain",
        "daqDevice", "daqChannel", "daqGain", "daqFilter", "daqFormula", "sensorDevice", "sensorModel",
        "sensorValidUntil", "sensorZero", "sensorRange", "sensorMaxOutput", "sensorApprox", "sensorDepth",
        "sensorEquation", "sensorSlope", "unit", "remark", "ampDevice", "ampModel", "ampValidUntil",
        "ampSensitivity", "ampGain", "ampHighPass", "ampLowPass", "collector1Channel", "collector1Range",
        "collector1Filter", "collector2Channel", "collector2Range", "collector2Filter"
    };

    public static void Validate(string tableId, JsonElement data)
    {
        if (!TableIds.Contains(tableId)) throw Oops.Oh("不支持的传递表。").StatusCode(400);
        if (data.ValueKind != JsonValueKind.Object) throw Oops.Oh("传递表行必须是 JSON 对象。").StatusCode(400);
        foreach (var property in data.EnumerateObject())
        {
            if (!Fields.Contains(property.Name)) throw Oops.Oh("传递表包含未知字段。").StatusCode(400);
            if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                throw Oops.Oh("传递表字段只允许标量值。").StatusCode(400);
        }
    }
}
