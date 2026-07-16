using System.Globalization;
using System.Text.Json;
using Spectre.Console;

namespace Usuario_Cliente.Services;

public class CajeroConsole
{
    private const decimal LIMITE_DEPOSITO = 2000m;
    private const decimal LIMITE_RETIRO = 800m;

    private readonly CajeroApiClient _apiClient;
    private string _cuenta = string.Empty;
    private string _pin = string.Empty;
    private string _titular = string.Empty;
    private string _sessionId = string.Empty;

    public CajeroConsole(CajeroApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            DrawTitle();

            string option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Seleccione una opción:")
                    .AddChoices("Insertar tarjeta", "Salir"));

            if (option == "Salir")
            {
                AnsiConsole.MarkupLine("[green]Hasta luego.[/]");
                return;
            }

            if (await AuthenticateAsync())
            {
                await RunSessionAsync();
            }
        }
    }

    private async Task<bool> AuthenticateAsync()
    {
        _cuenta = AnsiConsole.Ask<string>("Ingrese el número de cuenta/tarjeta:");
        _pin = AnsiConsole.Prompt(new TextPrompt<string>("Ingrese su PIN").Secret());

        string authResult = await _apiClient.AutenticarAsync(_cuenta, _pin);
        if (authResult.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase) || authResult.Contains("error", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine("[red]Autenticación fallida.[/]");
            AnsiConsole.WriteLine(authResult);
            AnsiConsole.MarkupLine("[grey]Presione Enter para continuar...[/]");
            Console.ReadLine();
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(authResult);
            _titular = document.RootElement.GetProperty("titular").GetString() ?? string.Empty;
            _sessionId = Guid.NewGuid().ToString();
        }
        catch
        {
            _titular = string.Empty;
            _sessionId = Guid.NewGuid().ToString();
        }

        return true;
    }

    private async Task RunSessionAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            DrawSessionHeader();

            string option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Seleccione una opción:")
                    .AddChoices("Consultar saldo", "Retirar efectivo", "Depositar efectivo", "Consultar movimientos", "Transferir dinero", "Retirar tarjeta"));

            switch (option)
            {
                case "Consultar saldo":
                    await ConsultarSaldoAsync();
                    break;
                case "Depositar efectivo":
                    await DepositarAsync();
                    break;
                case "Retirar efectivo":
                    await RetirarAsync();
                    break;
                case "Consultar movimientos":
                    await MostrarHistorialAsync();
                    break;
                case "Transferir dinero":
                    await TransferirAsync();
                    break;
                case "Retirar tarjeta":
                    return;
            }

            AnsiConsole.MarkupLine("[grey]Presione Enter para continuar...[/]");
            Console.ReadLine();
        }
    }

    private void DrawTitle()
    {
        Panel title = new Panel("[bold cyan]CAJERO AUTOMATICO BANCO RUBY[/]")
            .Header("[green]Banco Ruby[/]")
            .Border(BoxBorder.Double)
            .Expand();

        AnsiConsole.Write(title);
        AnsiConsole.WriteLine();
    }

    private void DrawSessionHeader()
    {
        Table headerTable = new Table().Expand().HideHeaders();
        headerTable.AddColumn(new TableColumn("Info").NoWrap());
        headerTable.AddColumn(new TableColumn("Valor"));
        headerTable.AddRow("[bold green]Sesion activa:[/]", _sessionId);
        headerTable.AddRow("[bold green]Tarjeta:[/]", MaskCard(_cuenta));
        headerTable.AddRow("[bold green]Cuenta:[/]", _cuenta);
        headerTable.AddRow("[bold green]Titular:[/]", _titular);

        Panel panel = new Panel(headerTable)
            .Border(BoxBorder.Double)
            .Header("[bold cyan]CAJERO AUTOMATICO BANCO RUBY[/]");

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static string MaskCard(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length <= 4)
            return cardNumber;

        string visible = cardNumber[^4..];
        return new string('*', cardNumber.Length - 4) + visible;
    }

    private static string FormatAccountLabel(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return string.Empty;

        return accountNumber.StartsWith("RUBY-", StringComparison.OrdinalIgnoreCase)
            ? accountNumber
            : $"RUBY-{accountNumber[^4..]}";
    }

    private async Task ConsultarSaldoAsync()
    {
        string result = await _apiClient.ConsultarSaldoAsync(_cuenta);
        if (result.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(result)}[/]");
            return;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(result);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("error", out JsonElement error))
            {
                AnsiConsole.MarkupLine($"[red]ERROR: {Markup.Escape(error.GetString() ?? string.Empty)}[/]");
                return;
            }

            decimal saldo = root.GetProperty("saldo").GetDecimal();
            string titular = root.GetProperty("titular").GetString() ?? string.Empty;

            Table table = new Table().Border(TableBorder.Rounded).Title("Saldo");
            table.AddColumn("Cuenta");
            table.AddColumn("Titular");
            table.AddColumn("Saldo");
            table.AddRow(FormatAccountLabel(_cuenta), Markup.Escape(titular), $"${saldo:N2}");
            AnsiConsole.Write(table);
        }
        catch
        {
            AnsiConsole.WriteLine(result);
        }
    }

    private async Task DepositarAsync()
    {
        decimal monto = AnsiConsole.Ask<decimal>($"Monto a depositar (máx {LIMITE_DEPOSITO:N0}):");
        if (monto <= 0)
        {
            AnsiConsole.MarkupLine("[red]Monto inválido.[/]");
            return;
        }

        if (monto > LIMITE_DEPOSITO)
        {
            AnsiConsole.MarkupLine($"[red]ERROR: Límite de depósito {LIMITE_DEPOSITO:N0}.[/]");
            return;
        }

        string result = await _apiClient.DepositarAsync(_cuenta, monto);
        if (result.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(result)}[/]");
            return;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(result);
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("error", out JsonElement err))
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(err.GetString() ?? string.Empty)}[/]");
                return;
            }

            string? mensaje = root.TryGetProperty("mensaje", out JsonElement m) ? m.GetString() : null;
            decimal saldo = root.TryGetProperty("saldo", out JsonElement s) ? s.GetDecimal() : (root.TryGetProperty("saldoOrigen", out JsonElement so) ? so.GetDecimal() : 0m);

            if (!string.IsNullOrEmpty(mensaje)) AnsiConsole.MarkupLine($"[green]{Markup.Escape(mensaje)}[/]");
            AnsiConsole.MarkupLine($"[bold]Saldo actual:[/] [yellow]${saldo:N2}[/]");
        }
        catch
        {
            AnsiConsole.WriteLine(result);
        }
    }

    private async Task RetirarAsync()
    {
        decimal monto = AnsiConsole.Ask<decimal>($"Monto a retirar (máx {LIMITE_RETIRO:N0}):");
        if (monto <= 0)
        {
            AnsiConsole.MarkupLine("[red]Monto inválido.[/]");
            return;
        }

        if (monto > LIMITE_RETIRO)
        {
            AnsiConsole.MarkupLine($"[red]ERROR: Límite de retiro {LIMITE_RETIRO:N0}.[/]");
            return;
        }

        string result = await _apiClient.RetirarAsync(_cuenta, monto);
        if (result.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(result)}[/]");
            return;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(result);
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("error", out JsonElement err))
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(err.GetString() ?? string.Empty)}[/]");
                return;
            }

            string? mensaje = root.TryGetProperty("mensaje", out JsonElement m) ? m.GetString() : null;
            decimal saldo = root.TryGetProperty("saldo", out JsonElement s) ? s.GetDecimal() : (root.TryGetProperty("saldoOrigen", out JsonElement so) ? so.GetDecimal() : 0m);

            if (!string.IsNullOrEmpty(mensaje)) AnsiConsole.MarkupLine($"[green]{Markup.Escape(mensaje)}[/]");
            AnsiConsole.MarkupLine($"[bold]Saldo actual:[/] [yellow]${saldo:N2}[/]");
        }
        catch
        {
            AnsiConsole.WriteLine(result);
        }
    }

    private async Task MostrarHistorialAsync()
    {
        string result = await _apiClient.ObtenerHistorialAsync(_cuenta);
        if (result.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(result)}[/]");
            return;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(result);
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("error", out JsonElement err))
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(err.GetString() ?? string.Empty)}[/]");
                return;
            }

            string titular = root.GetProperty("titular").GetString() ?? string.Empty;
            List<JsonElement> historial = root.GetProperty("historial").EnumerateArray().ToList();

            if (historial.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No hay movimientos registrados.[/]");
                return;
            }

            Table table = new Table()
                .Border(TableBorder.Rounded)
                .Title($"[bold cyan]Movimientos — {Markup.Escape(titular)}[/]")
                .Expand();

            table.AddColumn(new TableColumn("[bold]#[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Movimiento[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Descripción[/]").LeftAligned());
            table.AddColumn(new TableColumn("[bold]Valor[/]").RightAligned());
            table.AddColumn(new TableColumn("[bold]Fecha[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Hora[/]").Centered());

            int numero = 1;
            foreach (JsonElement item in historial)
            {
                // Lee "tipo" o "Tipo" según cómo venga el JSON del servidor
                string tipo = string.Empty;
                if (item.TryGetProperty("tipo", out JsonElement tp)) tipo = tp.GetString() ?? string.Empty;
                else if (item.TryGetProperty("Tipo", out JsonElement tp2)) tipo = tp2.GetString() ?? string.Empty;

                // Lee "monto" o "Monto"
                decimal monto = 0m;
                if (item.TryGetProperty("monto", out JsonElement mn)) monto = mn.GetDecimal();
                else if (item.TryGetProperty("Monto", out JsonElement mn2)) monto = mn2.GetDecimal();

                // Lee "descripcion" o "Descripcion"
                string desc = string.Empty;
                if (item.TryGetProperty("descripcion", out JsonElement dc)) desc = dc.GetString() ?? string.Empty;
                else if (item.TryGetProperty("Descripcion", out JsonElement dc2)) desc = dc2.GetString() ?? string.Empty;

                // Lee "creadoEn" o "CreadoEn"
                DateTime fechaRaw = DateTime.MinValue;
                if (item.TryGetProperty("creadoEn", out JsonElement fe)) fechaRaw = fe.GetDateTime();
                else if (item.TryGetProperty("CreadoEn", out JsonElement fe2)) fechaRaw = fe2.GetDateTime();

                DateTime fechaLocal = fechaRaw.ToLocalTime();

                string color = tipo.ToLowerInvariant() switch
                {
                    string t when t.Contains("withdrawal") || t.Contains("retiro") => "red",
                    string t when t.Contains("deposit") || t.Contains("dep") => "green",
                    string t when t.Contains("transferencia salida") || t.Contains("transferencia enviada") => "red",
                    string t when t.Contains("transferencia entrada") || t.Contains("transferencia recibida") => "green",
                    _ => "white"
                };

                string etiqueta = tipo.ToLowerInvariant() switch
                {
                    string t when t.Contains("withdrawal") || t.Contains("retiro") => "Retiro",
                    string t when t.Contains("deposit") || t.Contains("dep") => "Depósito",
                    string t when t.Contains("transferencia salida") || t.Contains("transferencia enviada") => "Transferencia enviada",
                    string t when t.Contains("transferencia entrada") || t.Contains("transferencia recibida") => "Transferencia recibida",
                    _ => Markup.Escape(tipo)
                };

                table.AddRow(
                    $"[{color}]{numero}[/]",
                    $"[{color}]{etiqueta}[/]",
                    $"[{color}]{Markup.Escape(desc)}[/]",
                    $"[{color}]${monto:N2}[/]",
                    $"[{color}]{fechaLocal:dd/MM/yyyy}[/]",
                    $"[{color}]{fechaLocal:HH:mm:ss}[/]"
                );

                numero++;
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]●[/] Depósito / Transferencia recibida   [red]●[/] Retiro / Transferencia enviada");
        }
        catch
        {
            AnsiConsole.WriteLine(result);
        }
    }

    private async Task TransferirAsync()
    {
        string destino = AnsiConsole.Ask<string>("Cuenta destino:");
        if (string.IsNullOrWhiteSpace(destino))
        {
            AnsiConsole.MarkupLine("[red]Cuenta destino inválida.[/]");
            return;
        }

        string banco = AnsiConsole.Ask<string>("Banco destino (opcional):");
        string concepto = AnsiConsole.Ask<string>("Concepto (opcional):");
        decimal monto = AnsiConsole.Ask<decimal>("Monto a transferir:");

        if (monto <= 0)
        {
            AnsiConsole.MarkupLine("[red]Monto inválido.[/]");
            return;
        }

        string result = await _apiClient.TransferirAsync(_cuenta, destino, banco, monto, concepto);
        if (result.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(result)}[/]");
            return;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(result);
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("error", out JsonElement err))
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(err.GetString() ?? string.Empty)}[/]");
                return;
            }

            string? mensaje = root.TryGetProperty("mensaje", out JsonElement m) ? m.GetString() : null;
            if (!string.IsNullOrEmpty(mensaje)) AnsiConsole.MarkupLine($"[green]{Markup.Escape(mensaje)}[/]");
        }
        catch
        {
            AnsiConsole.WriteLine(result);
        }
    }
}