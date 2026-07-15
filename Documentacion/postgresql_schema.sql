-- Script PostgreSQL para Banco Ruby
-- Adáptalo si necesitas usar otro usuario o contraseña.

CREATE DATABASE "Banco Ruby";

\c "Banco Ruby";

CREATE TABLE IF NOT EXISTS usuario (
    usuario_id SERIAL PRIMARY KEY,
    nombre TEXT NOT NULL,
    pin TEXT NOT NULL,
    creado_en TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS cuenta (
    cuenta_id SERIAL PRIMARY KEY,
    usuario_id INT NOT NULL REFERENCES usuario(usuario_id),
    numero_cuenta TEXT NOT NULL UNIQUE,
    saldo NUMERIC(18,2) NOT NULL DEFAULT 0,
    estado BOOLEAN NOT NULL DEFAULT TRUE,
    creado_en TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS auditoria (
    auditoria_id SERIAL PRIMARY KEY,
    cuenta_id INT NOT NULL REFERENCES cuenta(cuenta_id),
    numero_cuenta TEXT NOT NULL,
    tipo TEXT NOT NULL,
    monto NUMERIC(18,2) NOT NULL,
    descripcion TEXT NOT NULL,
    creado_en TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

INSERT INTO usuario (nombre, pin)
VALUES ('La Bestia', '2004')
ON CONFLICT DO NOTHING;

INSERT INTO cuenta (usuario_id, numero_cuenta, saldo, estado)
VALUES (
    (SELECT usuario_id FROM usuario WHERE nombre = 'La Bestia' AND pin = '2004'),
    '1234567812345678',
    1000.00,
    TRUE
)
ON CONFLICT (numero_cuenta) DO UPDATE
SET saldo = EXCLUDED.saldo,
    estado = EXCLUDED.estado;

INSERT INTO auditoria (cuenta_id, numero_cuenta, tipo, monto, descripcion, creado_en)
VALUES (
    (SELECT cuenta_id FROM cuenta WHERE numero_cuenta = '1234567812345678'),
    '1234567812345678',
    'Deposit',
    1000.00,
    'Saldo inicial',
    NOW()
);
