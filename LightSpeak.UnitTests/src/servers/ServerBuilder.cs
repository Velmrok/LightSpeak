using ServersService.src;
using ServersService.src.models;
using ServersService.src.permissions;

namespace LightSpeak.UnitTests.src.servers;

public class ServerBuilder
{
    private readonly string _id = $"server-{Guid.NewGuid():N}";
    private readonly Role _everyone;
    private readonly List<Role> _roles = [];
    private readonly List<Member> _members = [];
    private readonly List<Channel> _channels = [];
    private readonly List<UserSnapshot> _users = [];

    public ServerBuilder()
    {
        _everyone = DefaultRoles.CreateEveryone(_id);
        _roles.Add(_everyone);
    }

    public string Id => _id;
    public Role Everyone => _everyone;

    public ServerBuilder EveryonePermissions(Permission p)
    {
        _everyone.Permissions = p;
        return this;
    }

    public Role AddRole(string name, Permission perms, int priority = 1)
    {
        var role = new Role { Name = name, Permissions = perms, ServerId = _id, Priority = priority };
        _roles.Add(role);
        return role;
    }

    public ServerBuilder AddMember(string userId, params Role[] extraRoles)
    {
        _users.Add(new UserSnapshot { Id = userId, Name = userId, AvatarUrl = "" });
        _members.Add(new Member
        {
            UserId = userId,
            ServerId = _id,
            Roles = [_everyone, .._extraRoles(extraRoles)]
        });
        return this;
    }

    private static IEnumerable<Role> _extraRoles(Role[] r) => r;

    public Channel AddChannel(string name = "general")
    {
        var ch = new Channel { Id = $"channel-{Guid.NewGuid():N}", Name = name, ServerId = _id };
        _channels.Add(ch);
        return ch;
    }

    public Server Build() => new()
    {
        Id = _id,
        Name = "Test Server",
        Roles = _roles,
        Members = _members,
        Channels = _channels
    };

    public async Task<Server> SaveAsync(AppDbContext db)
    {
        db.UserSnapshots.AddRange(_users);
        var server = Build();
        db.Servers.Add(server);
        await db.SaveChangesAsync();
        return server;
    }
}