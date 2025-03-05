using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrKouk.Web.ERP.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GrKouk.Web.ERP.Services;

// Example hosted service that cleans up expired tokens
// must register the service
// builder.Services.AddHostedService<TokenCleanupService>();
// 
public class TokenCleanupService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceProvider _serviceProvider;

    public TokenCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Run once every hour (example)
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

            var expiredTokens = context.RefreshTokens
                .Where(rt => rt.ExpiryDate <= DateTime.UtcNow);

            context.RefreshTokens.RemoveRange(expiredTokens);
            context.SaveChanges();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}