START TRANSACTION;

CREATE TABLE public.deposito_rechazos_historial (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "DepositoId" uuid NOT NULL,
    "ImagenVoucherRechazada" text,
    "MotivoRechazo" character varying(500),
    "Observaciones" text,
    "FechaRechazo" timestamp with time zone,
    "RechazadoPor" uuid,
    "RegularizadoPor" uuid,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT "PK_deposito_rechazos_historial" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_deposito_rechazos_historial_depositos_DepositoId" FOREIGN KEY ("DepositoId") REFERENCES public.depositos ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_deposito_rechazos_historial_profiles_RechazadoPor" FOREIGN KEY ("RechazadoPor") REFERENCES public.profiles ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_deposito_rechazos_historial_profiles_RegularizadoPor" FOREIGN KEY ("RegularizadoPor") REFERENCES public.profiles ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_deposito_rechazos_historial_DepositoId_CreatedAt" ON public.deposito_rechazos_historial ("DepositoId", "CreatedAt");

CREATE INDEX "IX_deposito_rechazos_historial_RechazadoPor" ON public.deposito_rechazos_historial ("RechazadoPor");

CREATE INDEX "IX_deposito_rechazos_historial_RegularizadoPor" ON public.deposito_rechazos_historial ("RegularizadoPor");

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260916201925_AddDepositoRechazoHistorial', '8.0.6');

COMMIT;