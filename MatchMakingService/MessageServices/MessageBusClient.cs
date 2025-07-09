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

		private async Task PublishMessage(string message, string routingKey)
		{
			var body = Encoding.UTF8.GetBytes(message);

			await _channel.BasicPublishAsync(
				exchange: "game.exchange",
				routingKey: routingKey,
				body: body
			);
		}

		//TODO: investigar mejor lo del dispose async
		public async Task DisposeAsync()
		{
			Console.WriteLine("Desechando MessageBusClient...");

			if (_channel.IsOpen)
			{
				await _channel.CloseAsync();
				await _channel.DisposeAsync();
			}
			if (_connection.IsOpen)
			{
				await _connection.CloseAsync();
				await _connection.DisposeAsync();
			}

			Console.WriteLine("MessageBusClient desechado correctamente.");
		}

		//Esta funcion es generica, debería utilizarla en vez de los metodos especificos
		public async Task PublishEventAsync(IEventDto eventMessage)
		{
			var message = JsonSerializer.Serialize(eventMessage);

			if (!_connection.IsOpen)
			{
				Console.WriteLine("La coneccion esta cerrada, no se puede enviar");
				return;
			}

			Console.WriteLine($"Publicando mensaje: {message} con routingKey: {eventMessage.Event}");
			try
			{
				await PublishMessage(message, eventMessage.Event);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"No se pudo enviar el mensaje. Excepcion: {ex}");
			}
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


		public async Task PublishCreateGameAsync(CreateGameDto createGameDto)
		{
			var message = JsonSerializer.Serialize(createGameDto);

			if (!_connection.IsOpen)
			{
				Console.WriteLine("La coneccion esta cerrada, no se puede enviar");
			}

			Console.WriteLine($"Publicando mensaje: {message}");

			try
			{
				await PublishMessage(message, createGameDto.Event);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"no se pudo enviar el mensaje. Excepcion: {ex}");
			}
		}
	}
}
