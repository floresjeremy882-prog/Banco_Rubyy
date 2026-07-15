namespace BancoCenit.Common;

public sealed class Usuario
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = default!;
    public string Pin { get; set; } = default!;
    public DateTime CreadoEn { get; set; }
    public List<Cuenta> Cuentas { get; set; } = new();
}

public sealed class Cuenta
{
    public int CuentaId { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string NumeroCuenta { get; set; } = default!;
    public decimal Saldo { get; set; }
    public bool Estado { get; set; }
    public DateTime CreadoEn { get; set; }
    public List<Auditoria> Auditorias { get; set; } = new();
}

public sealed class Auditoria
{
    public int AuditoriaId { get; set; }
    public int CuentaId { get; set; }
    public Cuenta? Cuenta { get; set; }
    public string NumeroCuenta { get; set; } = default!;
    public string Tipo { get; set; } = default!;
    public decimal Monto { get; set; }
    public string Descripcion { get; set; } = default!;
    public DateTime CreadoEn { get; set; }
}
