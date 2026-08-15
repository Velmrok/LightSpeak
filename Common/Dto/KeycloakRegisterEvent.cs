
namespace Common.Dto;
public record KeycloakRegisterEvent(
    long Time,
    string UserId,
    KeycloakRegisterEventDetails Details
);

public record KeycloakRegisterEventDetails(
    string Email,
    string Username
);

