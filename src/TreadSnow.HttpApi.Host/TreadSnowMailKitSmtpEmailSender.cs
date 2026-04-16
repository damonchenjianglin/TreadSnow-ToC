using System.Net.Mail;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;
using Volo.Abp.Emailing.Smtp;
using Volo.Abp.MailKit;
using Volo.Abp.MultiTenancy;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace TreadSnow;

/// <summary>
/// 自定义MailKit邮件发送器，支持所有主流邮箱（163、QQ、Gmail、Outlook等）。
/// 关闭SSL证书吊销检查，解决国内邮箱SSL握手失败问题。
/// </summary>
[Dependency(ServiceLifetime.Transient, ReplaceServices = true)]
public class TreadSnowMailKitSmtpEmailSender : EmailSenderBase, IMailKitSmtpEmailSender
{
    /// <summary>
    /// MailKit配置选项
    /// </summary>
    protected AbpMailKitOptions AbpMailKitOptions { get; }

    /// <summary>
    /// SMTP邮件发送配置
    /// </summary>
    protected ISmtpEmailSenderConfiguration SmtpConfiguration { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="currentTenant">当前租户</param>
    /// <param name="smtpConfiguration">SMTP配置</param>
    /// <param name="backgroundJobManager">后台任务管理器</param>
    /// <param name="abpMailKitConfiguration">MailKit配置选项</param>
    public TreadSnowMailKitSmtpEmailSender(ICurrentTenant currentTenant, ISmtpEmailSenderConfiguration smtpConfiguration, IBackgroundJobManager backgroundJobManager, IOptions<AbpMailKitOptions> abpMailKitConfiguration)
        : base(currentTenant, smtpConfiguration, backgroundJobManager)
    {
        AbpMailKitOptions = abpMailKitConfiguration.Value;
        SmtpConfiguration = smtpConfiguration;
    }

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="mail">邮件消息</param>
    protected async override Task SendEmailAsync(MailMessage mail)
    {
        using (var client = await BuildClientAsync())
        {
            var message = MimeMessage.CreateFromMailMessage(mail);
            message.MessageId = MimeUtils.GenerateMessageId();
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    /// <summary>
    /// 构建SMTP客户端，关闭证书吊销检查以兼容国内邮箱
    /// </summary>
    /// <returns>配置好的SmtpClient实例</returns>
    public async Task<SmtpClient> BuildClientAsync()
    {
        var client = new SmtpClient();

        try
        {
            // 关闭证书吊销检查，解决163、QQ等国内邮箱SSL握手失败问题
            client.CheckCertificateRevocation = false;
            client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            await ConfigureClient(client);
            return client;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    /// <summary>
    /// 配置SMTP客户端连接和认证
    /// </summary>
    /// <param name="client">SMTP客户端</param>
    protected virtual async Task ConfigureClient(SmtpClient client)
    {
        await client.ConnectAsync(
            await SmtpConfiguration.GetHostAsync(),
            await SmtpConfiguration.GetPortAsync(),
            await GetSecureSocketOption()
        );

        if (await SmtpConfiguration.GetUseDefaultCredentialsAsync())
        {
            return;
        }

        await client.AuthenticateAsync(
            await SmtpConfiguration.GetUserNameAsync(),
            await SmtpConfiguration.GetPasswordAsync()
        );
    }

    /// <summary>
    /// 获取SSL/TLS安全连接方式，自动根据端口号选择最佳策略
    /// </summary>
    /// <returns>安全连接选项</returns>
    protected virtual async Task<SecureSocketOptions> GetSecureSocketOption()
    {
        if (AbpMailKitOptions.SecureSocketOption.HasValue)
        {
            return AbpMailKitOptions.SecureSocketOption.Value;
        }

        var port = await SmtpConfiguration.GetPortAsync();
        var enableSsl = await SmtpConfiguration.GetEnableSslAsync();

        // 根据端口号自动选择SSL模式，兼容各主流邮箱
        // 465端口：隐式SSL（163、QQ、Gmail等）
        // 587端口：STARTTLS（Outlook、Gmail等）
        // 25端口：按配置决定
        return port switch
        {
            465 => SecureSocketOptions.SslOnConnect,
            587 => SecureSocketOptions.StartTls,
            _ => enableSsl ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None
        };
    }
}
