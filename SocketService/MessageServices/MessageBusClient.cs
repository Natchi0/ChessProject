using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SocketService.Dtos;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace SocketService.MessageServices
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

			//declarar exchange
			await _channel.ExchangeDeclareAsync(exchange, ExchangeType.Direct, durable: true);

			Console.WriteLine("SocketService RabbitMQ Inicializado");
		}

		public async Task PublishNewMatchRequest(RequestMatchPublishDto requestMatchPublishDto)
		{
			var message = JsonSerializer.Serialize(requestMatchPublishDto);

			if (!_connection.IsOpen)
			{
				Console.WriteLine("La coneccion esta cerrada, no se puede enviar");
			}

			Console.WriteLine($"Publishing message: {message}");

			try
			{
				await PublishMessage(message, requestMatchPublishDto.Event);
			}
			catch(Exception ex)
			{
				Console.WriteLine($"no se pudo enviar el mensaje. Excepcion: {ex}");
			}
		}

		//funcion generica para enviar mensajes a RabbitMQ
		private async Task PublishMessage(string message, string routeKey) //routeKey para deteminar la cola a la que enviar
		{
			var body = Encoding.UTF8.GetBytes(message);

			await _channel.BasicPublishAsync(exchange: "game.exchange", routingKey: routeKey, body: body);
		}

		public void DisposeAync()
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

		//public void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
		//{
		//	Console.WriteLine("coneccion RabbitMQ cerrada.");
		//	_channel.Dispose();
		//	_connection.Dispose();
		//}
	}
}
