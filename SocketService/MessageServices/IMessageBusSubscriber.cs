namespace SocketService.MessageServices
{
	public interface IMessageBusSubscriber
	{
		Task StartConsumingAsync();
	}
}
