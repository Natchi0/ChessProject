
using MatchMakingService.Dtos;
using MatchMakingService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace MatchMakingService.MessageServices
{
	public class MessageBusSubscriber : IMessageBusSubscriber
	{
		private readonly IConfiguration _configuration;
		private readonly MatchService _matchService;
		private IConnection _connection;
		private IChannel _channel;

		public MessageBusSubscriber(IConfiguration configuration, MatchService matchService)
		{
			_configuration = configuration;
			_matchService = matchService;
		}

		public async Task StartConsumingAsync()
		{
			var factory = new ConnectionFactory
			{
				HostName = _configuration["RabbitMQ:HostName"]!,
				Port = int.Parse(_configuration["RabbitMQ:Port"]!)
			};
			_connection = await factory.CreateConnectionAsync();
			_channel = await _connection.CreateChannelAsync();

			const string exchange = "game.exchange";
			await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Direct, durable: true);

			// Declarar cola para recibir mensajes
			await _channel.QueueDeclareAsync(queue: "MatchMakingQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

			//vincular colas
			await _channel.QueueBindAsync("MatchMakingQueue", exchange, "find.match");

			var consumer = new AsyncEventingBasicConsumer(_channel);
			consumer.ReceivedAsync += async (model, ea) =>
			{
				var body = ea.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);

				await HanddleMessageAsync(message, ea.RoutingKey);
			};

			await _channel.BasicConsumeAsync(queue: "MatchMakingQueue", autoAck: true, consumer: consumer);
		}

		private async Task HanddleMessageAsync(string message, string routingKey)
		{
			Console.WriteLine($"Received message: {message} with routing key: {routingKey}");
			switch (routingKey)
			{
				case "find.match":
					var requestMatch = JsonSerializer.Deserialize<RequestMatchPublishDto>(message);

					if(requestMatch != null)
					{
						await _matchService.RequestMatch(requestMatch.PlayerId, requestMatch.ConnectionId)
							.ContinueWith(task =>
							{
								if (task.IsFaulted)
								{
									Console.WriteLine($"Error processing match request: {task.Exception?.Message}");
								}
								else
								{
									Console.WriteLine($"Match request processed successfully for player {requestMatch.PlayerId}");
								}
							});
					}

					break;
				default:
					Console.WriteLine($"No handler for routing key: {routingKey}");
					break;
			}
		}

		public void Dispose()
		{
			if (_channel.IsOpen)
			{
				_channel.CloseAsync();
				_channel.DisposeAsync();
			}
			if (_connection.IsOpen)
			{
				_connection.CloseAsync();
				_connection.DisposeAsync();
			}

			Console.WriteLine("MessageBusSubscriber disposed.");
		}
	}
}
