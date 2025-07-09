using SocketService.Dtos;

namespace SocketService.MessageServices
{
	public interface IMessageBusClient
	{
		Task InitializeAsync();
		Task PublishEventAsync(IEventDto eventMessage);
		Task PublishNewMatchRequest(RequestMatchPublishDto requestMatchPublishDto);
	}
}
