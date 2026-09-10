namespace Common.Mappers;


public static class ProfileClientMapper
{
    public static Dto.GetUserSnapshotResponse MapToDomainDto(this Protos.GetUserSnapshotResponse response)
    {
        return new (
            UserId: response.UserId,
            Username: response.Username,
            AvatarUrl: response.ProfilePictureUrl
        );
    }
    public static Protos.GetUserSnapshotResponse MapToGrpcDto(this Dto.GetUserSnapshotResponse response)
    {
        return new Protos.GetUserSnapshotResponse
        {
            UserId = response.UserId,
            Username = response.Username,
            ProfilePictureUrl = response.AvatarUrl
        };
    }
}