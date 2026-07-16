using BancoCenit.Common;
using Microsoft.EntityFrameworkCore;

namespace BancoCenit.Features;

public interface IBankService
{
    Task<OperationResult<SaldoResponse>> ConsultarSaldoAsync(string numeroCuenta);
    Task<OperationResult<OperacionResponse>> DepositarAsync(DepositoRequest request);
    Task<OperationResult<OperacionResponse>> RetirarAsync(RetiroRequest request);
    Task<OperationResult<TransferenciaResponse>> TransferirAsync(TransferenciaRequest request);
    Task<OperationResult<HistorialResponse>> ObtenerHistorialAsync(string numeroCuenta);
}

public sealed class BankService : IBankService
{
    private readonly BancoRubyDbContext _db;
    private readonly IEventBus _eventBus;
    private readonly ITransferInterceptor _transferInterceptor;

    public BankService(BancoRubyDbContext db, IEventBus eventBus, ITransferInterceptor transferInterceptor)
    {
        _db = db;
        _eventBus = eventBus;
        _transferInterceptor = transferInterceptor;
    }

    public async Task<OperationResult<SaldoResponse>> ConsultarSaldoAsync(string numeroCuenta)
    {
        OperationResult<Cuenta> cuentaResult = await ObtenerCuentaActiva(numeroCuenta);
        if (cuentaResult.IsFailure)
        {
            return OperationResult<SaldoResponse>.Fail(cuentaResult.StatusCode, cuentaResult.Error!);
        }

        return OperationResult<SaldoResponse>.Ok(new SaldoResponse(cuentaResult.Value.Saldo, cuentaResult.Value.Usuario?.Nombre ?? string.Empty));
    }

    public async Task<OperationResult<OperacionResponse>> DepositarAsync(DepositoRequest request)
    {
        OperationResult<Cuenta> cuentaResult = await ObtenerCuentaActiva(request.NumeroCuenta);
        if (cuentaResult.IsFailure)
        {
            return OperationResult<OperacionResponse>.Fail(cuentaResult.StatusCode, cuentaResult.Error!);
        }

        OperationResult<decimal> validacion = await ValidarMontoAsync(request.Monto, "Depósito");
        if (validacion.IsFailure)
        {
            return OperationResult<OperacionResponse>.Fail(validacion.StatusCode, validacion.Error!);
        }

        return await RealizarDepositoAsync(cuentaResult.Value, request.Monto);
    }

    public async Task<OperationResult<OperacionResponse>> RetirarAsync(RetiroRequest request)
    {
        OperationResult<Cuenta> cuentaResult = await ObtenerCuentaActiva(request.NumeroCuenta);
        if (cuentaResult.IsFailure)
        {
            return OperationResult<OperacionResponse>.Fail(cuentaResult.StatusCode, cuentaResult.Error!);
        }

        OperationResult<decimal> validacion = await ValidarRetiroAsync(cuentaResult.Value, request.Monto);
        if (validacion.IsFailure)
        {
            return OperationResult<OperacionResponse>.Fail(validacion.StatusCode, validacion.Error!);
        }

        return await RealizarRetiroAsync(cuentaResult.Value, request.Monto);
    }

    public async Task<OperationResult<TransferenciaResponse>> TransferirAsync(TransferenciaRequest request)
    {
        if (request.NumeroCuentaOrigen == request.NumeroCuentaDestino)
        {
            return OperationResult<TransferenciaResponse>.BadRequest("La cuenta origen y destino no pueden ser la misma.");
        }

        OperationResult<Cuenta> origenResult = await ObtenerCuentaActiva(request.NumeroCuentaOrigen);
        if (origenResult.IsFailure)
        {
            return OperationResult<TransferenciaResponse>.Fail(origenResult.StatusCode, origenResult.Error!);
        }

        OperationResult<Cuenta> destinoResult = await ObtenerCuentaActiva(request.NumeroCuentaDestino);
        if (destinoResult.IsFailure)
        {
            return OperationResult<TransferenciaResponse>.Fail(destinoResult.StatusCode, destinoResult.Error!);
        }

        OperationResult<decimal> validacion = await ValidarTransferenciaAsync(origenResult.Value, destinoResult.Value, request.Monto);
        if (validacion.IsFailure)
        {
            return OperationResult<TransferenciaResponse>.Fail(validacion.StatusCode, validacion.Error!);
        }

        return await RealizarTransferenciaAsync((origenResult.Value, destinoResult.Value), request.Monto, request);
    }

    public async Task<OperationResult<HistorialResponse>> ObtenerHistorialAsync(string numeroCuenta)
    {
        OperationResult<Cuenta> cuentaResult = await ObtenerCuentaActiva(numeroCuenta);
        if (cuentaResult.IsFailure)
        {
            return OperationResult<HistorialResponse>.Fail(cuentaResult.StatusCode, cuentaResult.Error!);
        }

        List<HistorialItem> auditorias = await _db.Auditoria
            .AsNoTracking()
            .Where(a => a.CuentaId == cuentaResult.Value.CuentaId)
            .OrderByDescending(a => a.CreadoEn)
            .Select(a => new HistorialItem(a.Tipo, a.Monto, a.Descripcion, a.CreadoEn))
            .ToListAsync();

        return OperationResult<HistorialResponse>.Ok(new HistorialResponse(cuentaResult.Value.Usuario?.Nombre ?? string.Empty, auditorias));
    }

