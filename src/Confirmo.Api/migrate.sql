START TRANSACTION;

CREATE TABLE public.deposito_eventos (
    "Id" bigint GENERATED ALWAYS AS IDENTITY,
    "DepositoId" uuid NOT NULL,
    "UsuarioId" uuid,
    "Evento" character varying(40) NOT NULL,
    "Origen" character varying(40) NOT NULL,
    "SesionId" uuid,
    "Metadata" jsonb,
    "OcurridoEn" timestamp with time zone NOT NULL,
    "RegistradoEn" timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT "PK_deposito_eventos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_deposito_eventos_depositos_DepositoId" FOREIGN KEY ("DepositoId") REFERENCES public.depositos ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_deposito_eventos_profiles_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES public.profiles ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_deposito_eventos_DepositoId_OcurridoEn" ON public.deposito_eventos ("DepositoId", "OcurridoEn");

CREATE INDEX "IX_deposito_eventos_Evento_OcurridoEn" ON public.deposito_eventos ("Evento", "OcurridoEn");

CREATE INDEX "IX_deposito_eventos_UsuarioId_OcurridoEn" ON public.deposito_eventos ("UsuarioId", "OcurridoEn");

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20261111221558_AddDepositoEventos', '8.0.6');

COMMIT;

