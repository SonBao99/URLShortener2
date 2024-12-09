using StackExchange.Redis;
using UrlShortener.Messaging.Services;
var builder = WebApplication.CreateBuilder(args);

// Add Redis configuration
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));
builder.Services.AddSingleton<IRedisService, RedisService>();

// Add RabbitMQ service
builder.Services.AddSingleton<IRabbitMQService>(sp =>
{
    var isDevelopment = builder.Environment.IsDevelopment();
    var host = isDevelopment ? "localhost" : "rabbitmq";
    var connectionString = $"amqp://guest:guest@{host}:5672";
    return new RabbitMQService(connectionString);
});

// Configure RabbitMQ subscribers
builder.Services.AddHostedService<RabbitMQSubscriber>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
