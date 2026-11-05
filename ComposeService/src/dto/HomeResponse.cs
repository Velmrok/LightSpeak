using Common.Dto;

namespace ComposeService.src.dto;

public record HomeResponse(
    List<ServerSummary>? Servers,
    List<UserSummary> Friends,
    GetUserProfileResponse UserProfile
); 