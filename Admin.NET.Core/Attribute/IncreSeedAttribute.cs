namespace Admin.NET.Core;

/// <summary>
/// 增量种子特性
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class IncreSeedAttribute : Attribute
{
}