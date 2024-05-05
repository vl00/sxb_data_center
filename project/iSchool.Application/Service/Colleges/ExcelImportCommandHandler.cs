using Dapper;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using MediatR;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static iSchool.Infrastructure.ObjectHelper;

namespace iSchool.Application.Service.CollegeDirectory
{
    public class ExcelImportCommandHandler : IRequestHandler<ExcelImportCommand, bool>
    {
        UnitOfWork unitOfWork;
        IRepository<College> repository;

        public ExcelImportCommandHandler(IUnitOfWork unitOfWork, IRepository<College> repository)
        {
            this.unitOfWork = unitOfWork as UnitOfWork;
            this.repository = repository;
        }

        public Task<bool> Handle(ExcelImportCommand request, CancellationToken cancellationToken)
        {
            using (var package = new ExcelPackage())
            {
                package.Load(request.Excel);

                var worksheet = package.Workbook.Worksheets.First(); //sheet1

                var ls = new List<College>();
                for (var i = 2; i <= worksheet.Dimension.Rows; i++)
                {
                    var college = new College();
                    college.Id = Guid.NewGuid();
                    college.Createor = request.UserId;
                    college.Modifier = request.UserId;
                    college.IsValid = true;
                    college.Weight = 99;
                    college.Name = worksheet.Cells[$"A{i}"].Value?.ToString();
                    college.Name_e = worksheet.Cells[$"B{i}"].Value?.ToString();
                    college.Nation = worksheet.Cells[$"D{i}"].Value?.ToString();

                    var cell_c = worksheet.Cells[$"C{i}"].Value?.ToString().Trim();
                    college.Type = cell_c == "国内" ? (byte)1 :
                        cell_c == "国际" ? (byte)2 :
                        (byte)0;

                    //忽略中文名为空的大学 
                    if (string.IsNullOrWhiteSpace(college.Name)) continue;

                    ls.Add(college);
                }

                try
                {
                    unitOfWork.BeginTransaction();

                    foreach (var college in ls)
                    {
                        unitOfWork.DbConnection.TryOpen()
                            .Execute($@"
if not exists(select 1 from [dbo].[College] where name=@Name) begin
    insert [dbo].[College] (id,name,name_e,type,weight,nation,CreateTime,Createor,ModifyDateTime,Modifier,IsValid)
        values(@Id,@Name,@Name_e,@Type,@Weight,@Nation,@CreateTime,@Createor,@ModifyDateTime,@Modifier,@IsValid)
end else begin
    update [dbo].[College] set name=@Name,name_e=@Name_e,type=@Type,weight=@Weight,nation=@Nation,ModifyDateTime=@ModifyDateTime,Modifier=@Modifier,IsValid=@IsValid
    where name=@Name
end
", college, unitOfWork.DbTransaction);
                    }

                    unitOfWork.CommitChanges();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    throw ex;
                }

                return Task.FromResult(true);
            }
        }
    }
}
