namespace GameServer.MessageServices
{
	public interface IMessageBusSubscriber
	{
		Task StartConsumingAsync();
	}
}
