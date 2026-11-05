using Common.Dto;

namespace ServersService.src.dto;

public record GetServerDetailsResponse(string ServerId, string Name, string AvatarUrl, List<ChannelSummary> Channels, List<UserSummary> Members);