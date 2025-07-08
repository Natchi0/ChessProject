using DAL;
using GameServer.Dtos;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Text;
using System.Text.Json;

namespace GameServer.MessageServices
{
	public class MessageBusSubscriber : IMessageBusSubscriber
	{
		private readonly IConfiguration _configuration;
		private readonly IMessageBusClient _messageBusClient;
		private readonly GameHandler _gameHandler;
		private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
		private IConnection _connection;
		private IChannel _channel;

		public MessageBusSubscriber(
			IConfiguration configuration, 
			IMessageBusClient messageBusClient, 
			GameHandler handler, 
			IDbContextFactory<AppDbContext> dbContextFactory)
		{
			_configuration = configuration;
			_messageBusClient = messageBusClient;
			_gameHandler = handler;
			_dbContextFactory = dbContextFactory;
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

			await _channel.QueueDeclareAsync(queue: "GameServerQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

			await _channel.QueueBindAsync("GameServerQueue", exchange, "move");
			await _channel.QueueBindAsync("GameServerQueue", exchange, "join");
			await _channel.QueueBindAsync("GameServerQueue", exchange, "leave");
			await _channel.QueueBindAsync("GameServerQueue", exchange, "create.game");

			var consumer = new AsyncEventingBasicConsumer(_channel);
			consumer.ReceivedAsync += async (model, ea) =>
			{
				var body = ea.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);
				await HandleMessageAsync(message, ea.RoutingKey);
			};

			await _channel.BasicConsumeAsync(queue: "GameServerQueue", autoAck: true, consumer: consumer);
		}

		private async Task HandleMessageAsync(string message, string routingKey)
		{
			switch (routingKey)
			{
				case "move":
					// Handle move message
					break;
				case "join":
					// Handle join message
					break;
				case "leave":
					// Handle leave message
					break;
				case "create.game":
					await HandleCreateGameAsync(message);
					break;
				default:
					Console.WriteLine($"Unknown routing key: {routingKey}");
					break;
			}
		}

		private async Task HandleCreateGameAsync(string message)
		{
			var createGameDto = JsonSerializer.Deserialize<CreateGameDto>(message);

			if (createGameDto == null)
			{
				Console.WriteLine("Mensaje inválido en create.game");
				return;
			}

			using var context = await _dbContextFactory.CreateDbContextAsync();

			var player1Exists = await context.Players.AnyAsync(p => p.Id == createGameDto.Player1Id);
			var player2Exists = await context.Players.AnyAsync(p => p.Id == createGameDto.Player2Id);

			if (!player1Exists || !player2Exists)
			{
				Console.WriteLine("Uno o ambos jugadores no existen");
				// TODO: ver si puedo publicar un evento de error o si sería mejor manejarlo de otra forma
				return;
			}

			var game = await _gameHandler.CreateGame(createGameDto.Player1Id, createGameDto.Player2Id);

			if (game == null)
			{
				Console.WriteLine("Error al crear el juego");
				return;
			}

			//publico el evento de juego creado
			var gameCreatedEvent = new GameCreatedDto
			{
				GameId = game.Id,
				Player1Id = (int)game.PlayerId1!,//TODO: tal vez la id en game no debería ser nullable
				Player2Id = (int)game.PlayerId2!,
				BoardState = game.BoardState,
				State = game.State,
				MatchRequestId = createGameDto.MatchRequestId,
				Event = ERoutingKey.GameCreated
			};

			await _messageBusClient.PublishGameCreatedAsync(gameCreatedEvent);
		}
	}
}
