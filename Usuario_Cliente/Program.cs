using Usuario_Cliente.Services;

string apiBaseUrl = Environment.GetEnvironmentVariable("BANK_API_BASE_URL") ?? "http://localhost:5000";

CajeroApiClient apiClient = new CajeroApiClient(apiBaseUrl);
CajeroConsole console = new CajeroConsole(apiClient);
await console.RunAsync();
