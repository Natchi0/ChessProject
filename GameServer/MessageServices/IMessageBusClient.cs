using GameServer.Dtos;

namespace GameServer.MessageServices
{
	public interface IMessageBusClient
	{
		Task InitializeAsync();
		Task PublishGameCreatedAsync(GameCreatedDto gameCreatedDto);
	}
}
