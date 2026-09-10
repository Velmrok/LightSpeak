namespace Common.Dto;

public record GetUserProfileResponse(
    string UserId,
    string Username,
    string AvatarUrl,
    string Email
);