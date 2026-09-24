using Newtonsoft.Json.Linq;

namespace Admin.NET.Application101.Dtos.Records;

public sealed record RecordRowInput(Guid Id, int Order, JObject Data);
public sealed record SaveRecordRowsInput(IReadOnlyList<RecordRowInput> Rows);
public sealed record RecordRowDto(Guid Id, int Order, JObject Data);
public sealed record SignatureDto(string Role, string SignerName, DateTime SignedAt);
public sealed record SignRecordInput(string Role);
