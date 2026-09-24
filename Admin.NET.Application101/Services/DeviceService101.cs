using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Devices;

namespace Admin.NET.Application101.Services;

public sealed class DeviceService101(SqlSugarRepository<Device101> repository) : ITransient
{
    public async Task<PageResult<DeviceDto>> PageAsync(DevicePageQuery input)
    {
        var keyword = input.Keyword?.Trim();
        var query = repository.Context.Queryable<Device101, Person101>((device, owner) =>
                new JoinQueryInfos(JoinType.Left, device.OwnerPersonId == owner.Id))
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), (device, owner) =>
                device.Code.Contains(keyword!) || device.Name.Contains(keyword!) || device.FactoryCode.Contains(keyword!) ||
                device.Model.Contains(keyword!) || device.Rig.Contains(keyword!) || owner.Name.Contains(keyword!))
            .WhereIF(input.System.HasValue, (device, owner) => device.System == input.System)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Status), (device, owner) => device.UsageStatus == input.Status)
            .OrderBy((device, owner) => device.Code);
        RefAsync<int> total = 0;
        var items = await query.Select((device, owner) => new DeviceDto
        {
            Id = device.Id, Code = device.Code, Name = device.Name, FactoryCode = device.FactoryCode,
            Manufacturer = device.Manufacturer, Model = device.Model, Range = device.Range,
            EnabledDate = device.EnabledDate, UsageStatus = device.UsageStatus, IsMeasuring = device.IsMeasuring,
            CalibrationDate = device.CalibrationDate, CalibrationCycle = device.CalibrationCycle,
            ValidUntil = device.ValidUntil, CalibrationStatus = device.CalibrationStatus,
            CertificateNo = device.CertificateNo, CertificateStoredFileId = device.CertificateStoredFileId,
            LastMaintenance = device.LastMaintenance, MaintenanceContent = device.MaintenanceContent,
            MaintenanceCycle = device.MaintenanceCycle, NextMaintenance = device.NextMaintenance,
            MaintenanceStoredFileId = device.MaintenanceStoredFileId, SuggestedUses = device.SuggestedUses,
            UsedCount = device.UsedCount, SuggestedYears = device.SuggestedYears,
            OwnerPersonId = device.OwnerPersonId, OwnerName = owner.Name, Department = device.Department,
            Area = device.Area, Rig = device.Rig, RigCode = device.RigCode, System = device.System,
            Subsystem = device.Subsystem
        })
            .ToPageListAsync(input.Page, input.PageSize, total);
        return new PageResult<DeviceDto> { Items = items, Page = input.Page, PageSize = input.PageSize, Total = total };
    }

    public async Task<DeviceDto> GetAsync(Guid id)
    {
        var item = await repository.Context.Queryable<Device101, Person101>((device, owner) =>
                new JoinQueryInfos(JoinType.Left, device.OwnerPersonId == owner.Id))
            .Where((device, owner) => device.Id == id)
            .Select((device, owner) => new DeviceDto
            {
                Id = device.Id, Code = device.Code, Name = device.Name, FactoryCode = device.FactoryCode,
                Manufacturer = device.Manufacturer, Model = device.Model, Range = device.Range,
                EnabledDate = device.EnabledDate, UsageStatus = device.UsageStatus,
                IsMeasuring = device.IsMeasuring, CalibrationDate = device.CalibrationDate,
                CalibrationCycle = device.CalibrationCycle, ValidUntil = device.ValidUntil,
                CalibrationStatus = device.CalibrationStatus, CertificateNo = device.CertificateNo,
                CertificateStoredFileId = device.CertificateStoredFileId, LastMaintenance = device.LastMaintenance,
                MaintenanceContent = device.MaintenanceContent, MaintenanceCycle = device.MaintenanceCycle,
                NextMaintenance = device.NextMaintenance, MaintenanceStoredFileId = device.MaintenanceStoredFileId,
                SuggestedUses = device.SuggestedUses, UsedCount = device.UsedCount,
                SuggestedYears = device.SuggestedYears, OwnerPersonId = device.OwnerPersonId,
                OwnerName = owner.Name, Department = device.Department, Area = device.Area, Rig = device.Rig,
                RigCode = device.RigCode, System = device.System, Subsystem = device.Subsystem
            }).FirstAsync();
        return item ?? throw NotFound();
    }

    public async Task<Guid> CreateAsync(CreateDeviceInput input)
    {
        await EnsureCodeUnique(input.Code, null);
        var item = Map(input, new Device101());
        await repository.InsertAsync(item);
        return item.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateDeviceInput input)
    {
        var item = await FindAsync(id);
        await EnsureCodeUnique(input.Code, id);
        Map(input, item); item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).ExecuteCommandAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await FindAsync(id);
        if (await repository.Context.Queryable<TaskDevice101>().AnyAsync(row => row.DeviceId == id))
            throw Oops.Oh("设备已被任务引用，不能删除。").StatusCode(409);
        item.IsDelete = true; item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).UpdateColumns(row => new { row.IsDelete, row.UpdateTime }).ExecuteCommandAsync();
    }

    private async Task EnsureCodeUnique(string code, Guid? exceptId)
    {
        var value = code.Trim();
        if (await repository.IsAnyAsync(item => item.Code == value && (!exceptId.HasValue || item.Id != exceptId.Value)))
            throw Oops.Oh("设备编号已存在。").StatusCode(409);
    }

    private async Task<Device101> FindAsync(Guid id) =>
        await repository.GetFirstAsync(item => item.Id == id) ?? throw NotFound();

    private static Device101 Map(CreateDeviceInput input, Device101 item)
    {
        item.Code = input.Code.Trim(); item.Name = input.Name.Trim(); item.FactoryCode = input.FactoryCode.Trim();
        item.Manufacturer = input.Manufacturer.Trim(); item.Model = input.Model.Trim(); item.Range = input.Range.Trim();
        item.EnabledDate = input.EnabledDate; item.UsageStatus = input.UsageStatus.Trim();
        item.IsMeasuring = input.IsMeasuring; item.CalibrationDate = input.CalibrationDate;
        item.CalibrationCycle = input.CalibrationCycle.Trim(); item.ValidUntil = input.ValidUntil;
        item.CalibrationStatus = input.CalibrationStatus.Trim(); item.CertificateNo = input.CertificateNo.Trim();
        item.CertificateStoredFileId = input.CertificateStoredFileId; item.LastMaintenance = input.LastMaintenance;
        item.MaintenanceContent = input.MaintenanceContent.Trim(); item.MaintenanceCycle = input.MaintenanceCycle.Trim();
        item.NextMaintenance = input.NextMaintenance; item.MaintenanceStoredFileId = input.MaintenanceStoredFileId;
        item.SuggestedUses = input.SuggestedUses; item.UsedCount = input.UsedCount;
        item.SuggestedYears = input.SuggestedYears; item.OwnerPersonId = input.OwnerPersonId;
        item.Department = input.Department.Trim(); item.Area = input.Area.Trim(); item.Rig = input.Rig.Trim();
        item.RigCode = input.RigCode.Trim(); item.System = input.System; item.Subsystem = input.Subsystem.Trim();
        return item;
    }

    private static Exception NotFound() => Oops.Oh("记录不存在。").StatusCode(404);
}
