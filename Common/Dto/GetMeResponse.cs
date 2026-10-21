namespace Common.Dto;

public record GetMeResponse(
    string Id,
    string Username,
    string AvatarUrl,
    string Email
);