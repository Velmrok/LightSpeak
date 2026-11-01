namespace Common.Dto;

public record GetServersResponse(
    List<ServerSummary> Servers
);