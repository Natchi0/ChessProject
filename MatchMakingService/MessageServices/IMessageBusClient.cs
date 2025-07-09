using MatchMakingService.Dtos;

namespace MatchMakingService.MessageServices
{
	public interface IMessageBusClient
	{
		Task InitializeAsync();
		Task PublishEventAsync(IEventDto eventMessage);
		Task PublishMatchFoundAsync(MatchFoundPublishDto matchFoundPublishDto);
		Task PublishCreateGameAsync(CreateGameDto createGameDto);
	}
}