    private async Task<OperationResult<Cuenta>> ObtenerCuentaActiva(string numeroCuenta)
    {
        Cuenta cuenta = await _db.Cuentas
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta && c.Estado);

        return cuenta is null
            ? OperationResult<Cuenta>.NotFound("Cuenta no encontrada o inactiva.")
            : OperationResult<Cuenta>.Ok(cuenta);
    }

    private static Task<OperationResult<decimal>> ValidarMontoAsync(decimal monto, string tipo)
    {
        if (monto <= 0)
        {
            return Task.FromResult(OperationResult<decimal>.BadRequest("El monto debe ser mayor que cero."));
        }

        return Task.FromResult(OperationResult<decimal>.Ok(monto));
    }

    private static Task<OperationResult<decimal>> ValidarRetiroAsync(Cuenta cuenta, decimal monto)
    {
        if (monto <= 0)
        {
            return Task.FromResult(OperationResult<decimal>.BadRequest("El monto debe ser mayor que cero."));
        }

        if (monto % 10 != 0)
        {
            return Task.FromResult(OperationResult<decimal>.BadRequest("El retiro debe ser múltiplo de 10."));
        }

        if (monto > 500)
        {
            return Task.FromResult(OperationResult<decimal>.BadRequest("El retiro excede el límite de 500."));
        }

        if (monto > cuenta.Saldo)
        {
            return Task.FromResult(OperationResult<decimal>.BadRequest("Fondos insuficientes."));
        }

        return Task.FromResult(OperationResult<decimal>.Ok(monto));
    }

    private static Task<OperationResult<decimal>> ValidarTransferenciaAsync(Cuenta origen, Cuenta destino, decimal monto)
    {
        if (monto <= 0)
        {
            return Task.FromResult(OperationResult<decimal>.BadRequest("El monto debe ser mayor que cero."));
        }

        if (monto > origen.Saldo)
        {
            return Task.FromResult(OperationResult<decimal>.BadRequest("Fondos insuficientes en la cuenta origen."));
        }

        return Task.FromResult(OperationResult<decimal>.Ok(monto));
    }

    private async Task<OperationResult<OperacionResponse>> RealizarDepositoAsync(Cuenta cuenta, decimal monto)
    {
        cuenta.Saldo += monto;
        await _db.SaveChangesAsync();
        await _eventBus.PublishAsync(new AuditoriaEvent
        {
            CuentaId = cuenta.CuentaId,
            NumeroCuenta = cuenta.NumeroCuenta,
            Tipo = "Depósito",
            Monto = monto,
            Descripcion = $"Depósito de {monto:N2}",
            CreadoEn = DateTime.UtcNow
        });

        return OperationResult<OperacionResponse>.Ok(new OperacionResponse($"Depósito de ${monto:N2} realizado.", cuenta.Saldo));
    }

    private async Task<OperationResult<OperacionResponse>> RealizarRetiroAsync(Cuenta cuenta, decimal monto)
    {
        cuenta.Saldo -= monto;
        await _db.SaveChangesAsync();
        await _eventBus.PublishAsync(new AuditoriaEvent
        {
            CuentaId = cuenta.CuentaId,
            NumeroCuenta = cuenta.NumeroCuenta,
            Tipo = "Retiro",
            Monto = monto,
            Descripcion = $"Retiro de {monto:N2}",
            CreadoEn = DateTime.UtcNow
        });

        return OperationResult<OperacionResponse>.Ok(new OperacionResponse($"Retiro de ${monto:N2} realizado.", cuenta.Saldo));
    }

    private async Task<OperationResult<TransferenciaResponse>> RealizarTransferenciaAsync((Cuenta origen, Cuenta destino) pair, decimal monto, TransferenciaRequest request)
    {
        await _transferInterceptor.InterceptTransferAsync(request, pair.origen, pair.destino);

        pair.origen.Saldo -= monto;
        pair.destino.Saldo += monto;
        await _db.SaveChangesAsync();

        await _eventBus.PublishAsync(new AuditoriaEvent
        {
            CuentaId = pair.origen.CuentaId,
            NumeroCuenta = pair.origen.NumeroCuenta,
            Tipo = "Transferencia salida",
            Monto = monto,
            Descripcion = $"Transferencia de {monto:N2} a {pair.destino.NumeroCuenta}",
            CreadoEn = DateTime.UtcNow
        });

        await _eventBus.PublishAsync(new AuditoriaEvent
        {
            CuentaId = pair.destino.CuentaId,
            NumeroCuenta = pair.destino.NumeroCuenta,
            Tipo = "Transferencia entrada",
            Monto = monto,
            Descripcion = $"Transferencia de {monto:N2} desde {pair.origen.NumeroCuenta}",
            CreadoEn = DateTime.UtcNow
        });

        await _eventBus.PublishAsync(new PagoCompletadoEvent
        {
            CuentaId = pair.origen.CuentaId,
            NumeroCuenta = pair.origen.NumeroCuenta,
            Monto = monto,
            Descripcion = $"Transferencia completada de {monto:N2} a {pair.destino.NumeroCuenta}.",
            CreadoEn = DateTime.UtcNow
        });

        return OperationResult<TransferenciaResponse>.Ok(new TransferenciaResponse(
            $"Transferencia de ${monto:N2} realizada de {pair.origen.NumeroCuenta} a {pair.destino.NumeroCuenta}.",
            pair.origen.Saldo,
            pair.destino.Saldo));
    }
}
