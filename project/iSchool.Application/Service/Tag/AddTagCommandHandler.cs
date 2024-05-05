using Dapper;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Application.Service
{
    public class AddTagCommandHandler : IRequestHandler<AddTagCommand, (Tag, bool)>
    {
        private IRepository<Tag> _tagRepository;
        private UnitOfWork _unitOfWork;
        private IMediator mediator;

        public AddTagCommandHandler(IRepository<Tag> tagRepository, IUnitOfWork unitOfWork, IMediator mediator)
        {
            _tagRepository = tagRepository;
            _unitOfWork = (UnitOfWork)unitOfWork;
            this.mediator = mediator;
        }

        public async Task<(Tag, bool)> Handle(AddTagCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var tag = _tagRepository.GetIsValid(p => p.Name == request.Name && p.Type == request.Type);
            var isnew = tag == null;

            if (isnew)
            {
                tag = new Tag();
                tag.Id = Guid.NewGuid();
                tag.Creator = request.UserId;
                tag.Modifier = request.UserId;
                tag.Sort = 99;
                tag.Type = request.Type;
                tag.Name = request.Name;
                tag.IsValid = true;                

                var firstWord = request.Name.Substring(0, 1);
                tag.SpellCode = NPinyin.Pinyin.GetInitials(firstWord).ToUpperInvariant();
                if (firstWord == tag.SpellCode) tag.SpellCode = ChineseConvert.UtilIndexCode(firstWord).ToUpperInvariant();

                _tagRepository.Insert(tag);
            }
            //_unitOfWork.DbConnection.Execute(@"
            //if exists (select 1 from Tag where name=@Name) begin
            //    update [dbo].[Tag] set IsValid=@IsValid,ModifyDateTime=@ModifyDateTime,Modifier=@Modifier where name=@Name ; --Id=@Id
            //end else begin
            //    insert [dbo].[Tag] (id,name,spellcode,type,sort,CreateTime,Creator,ModifyDateTime,Modifier,IsValid)
            //        values(@Id,@Name,@SpellCode,@Type,@Sort,@CreateTime,@Creator,@ModifyDateTime,@Modifier,@IsValid) ;
            //end
            //", tag, _unitOfWork.DbTransaction);

            _ = Task.Factory.StartNew(() => mediator.Send(new GetTagListQuery(false)));

            return (tag, isnew);

        }
    }
}
