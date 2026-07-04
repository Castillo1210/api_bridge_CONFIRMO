namespace Confirmo.Api.Services;

public class PythonWorkerClient : IPythonWorkerClient
{
    private readonly HttpClient _http;
    private readonly ILogger<PythonWorkerClient> _logger;

    public PythonWorkerClient(HttpClient http, IConfiguration config, ILogger<PythonWorkerClient> logger)
    {
        _http = http;
        _http.BaseAddress = new Uri(config["PythonWorker:BaseUrl"]!);
        _logger = logger;
    }

    public async Task EnqueueProcessAsync(string depositId)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/process-deposit", new { deposit_id = depositId});
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Job encolado en Python Worker para deposit {DepositId}", depositId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encolando job para deposit {DepositId}", depositId);
            throw;
        }
    }
}