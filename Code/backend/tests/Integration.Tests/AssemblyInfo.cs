using Xunit;

// Integration tests spin up real ASP.NET Core hosts via WebApplicationFactory<Program>, which
// re-invokes the Program.cs entry point (including Serilog's static Log.Logger bootstrap/
// close-and-flush) per factory instance. Running test classes concurrently races on that shared
// static logger and on host startup, so collection parallelization is disabled for this assembly.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
