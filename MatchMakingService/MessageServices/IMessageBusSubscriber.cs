namespace MatchMakingService.MessageServices
{
	public interface IMessageBusSubscriber
	{
		Task StartConsumingAsync();
	}
}
