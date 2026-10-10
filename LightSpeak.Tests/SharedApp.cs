namespace LightSpeak.Tests;
public static class SharedApp
{
    private static readonly Lazy<Task<AppFixture>> Instance = new(async () =>
    {
        var fixture = new AppFixture();
        await fixture.InitializeAsync();
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            fixture.DisposeAsync().GetAwaiter().GetResult();

        return fixture;
    });

    public static Task<AppFixture> GetAsync() => Instance.Value;
}