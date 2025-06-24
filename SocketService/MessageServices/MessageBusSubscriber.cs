
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace SocketService.MessageServices
{
	public class MessageBusSubscriber : IMessageBusSubscriber
	{
		private readonly IConfiguration _configuration;
		private IConnection _connection;
		private IChannel _channel;

		public MessageBusSubscriber(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task StartConsumingAsync()
		{
			var factory = new ConnectionFactory { HostName = _configuration["RabbitMQ:HostName"]!, Port = int.Parse(_configuration["RabbitMQ:Port"]!) };
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

			var consumer = new AsyncEventingBasicConsumer(_channel);
			consumer.ReceivedAsync += async (model, ea) =>
			{
				var body = ea.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);

				await HanddleMessageAsync(message, ea.RoutingKey);
			};

			await _channel.BasicConsumeAsync(queue: "SocketQueue", autoAck: true, consumer: consumer);
		}

		private async Task HanddleMessageAsync(string message, string routingKey)
		{
			Console.WriteLine($"Received message: {message} with routing key: {routingKey}");
			switch (routingKey)
			{
				case "match.found":
					// Handle match found logic
					break;
				case "game.updated":
					// Handle game updated logic
					break;
				case "move.rejected":
					// Handle move rejected logic
					break;
				default:
					Console.WriteLine($"Unknown routing key: {routingKey}");
					break;
			}

			Console.WriteLine($"Processed message: {message} with routing key: {routingKey}");
			
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
	}
}
