using SocketService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddPolicy(name: "defaultPolicy", policy =>
	{
		policy.WithOrigins("http://localhost:5173")
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});

builder.Services.AddSignalR(); 

var app = builder.Build();

app.UseCors("defaultPolicy");


//app.UseHttpsRedirection();

app.MapHub<MainHub>("/mainHub");

app.MapGet("/", () => "Welcome to the Socket Service!");

app.Run();