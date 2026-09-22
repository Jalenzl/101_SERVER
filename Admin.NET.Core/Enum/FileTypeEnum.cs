namespace Admin.NET.Core;

/// <summary>
/// 文档类型枚举
/// </summary>
[Description("文档类型枚举")]
public enum FileTypeEnum
{
    /// <summary>未指定类型</summary>
    [Description("未指定")]
    None = 0,

    /// <summary>
    ///
    /// </summary>
    [Description("附件")]
    Attacnment = 1,

    /// <summary>
    ///
    /// </summary>
    [Description("报告")]
    Report = 2,

    /// <summary>报告模板文件</summary>
    [Description("报告模板")]
    ReportTemplate = 3,

    #region 月报
    /// <summary>设备制造分级清单</summary>
    [Description("设备制造分级清单")]
    EquipmentManufacturingClassificationList = 4,
    #endregion

 
    /// <summary>相关材料</summary>
    [Description("相关材料")]
    RelatedFile = 5,

    /// <summary>设备制造分级清单</summary>
    [Description("证明材料")]
    ProveFile = 6,


    #region QualityRecordSheet 监造监督记录单
    /// <summary>见证通知单</summary>
    [Description("见证通知单")] 
    Wintess = 7,

    #endregion

    /// <summary>证书</summary>
    [Description("证书")]
    Certification = 8,


    /// <summary>设备出厂图片</summary>
    [Description("设备出厂图片")]
    ProjectFactoryEquipmentImages = 9,


    /// <summary>封面</summary>
    [Description("封面")]
    Cover = 10,

    /// <summary>备忘录内容</summary>
    [Description("备忘录内容")]
    MemoContext = 11,

    /// <summary>概要附件</summary>
    [Description("概要附件")]
    Summary = 12,
}