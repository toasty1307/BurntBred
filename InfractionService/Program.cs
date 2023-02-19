using FastEndpoints;
using FastEndpoints.Swagger;
using InfractionService.Exceptions;
using Remora.Discord.Rest;
using Remora.Discord.Rest.Extensions;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddCommandLine(args)
#if DEBUG
    .AddJsonFile("appsettings.Development.json", optional: true)
#endif
    .AddJsonFile("appsettings.Production.json", optional: true);

builder.Host.UseSerilog((b, configuration) =>
{
    var consoleLogFormat = b.Configuration["Logging:Formats:Console"] ??
                           throw new InvalidConfigException("No format string was specified for console logging");
    var fileLogFormat = b.Configuration["Logging:Formats:File"] ??
                        throw new InvalidConfigException("No format string was specified for file logging");
    configuration
#if DEBUG
        .MinimumLevel.Information()
#else
        .MinimumLevel.Warning()
#endif
        .Enrich.FromLogContext()
        .WriteTo.Console(formatter: new ExpressionTemplate(consoleLogFormat, theme: TemplateTheme.Literate))
        .WriteTo.Map(
            _ => $"{DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime):yyyy-MM-dd}",
            (v, cf) =>
            {
                cf.File(
                    new ExpressionTemplate(fileLogFormat),
                    $"./Logs/{v}.log",
                    // 32 megabytes
                    fileSizeLimitBytes: 33_554_432,
                    flushToDiskInterval: TimeSpan.FromMinutes(2.5),
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 50
                );
            },
            sinkMapCountLimit: 1);
});

builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDoc();
builder.Services.AddDiscordRest(_ =>
    (
        builder.Configuration["Discord:Token"] ?? throw new InvalidConfigException("Bot token not specified"),
        DiscordTokenType.Bot
    )
);

var app = builder.Build();

app.UseAuthorization();
app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseHttpsRedirection();
app.Run();