START TRANSACTION;

ALTER TABLE public.depositos DROP CONSTRAINT "FK_depositos_bancos_BancoId";

ALTER TABLE public.depositos DROP CONSTRAINT "FK_depositos_empresas_EmpresaId";

ALTER TABLE public.depositos DROP CONSTRAINT "FK_depositos_profiles_VendedorId";

ALTER TABLE public.depositos DROP CONSTRAINT "FK_depositos_sucursales_SucursalId";

ALTER TABLE public.depositos DROP COLUMN "TrabajadorSucursalId";

ALTER TABLE public.sucursales ALTER COLUMN "Id" SET DEFAULT (gen_random_uuid());

ALTER TABLE public.profiles ALTER COLUMN "PhoneNumber" DROP NOT NULL;

ALTER TABLE public.profiles ADD "Email" character varying(200);

ALTER TABLE public.empresas ALTER COLUMN "Id" SET DEFAULT (gen_random_uuid());

ALTER TABLE public.depositos ALTER COLUMN "WarningIds" DROP NOT NULL;

ALTER TABLE public.depositos ALTER COLUMN "ImagenVoucher" TYPE character varying(500);

ALTER TABLE public.depositos ALTER COLUMN "ErrorIds" DROP NOT NULL;

ALTER TABLE public.depositos ADD trabajador_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE public.bancos ALTER COLUMN "Id" SET DEFAULT (gen_random_uuid());

CREATE TABLE public.cuentasbancarias (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "NumeroCuenta" character varying(50) NOT NULL,
    "Anexo" character varying(25) NOT NULL,
    "EmpresaId" uuid NOT NULL,
    "BancoId" uuid NOT NULL,
    "Activo" boolean NOT NULL,
    CONSTRAINT "PK_cuentasbancarias" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_cuentasbancarias_bancos_BancoId" FOREIGN KEY ("BancoId") REFERENCES public.bancos ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_cuentasbancarias_empresas_EmpresaId" FOREIGN KEY ("EmpresaId") REFERENCES public.empresas ("Id") ON DELETE RESTRICT
);

CREATE TABLE public.trabajadores (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "ProfileId" uuid NOT NULL,
    "Nombre" character varying(255) NOT NULL,
    "TelefonoPersonal" character varying(55),
    "EmpresaId" uuid NOT NULL,
    "SucursalId" uuid,
    "Activo" boolean NOT NULL,
    "FechaInicio" date NOT NULL DEFAULT (now()),
    "FechaFin" date,
    "CreadoPor" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_trabajadores" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_trabajadores_empresas_EmpresaId" FOREIGN KEY ("EmpresaId") REFERENCES public.empresas ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_trabajadores_profiles_CreadoPor" FOREIGN KEY ("CreadoPor") REFERENCES public.profiles ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_trabajadores_profiles_ProfileId" FOREIGN KEY ("ProfileId") REFERENCES public.profiles ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_trabajadores_sucursales_SucursalId" FOREIGN KEY ("SucursalId") REFERENCES public.sucursales ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_sucursales_EmpresaId" ON public.sucursales ("EmpresaId");

CREATE UNIQUE INDEX "IX_profiles_Email" ON public.profiles ("Email") WHERE email IS NOT NULL;

CREATE INDEX "IX_profiles_EmpresaId" ON public.profiles ("EmpresaId");

CREATE INDEX "IX_profiles_SucursalId" ON public.profiles ("SucursalId");

CREATE INDEX "IX_depositos_trabajador_id" ON public.depositos (trabajador_id);

CREATE INDEX "IX_depositos_ValidadoPor" ON public.depositos ("ValidadoPor");

CREATE INDEX "IX_cuentasbancarias_BancoId" ON public.cuentasbancarias ("BancoId");

CREATE INDEX "IX_cuentasbancarias_EmpresaId" ON public.cuentasbancarias ("EmpresaId");

CREATE INDEX idx_trabajadores_empresa_sucursal ON public.trabajadores ("EmpresaId", "SucursalId");

CREATE INDEX idx_trabajadores_profile_activo ON public.trabajadores ("ProfileId", "Activo");

CREATE INDEX "IX_trabajadores_CreadoPor" ON public.trabajadores ("CreadoPor");

CREATE INDEX "IX_trabajadores_SucursalId" ON public.trabajadores ("SucursalId");

ALTER TABLE public.depositos ADD CONSTRAINT "FK_depositos_bancos_BancoId" FOREIGN KEY ("BancoId") REFERENCES public.bancos ("Id") ON DELETE RESTRICT;

ALTER TABLE public.depositos ADD CONSTRAINT "FK_depositos_empresas_EmpresaId" FOREIGN KEY ("EmpresaId") REFERENCES public.empresas ("Id") ON DELETE RESTRICT;

ALTER TABLE public.depositos ADD CONSTRAINT "FK_depositos_profiles_ValidadoPor" FOREIGN KEY ("ValidadoPor") REFERENCES public.profiles ("Id") ON DELETE RESTRICT;

ALTER TABLE public.depositos ADD CONSTRAINT "FK_depositos_profiles_VendedorId" FOREIGN KEY ("VendedorId") REFERENCES public.profiles ("Id") ON DELETE RESTRICT;

ALTER TABLE public.depositos ADD CONSTRAINT "FK_depositos_sucursales_SucursalId" FOREIGN KEY ("SucursalId") REFERENCES public.sucursales ("Id") ON DELETE RESTRICT;

ALTER TABLE public.depositos ADD CONSTRAINT "FK_depositos_trabajadores_trabajador_id" FOREIGN KEY (trabajador_id) REFERENCES public.trabajadores ("Id") ON DELETE RESTRICT;

ALTER TABLE public.profiles ADD CONSTRAINT "FK_profiles_empresas_EmpresaId" FOREIGN KEY ("EmpresaId") REFERENCES public.empresas ("Id") ON DELETE RESTRICT;

ALTER TABLE public.profiles ADD CONSTRAINT "FK_profiles_sucursales_SucursalId" FOREIGN KEY ("SucursalId") REFERENCES public.sucursales ("Id") ON DELETE RESTRICT;

ALTER TABLE public.sucursales ADD CONSTRAINT "FK_sucursales_empresas_EmpresaId" FOREIGN KEY ("EmpresaId") REFERENCES public.empresas ("Id") ON DELETE RESTRICT;

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260704184158_AddTrabajdoresAndFixFks', '8.0.6');

COMMIT;

