namespace Common.Dto;

public record GetUserSnapshotResponse(
    string UserId,
    string Username,
    string AvatarUrl
);