
using MatchMakingService;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using SocketService.Dtos;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace SocketService.MessageServices
{
	public class MessageBusSubscriber : IMessageBusSubscriber
	{
		private readonly IConfiguration _configuration;
		private readonly IHubContext<MainHub> _hub;
		private IConnection _connection;
		private IChannel _channel;
		private readonly Dictionary<string, Func<string, Task>> _handlers;

		public MessageBusSubscriber(IConfiguration configuration, IHubContext<MainHub> hubContext)
		{
			_configuration = configuration;
			_hub = hubContext;

			//manejo de eventos de mensajes mediante un diccionario de handlers para evitar usar switch
			_handlers = new Dictionary<string, Func<string, Task>>
			{
				{ RoutingKey.MatchFound, HandleMatchFoundAsync },
				{ RoutingKey.GameUpdated, async message => Console.WriteLine($"Game updated event received: {message}") },
				{ RoutingKey.MoveRejected, async message => Console.WriteLine($"Move rejected event received: {message}") },
				{ RoutingKey.GameCreated, HandleGameCreatedAsync }
			};
		}

		public async Task StartConsumingAsync()
		{
			var factory = new ConnectionFactory { 
				HostName = _configuration["RabbitMQ:HostName"]!,
				Port = int.Parse(_configuration["RabbitMQ:Port"]!) 
			};

			_connection = await factory.CreateConnectionAsync();
			_channel = await _connection.CreateChannelAsync();

			const string exchange = "game.exchange";
			await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Direct, durable: true);
			//todo: cambiar durable a true
			await _channel.QueueDeclareAsync(queue: "SocketQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

			//vincular al exchange las colas de las que SocketService va a consumir
			await _channel.QueueBindAsync("SocketQueue", exchange, "match.found");
			await _channel.QueueBindAsync("SocketQueue", exchange, "game.updated");
			await _channel.QueueBindAsync("SocketQueue", exchange, "move.rejected");
			await _channel.QueueBindAsync("SocketQueue", exchange, RoutingKey.GameCreated);

			var consumer = new AsyncEventingBasicConsumer(_channel);
			consumer.ReceivedAsync += async (model, ea) =>
			{
				var body = ea.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);

				await HandleMessageAsync(message, ea.RoutingKey);
			};

			await _channel.BasicConsumeAsync(queue: "SocketQueue", autoAck: true, consumer: consumer);
		}

		public void DisposeAync()
		{
			Console.WriteLine("Desechando MessageBusSubscriber...");

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

			Console.WriteLine("MessageBusSubscriber desechado correctamente.");
		}

		private async Task HandleMessageAsync(string message, string routingKey)
		{
			Console.WriteLine($"Received message: {message} with routing key: {routingKey}");

			// Verificar si hay un handler registrado para el routingKey
			var handler = _handlers.FirstOrDefault(h => h.Key == routingKey).Value;

			if (handler != null)
				await handler(message);
			
			else
				Console.WriteLine($"No se encontró un handler para la routing key: {routingKey}");

			Console.WriteLine($"Processed message: {message} with routing key: {routingKey}");
			
		}

		//este evento unicamente avisa que se hizo match para cambiar la interfaz, pero el evento que inicia el jeugo es game.created
		private async Task HandleMatchFoundAsync(string message)
		{
			Console.WriteLine("Evento MatchFound");
			try
			{
				MatchFoundPublishDto? matchFound = JsonSerializer.Deserialize<MatchFoundPublishDto>(message);

				if (matchFound == null)
					throw new Exception("MatchFoundPublishDto es null o no se pudo deserializar correctamente");

				var connection1 = ConnectionMapping.GetConnectionId(matchFound.Player1Id);
				var connection2 = ConnectionMapping.GetConnectionId(matchFound.Player2Id);

				var matchInfo = new
				{
					Player1Id = matchFound.Player1Id,
					Player2Id = matchFound.Player2Id,
				};

				if (connection1 != null)
					await _hub.Clients.Client(connection1).SendAsync("MatchFound", matchFound);

				if (connection2 != null)
					await _hub.Clients.Client(connection2).SendAsync("MatchFound", matchFound);

			}
			catch(Exception ex)
			{
				Console.WriteLine($"Error al manejar el evento MatchFound: {ex.Message}");
			}
		}

		private async Task HandleGameCreatedAsync(string message)
		{
			var gameCreated = JsonSerializer.Deserialize<GameCreatedDto>(message);

			if (gameCreated == null)
			{
				Console.WriteLine("Mensaje inválido en HandleGameCreatedAsync");
				return;
			}

			try
			{
				
				var connection1 = ConnectionMapping.GetConnectionId(gameCreated.Player1Id);
				var connection2 = ConnectionMapping.GetConnectionId(gameCreated.Player2Id);

				if (connection1 != null)
					await _hub.Clients.Client(connection1).SendAsync("GameCreated", gameCreated);

				if (connection2 != null)
					await _hub.Clients.Client(connection2).SendAsync("GameCreated", gameCreated);

			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error al manejar el evento GameCreated: {ex.Message}");
			}
		}

	}
}
