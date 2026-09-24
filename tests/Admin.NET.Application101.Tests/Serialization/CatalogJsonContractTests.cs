using Admin.NET.Application101.Dtos.Preparation;
using Admin.NET.Application101.Dtos.Records;
using Admin.NET.Application101.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Admin.NET.Application101.Tests.Serialization;

public sealed class CatalogJsonContractTests
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    [Fact]
    public void CatalogRecord_EmitsFieldObject()
    {
        var source = JObject.Parse("""{"name":"地面校准试验","department":"运载试验技术事业部"}""");
        var dto = new CatalogRecordDto(Guid.NewGuid(), source, null);
        var json = JObject.Parse(JsonConvert.SerializeObject(dto, Settings));

        Assert.Equal("地面校准试验", (string?)json["data"]?["name"]);
    }

    [Fact]
    public void TaskAndTransferRows_EmitAndAcceptFieldObjects()
    {
        var source = JObject.Parse("""{"parameter":"F1"}""");
        var id = Guid.NewGuid();
        var taskRow = JObject.Parse(JsonConvert.SerializeObject(new RecordRowDto(id, 1, source), Settings));
        var transferRow = JObject.Parse(JsonConvert.SerializeObject(new TransferRowDto(id, 1, source), Settings));

        Assert.Equal("F1", (string?)taskRow["data"]?["parameter"]);
        Assert.Equal("F1", (string?)transferRow["data"]?["parameter"]);
        var inputJson = JsonConvert.SerializeObject(new
        {
            rows = new[] { new { id, order = 1, data = new { parameter = "F1" } } }
        });
        var input = JsonConvert.DeserializeObject<SaveRecordRowsInput>(inputJson, Settings);
        Assert.Equal("F1", input!.Rows[0].Data.Value<string>("parameter"));
    }
}
