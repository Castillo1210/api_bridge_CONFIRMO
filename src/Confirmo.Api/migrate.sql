START TRANSACTION;

CREATE TABLE public.deposito_correcciones (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "DepositoId" uuid NOT NULL,
    "Accion" character varying(30) NOT NULL,
    "ProfileId" uuid,
    "Motivo" character varying(500) NOT NULL,
    "EstadoAnterior" character varying(20) NOT NULL,
    "EstadoNuevo" character varying(20) NOT NULL,
    "Cambios" jsonb NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT "PK_deposito_correcciones" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_deposito_correcciones_depositos_DepositoId" FOREIGN KEY ("DepositoId") REFERENCES public.depositos ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_deposito_correcciones_profiles_ProfileId" FOREIGN KEY ("ProfileId") REFERENCES public.profiles ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_deposito_correcciones_DepositoId_CreatedAt" ON public.deposito_correcciones ("DepositoId", "CreatedAt");

CREATE INDEX "IX_deposito_correcciones_ProfileId" ON public.deposito_correcciones ("ProfileId");

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20261111172634_AddDepositoCorrecciones', '8.0.6');

COMMIT;

