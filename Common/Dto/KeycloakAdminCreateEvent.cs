using System.Text.Json.Serialization;

namespace Common.Dto;
public record KeycloakAdminCreateEvent(
    long Time,
    string ResourcePath, // "users/{id}"
    string Representation // "{\"username\":\"test-user243\",\"email\":\"test-user243@example.com\",\"emailVerified\":true,\"enabled\":true}"
);

public record KeycloakAdminCreateEventRepresentation(
    [property: JsonPropertyName("username")]
    string Username,
     [property: JsonPropertyName("email")]
    string Email
);