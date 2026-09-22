using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Admin.NET.Core;

public static class CommonUtil
{
    public static string ExecPercent(decimal passCount, decimal allCount)
    {
        if (allCount <= 0)
        {
            return "0%";
        }

        var value = (double)Math.Round(passCount / allCount * 100, 1);
        var rounded = value < 0
            ? Math.Round(value + 5 / Math.Pow(10, 1), 0, MidpointRounding.AwayFromZero)
            : Math.Round(value, 0, MidpointRounding.AwayFromZero);
        return $"{rounded}%";
    }

    public static string GetLocalhost()
    {
        var result = $"{App.HttpContext.Request.Scheme}://{App.HttpContext.Request.Host.Value}";
        if (App.HttpContext.Request.Headers.ContainsKey("X-Original-Host"))
        {
            result = $"{App.HttpContext.Request.Scheme}://{App.HttpContext.Request.Headers["X-Original-Host"]}";
        }

        return result;
    }

    public static string SerializeObjectToXml<T>(T obj)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        var serializer = new XmlSerializer(obj.GetType());
        using var stream = new MemoryStream();
        var settings = new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = true };
        using (var writer = XmlWriter.Create(stream, settings))
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            serializer.Serialize(writer, obj, namespaces);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static XElement SerializeStringToXml(string xml)
    {
        try
        {
            return XElement.Parse(xml);
        }
        catch
        {
            return null;
        }
    }
}
