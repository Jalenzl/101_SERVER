using Admin.NET.Application101.Dtos.Common;
using Admin.NET.Application101.Dtos.Personnel;

namespace Admin.NET.Application101.Services;

public sealed class PersonnelService101(SqlSugarRepository<Person101> repository) : ITransient
{
    public async Task<PageResult<PersonDto>> PageAsync(PersonPageQuery input)
    {
        var keyword = input.Keyword?.Trim();
        var query = repository.AsQueryable().WhereIF(!string.IsNullOrWhiteSpace(keyword), item =>
            item.Name.Contains(keyword!) || item.Department.Contains(keyword!) || item.Area.Contains(keyword!) ||
            item.Title.Contains(keyword!) || item.Contact.Contains(keyword!)).OrderBy(item => item.Name);
        RefAsync<int> total = 0;
        var items = await query.Select(item => new PersonDto
        {
            Id = item.Id, Name = item.Name, Gender = item.Gender, Department = item.Department, Area = item.Area,
            Title = item.Title, SpecialOps = item.SpecialOps, SpecialOpsValidUntil = item.SpecialOpsValidUntil,
            Inspector = item.Inspector, InspectorValidUntil = item.InspectorValidUntil,
            Calibrator = item.Calibrator, CalibratorValidUntil = item.CalibratorValidUntil,
            ProductAssurance = item.ProductAssurance, Contact = item.Contact, TestCount = item.TestCount,
            ExamPassed = item.ExamPassed
        }).ToPageListAsync(input.Page, input.PageSize, total);
        return new PageResult<PersonDto> { Items = items, Page = input.Page, PageSize = input.PageSize, Total = total };
    }

    public async Task<PersonDto> GetAsync(Guid id)
    {
        var item = await repository.AsQueryable().Where(entity => entity.Id == id).Select(entity => new PersonDto
        {
            Id = entity.Id, Name = entity.Name, Gender = entity.Gender, Department = entity.Department,
            Area = entity.Area, Title = entity.Title, SpecialOps = entity.SpecialOps,
            SpecialOpsValidUntil = entity.SpecialOpsValidUntil, Inspector = entity.Inspector,
            InspectorValidUntil = entity.InspectorValidUntil, Calibrator = entity.Calibrator,
            CalibratorValidUntil = entity.CalibratorValidUntil, ProductAssurance = entity.ProductAssurance,
            Contact = entity.Contact, TestCount = entity.TestCount, ExamPassed = entity.ExamPassed
        }).FirstAsync();
        return item ?? throw NotFound();
    }

    public async Task<Guid> CreateAsync(CreatePersonInput input)
    {
        var item = Map(input, new Person101());
        await repository.InsertAsync(item);
        return item.Id;
    }

    public async Task UpdateAsync(Guid id, UpdatePersonInput input)
    {
        var item = await FindAsync(id);
        Map(input, item); item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).ExecuteCommandAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await FindAsync(id);
        var referenced = await repository.Context.Queryable<TaskPerson101>().AnyAsync(row => row.PersonId == id) ||
            await repository.Context.Queryable<Document101>().AnyAsync(row => row.AuthorPersonId == id);
        if (referenced) throw Oops.Oh("人员已被任务或文件引用，不能删除。").StatusCode(409);
        item.IsDelete = true; item.UpdateTime = DateTime.UtcNow;
        await repository.AsUpdateable(item).UpdateColumns(row => new { row.IsDelete, row.UpdateTime }).ExecuteCommandAsync();
    }

    private async Task<Person101> FindAsync(Guid id) =>
        await repository.GetFirstAsync(item => item.Id == id) ?? throw NotFound();

    private static Person101 Map(CreatePersonInput input, Person101 item)
    {
        item.Name = input.Name.Trim(); item.Gender = input.Gender.Trim(); item.Department = input.Department.Trim();
        item.Area = input.Area.Trim(); item.Title = input.Title.Trim(); item.SpecialOps = input.SpecialOps;
        item.SpecialOpsValidUntil = input.SpecialOpsValidUntil; item.Inspector = input.Inspector;
        item.InspectorValidUntil = input.InspectorValidUntil; item.Calibrator = input.Calibrator;
        item.CalibratorValidUntil = input.CalibratorValidUntil; item.ProductAssurance = input.ProductAssurance;
        item.Contact = input.Contact.Trim(); item.TestCount = input.TestCount; item.ExamPassed = input.ExamPassed;
        return item;
    }

    private static Exception NotFound() => Oops.Oh("记录不存在。").StatusCode(404);
}
