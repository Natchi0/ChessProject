
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
		private readonly IServiceProvider _serviceProvider;
		private IConnection _connection;
		private IChannel _channel;
		private readonly Dictionary<string, Func<string, Task>> _handlers;

		public MessageBusSubscriber(IConfiguration configuration, IServiceProvider serviceProvider)
		{
			_configuration = configuration;
			_serviceProvider = serviceProvider;

			//manejo de eventos de mensajes mediante un diccionario de handlers para evitar usar switch
			_handlers = new Dictionary<string, Func<string, Task>>
			{
				{ RoutingKey.FindMatch, HandleFindMatchAsync },
				{ RoutingKey.GameCreated, HandleGameCreated }
			};
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
			await _channel.QueueBindAsync("MatchMakingQueue", exchange, RoutingKey.FindMatch);
			await _channel.QueueBindAsync("MatchMakingQueue", exchange, RoutingKey.GameCreated);

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

			// Verificar si hay un handler registrado para el routingKey
			var handler = _handlers.FirstOrDefault(h => h.Key == routingKey).Value;

			if (handler != null)
				await handler(message);

			else
				Console.WriteLine($"No se encontró un handler para la routing key: {routingKey}");

			Console.WriteLine($"Processed message: {message} with routing key: {routingKey}");
		}

		private async Task HandleGameCreated(string message)
		{
			var gameCreated = JsonSerializer.Deserialize<GameCreatedDto>(message);

			if (gameCreated == null)
			{
				Console.WriteLine("Mensaje inválido en game.created");
				return;
			}

			try
			{
				using var scope = _serviceProvider.CreateScope();
				var matchService = scope.ServiceProvider.GetRequiredService<MatchService>();

				await matchService.FinishRequestMatchProcess(gameCreated);
			}
			catch(Exception ex)
			{
				Console.WriteLine($"Error al deserializar el GameCreatedDto");
			}
		}

		private async Task HandleFindMatchAsync(string message)
		{
			var requestMatch = JsonSerializer.Deserialize<RequestMatchPublishDto>(message);

			if (requestMatch != null)
			{
				try//try por si hay algun error en el service
				{
					using var scope = _serviceProvider.CreateScope();
					var matchService = scope.ServiceProvider.GetRequiredService<MatchService>();

					await matchService.RequestMatch(requestMatch.PlayerId);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error processing match request: {ex.Message}");
				}
			}
		}

	}
}
