namespace LightSpeak.Tests.src;

public static class Eventually
{
    public static async Task Assert(
        Func<Task> action,
        TimeSpan timeout,
        CancellationToken ct = default,
        TimeSpan? pollInterval = null) 
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(200);
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                await action();
                return;
            }
            catch
            {
                await Task.Delay(interval, ct);
            }
            
        }

        throw new TimeoutException($"Test timeout : Condition not met within {timeout}.");
    }
}