using System.Net.Http.Json;
using System.Text.Json;

namespace Usuario_Cliente.Services;

public class CajeroApiClient
{
    private readonly HttpClient _http;

    public CajeroApiClient(string baseUrl)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<string> AutenticarAsync(string numero, string pin)
    {
        try
        {
            HttpResponseMessage res = await _http.PostAsJsonAsync($"/api/cuentas/{numero}/autenticar", new { Pin = pin });
            string content = await res.Content.ReadAsStringAsync();
            return res.IsSuccessStatusCode ? content : $"ERROR: {content}";
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.Message;
        }
    }

    public async Task<string> ConsultarSaldoAsync(string numero)
    {
        try
        {
            HttpResponseMessage res = await _http.GetAsync($"/api/cuentas/{numero}/saldo");
            string content = await res.Content.ReadAsStringAsync();
            return res.IsSuccessStatusCode ? content : $"ERROR: {content}";
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.Message;
        }
    }

    public async Task<string> DepositarAsync(string numero, decimal monto)
    {
        try
        {
            HttpResponseMessage res = await _http.PostAsJsonAsync($"/api/cuentas/{numero}/depositar", new { Monto = monto });
            string content = await res.Content.ReadAsStringAsync();
            return res.IsSuccessStatusCode ? content : $"ERROR: {content}";
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.Message;
        }
    }

    public async Task<string> RetirarAsync(string numero, decimal monto)
    {
        try
        {
            HttpResponseMessage res = await _http.PostAsJsonAsync($"/api/cuentas/{numero}/retirar", new { Monto = monto });
            string content = await res.Content.ReadAsStringAsync();
            return res.IsSuccessStatusCode ? content : $"ERROR: {content}";
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.Message;
        }
    }

    public async Task<string> TransferirAsync(string numeroOrigen, string cuentaDestino, string bancoDestino, decimal monto, string concepto)
    {
        try
        {
            HttpResponseMessage res = await _http.PostAsJsonAsync($"/api/cuentas/{numeroOrigen}/transferir", new { CuentaDestino = cuentaDestino, Banco = bancoDestino, Monto = monto, Concepto = concepto });
            string content = await res.Content.ReadAsStringAsync();
            return res.IsSuccessStatusCode ? content : $"ERROR: {content}";
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.Message;
        }
    }

    public async Task<string> ObtenerHistorialAsync(string numero)
    {
        try
        {
            HttpResponseMessage res = await _http.GetAsync($"/api/cuentas/{numero}/historial");
            string content = await res.Content.ReadAsStringAsync();
            return res.IsSuccessStatusCode ? content : $"ERROR: {content}";
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.Message;
        }
    }
}
