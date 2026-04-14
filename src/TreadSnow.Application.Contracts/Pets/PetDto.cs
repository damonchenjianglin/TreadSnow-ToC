using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Pets
{
    public class PetDto : EntityDto<Guid>
    {
        public Guid? TenantId { get; set; }
        public int No { get; set; }
        public string Name { get; set; }
        public Guid AccountId { get; set; }
    }
}