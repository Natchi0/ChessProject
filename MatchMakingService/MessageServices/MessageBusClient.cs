using MatchMakingService.Dtos;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MatchMakingService.MessageServices
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

		public async Task PublishMatchFoundAsync(MatchFoundPublishDto matchFoundPublishDto)
		{
			var message = JsonSerializer.Serialize(matchFoundPublishDto);

			if (!_connection.IsOpen)
			{
				Console.WriteLine("La coneccion esta cerrada, no se puede enviar");
			}

			Console.WriteLine($"Publicando mensaje: {message}");

			try
			{
				await PublishMessage(message, matchFoundPublishDto.Event);
			}
			catch(Exception ex)
			{
				Console.WriteLine($"no se pudo enviar el mensaje. Excepcion: {ex}");
			}
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
	}
}
