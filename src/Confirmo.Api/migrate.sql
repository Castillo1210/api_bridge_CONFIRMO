START TRANSACTION;

CREATE TABLE public.deposito_regularizaciones (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "DepositoId" uuid NOT NULL,
    "Accion" character varying(20) NOT NULL,
    "UsuarioId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    "Motivo" text,
    "ImagenAnterior" text,
    "ImagenNueva" text,
    CONSTRAINT "PK_deposito_regularizaciones" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_deposito_regularizaciones_depositos_DepositoId" FOREIGN KEY ("DepositoId") REFERENCES public.depositos ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_deposito_regularizaciones_profiles_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES public.profiles ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_deposito_regularizaciones_CreatedAt" ON public.deposito_regularizaciones ("CreatedAt");

CREATE INDEX "IX_deposito_regularizaciones_DepositoId_CreatedAt" ON public.deposito_regularizaciones ("DepositoId", "CreatedAt");

CREATE INDEX "IX_deposito_regularizaciones_UsuarioId" ON public.deposito_regularizaciones ("UsuarioId");

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260730212444_AddDepositoRegularizacion', '8.0.6');

COMMIT;