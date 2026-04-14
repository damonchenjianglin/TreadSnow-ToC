using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Accounts
{
    public class AccountDto : EntityDto<Guid>
    {
        public Guid? TenantId { get; set; }
        public int No { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string OpenId { get; set; }
        public string? Description { get; set; }
    }
}