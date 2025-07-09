using GameServer.Dtos;

namespace GameServer.MessageServices
{
	public interface IMessageBusClient
	{
		Task InitializeAsync();
		Task PublishEventAsync(IEventDto eventMessage);
		Task PublishGameCreatedAsync(GameCreatedDto gameCreatedDto);
	}
}
