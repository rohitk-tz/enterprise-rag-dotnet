using Application;
using Hangfire;
using Infrastructure;
using Infrastructure.Observability;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.AddProcessor(new ActivityEventLogProcessor());
});
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("RagMigration.Workers"))
    // Endpoint comes from OTEL_EXPORTER_OTLP_ENDPOINT (default http://localhost:4317, gRPC).
    // Hangfire instrumentation wraps each job execution in a span, so a job's HTTP calls and logs
    // share one trace. The Api registers it too, so a job enqueued from a request continues that
    // request's trace; jobs enqueued without an active span start their own root trace.
    .WithTracing(tracing => tracing
        .AddHangfireInstrumentation(options => options.RecordException = true)
        .AddHttpClientInstrumentation()
        // One span per SQL command Npgsql executes, which covers every EF Core query.
        .AddNpgsql()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics.AddHttpClientInstrumentation().AddConsoleExporter());
builder.Services.AddHangfireServer();

var host = builder.Build();
host.Run();
