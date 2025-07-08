using GameServer.MessageServices;

namespace GameServer.BackgroundServices
{
	public class RabbitMQInitializer : IHostedService
	{
		private readonly IMessageBusClient _messageBusClient;
		private readonly IMessageBusSubscriber _messageBusSubscriber;

		public RabbitMQInitializer(IMessageBusClient messageBusClient, IMessageBusSubscriber messageBusSubscriber)
		{
			_messageBusClient = messageBusClient;
			_messageBusSubscriber = messageBusSubscriber;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await _messageBusClient.InitializeAsync();
			await _messageBusSubscriber.StartConsumingAsync();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
