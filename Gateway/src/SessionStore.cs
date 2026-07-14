using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace Gateway.src;
public class SessionStore(IDistributedCache cache) : ITicketStore
{
    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        await SaveToCache(key, ticket);
    }
    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        byte[]? bytes = await cache.GetAsync(key);
        return bytes is not null ? TicketSerializer.Default.Deserialize(bytes) : null;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        Guid id = Guid.NewGuid();
        await SaveToCache(id.ToString(), ticket);
        return id.ToString();
    }
    private async Task SaveToCache(string id, AuthenticationTicket ticket)
    {
        byte[] bytes = TicketSerializer.Default.Serialize(ticket);
        DistributedCacheEntryOptions opts = new();
        if(ticket.Properties.ExpiresUtc is {} exp)
        {
            opts.SetAbsoluteExpiration(exp);
        }
        await cache.SetAsync(id, bytes, opts);
    }
}