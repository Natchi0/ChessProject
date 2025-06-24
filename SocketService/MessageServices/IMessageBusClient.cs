using SocketService.Dtos;

namespace SocketService.MessageServices
{
	public interface IMessageBusClient
	{
		Task InitializeAsync();
		Task PublishNewMatchRequest(RequestMatchPublishDto requestMatchPublishDto);
	}
}
