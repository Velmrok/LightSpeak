using Common.Dto;

namespace Common.Mappers;

public static class ServersClientMapper
{
    public static Dto.GetServersResponse MapToDomainDto(this Protos.GetServersResponse response)
    {
        return new (
            Servers: [.. response.Servers.Select(s => new ServerSummary(s.Id, s.Name, s.AvatarUrl))]
        );
    }
    public static Protos.GetServersResponse MapToGrpcDto(this Dto.GetServersResponse response)
    {
        return new ()
        {
            Servers = { response.Servers.Select(s => new Protos.ServerSummary { Id = s.Id, Name = s.Name, AvatarUrl = s.AvatarUrl }) }
        };
    }
}