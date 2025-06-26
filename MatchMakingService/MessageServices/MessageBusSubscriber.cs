
using ChessUtilsLib;
using DAL.Models;
using MatchMakingService.Dtos;
using MatchMakingService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Text;
using System.Text.Json;

namespace MatchMakingService.MessageServices
{
	public class MessageBusSubscriber : IMessageBusSubscriber
	{
		private readonly IConfiguration _configuration;
		private readonly IMessageBusClient _messageBusClient;
		private readonly IServiceProvider _serviceProvider;
		private IConnection _connection;
		private IChannel _channel;

		public MessageBusSubscriber(IConfiguration configuration, IMessageBusClient messageBusClient, IServiceProvider serviceProvider)
		{
			_configuration = configuration;
			_messageBusClient = messageBusClient;
			_serviceProvider = serviceProvider;
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
						using var scope = _serviceProvider.CreateScope();
						var matchService = scope.ServiceProvider.GetRequiredService<MatchService>();

						RequestMatchInfoDto matchRequest = await matchService.RequestMatch(requestMatch.PlayerId, requestMatch.ConnectionId);

						//si hay match info significa que se hizo match y hay que notificar
						if (matchRequest.MatchInfo != null)
						{
							var matchInfo = matchRequest.MatchInfo;

							MatchFoundPublishDto matchPublishDto = new MatchFoundPublishDto
							{
								Player1Id = matchInfo.Player1Id,
								Player2Id = matchInfo.Player2Id,
								GameId = matchInfo.GameId,
								BoardState = matchInfo.BoardState,
								Event = ERoutingKey.MatchFound
							};

							await _messageBusClient.PublishMatchFoundAsync(matchPublishDto);
						}
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
