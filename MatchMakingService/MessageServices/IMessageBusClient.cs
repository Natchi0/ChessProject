using MatchMakingService.Dtos;

namespace MatchMakingService.MessageServices
{
	public interface IMessageBusClient
	{
		Task InitializeAsync();
		Task PublishMatchFoundAsync(MatchFoundPublishDto matchFoundPublishDto);
	}
}
