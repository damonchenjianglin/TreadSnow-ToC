using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using TreadSnow.Accounts;
using TreadSnow.Pets;
using TreadSnow.UploadFiles;

namespace TreadSnow.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class TreadSnowDbContext :
    AbpDbContext<TreadSnowDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region �Զ������DbSet
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Pet> Pets { get; set; }
    public DbSet<UploadFile> UploadFiles { get; set; }

    #endregion 

    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public TreadSnowDbContext(DbContextOptions<TreadSnowDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        #region �Զ�����ṹ

        //��Ա��
        builder.Entity<Account>(b =>
        {
            b.ToTable(TreadSnowConsts.DbTablePrefix + "Accounts", TreadSnowConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId); //�⻧id
            b.Property(x => x.No).UseIdentityColumn(1000, 1); //���
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);  //����
            b.Property(x => x.Phone).IsRequired().HasMaxLength(64); //�ֻ�����
            b.Property(x => x.Email).IsRequired().HasMaxLength(64); //����
            b.Property(x => x.OpenId).HasMaxLength(64); //OpenId
            b.Property(x => x.Description).HasMaxLength(1000); //����
        });

        //�����
        builder.Entity<Pet>(b =>
        {
            b.ToTable(TreadSnowConsts.DbTablePrefix + "Pets", TreadSnowConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId); //�⻧id
            b.Property(x => x.No).UseIdentityColumn(1000, 1); //���
            b.Property(x => x.Name).IsRequired().HasMaxLength(64).IsRequired();  //����
            b.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).IsRequired(); //����
        });

        //������
        builder.Entity<UploadFile>(b =>
        {
            b.ToTable(TreadSnowConsts.DbTablePrefix + "UploadFiles", TreadSnowConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId); //�⻧id
            b.Property(x => x.EntityName).IsRequired().HasMaxLength(64);  //ʵ������
            b.Property(x => x.RecordId).IsRequired().HasMaxLength(64); //��¼id
            b.Property(x => x.Name).IsRequired().HasMaxLength(64); //����
            b.Property(x => x.Type).IsRequired().HasMaxLength(64); //�ļ�����
            b.Property(x => x.Path).IsRequired().HasMaxLength(1000); //·��
        });
        #endregion
    }
}
