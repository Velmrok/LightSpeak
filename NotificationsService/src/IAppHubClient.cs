
using NotificationsService.src.dto;

namespace NotificationsService.src;
public interface IAppHubClient
{
    Task MessageCreated(MessageCreatedClientDto evt);
}