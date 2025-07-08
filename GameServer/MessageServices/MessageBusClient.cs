using GameServer.Dtos;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace GameServer.MessageServices
{
	public class MessageBusClient : IMessageBusClient
	{
		private readonly IConfiguration _configuration;
		private IConnection _connection;
		private IChannel _channel;

		public MessageBusClient(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task InitializeAsync()
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

			Console.WriteLine("MatchMakingService RabbitMQ Initialized");
		}

		private async Task PublishMessage(string message, string routingKey)
		{
			var body = Encoding.UTF8.GetBytes(message);

			await _channel.BasicPublishAsync(
				exchange: "game.exchange",
				routingKey: routingKey,
				body: body
			);
		}

		public void DisposeAsync()
		{
			Console.WriteLine("Desechando MessageBusClient...");

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

			Console.WriteLine("MessageBusClient desechado correctamente.");
		}

		public async Task PublishGameCreatedAsync(GameCreatedDto gameCreatedDto)
		{
			var message = JsonSerializer.Serialize(gameCreatedDto);

			if (!_connection.IsOpen)
			{
				Console.WriteLine("La coneccion esta cerrada, no se puede enviar");
			}

			try
			{
				await PublishMessage(message, gameCreatedDto.Event);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"no se pudo enviar el mensaje. Excepcion: {ex}");
			}
		}
	}
}
