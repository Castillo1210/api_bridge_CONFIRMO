START TRANSACTION;

CREATE TABLE public.avisos (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Titulo" character varying(500) NOT NULL,
    "MensajeTexto" text NOT NULL,
    "MediaUrl" character varying(500),
    "TipoMedia" character varying(50),
    "RolesDestino" text[] NOT NULL DEFAULT ('{}'::text[]),
    "EnviarApp" boolean NOT NULL,
    "EnviarWhatsapp" boolean NOT NULL,
    "EnviarEmail" boolean NOT NULL,
    "AsuntoEmail" character varying(299),
    "EsRecurrente" boolean NOT NULL,
    "Frecuencia" character varying(55),
    "HoraEjecucion" interval,
    "DiaSemana" integer,
    "DiaMes" integer,
    "ProximaEjecucion" timestamp with time zone,
    "UltimaEjecucion" timestamp with time zone,
    "CreadoPor" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    "Estado" character varying(55) NOT NULL DEFAULT 'programado',
    "Activo" boolean NOT NULL DEFAULT TRUE,
    CONSTRAINT "PK_avisos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_avisos_profiles_CreadoPor" FOREIGN KEY ("CreadoPor") REFERENCES public.profiles ("Id") ON DELETE RESTRICT
);

CREATE TABLE public.envio_aviso_logs (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "AvisoId" uuid NOT NULL,
    "ProfileId" uuid NOT NULL,
    "Canal" character varying(55) NOT NULL,
    "Estado" character varying(55) NOT NULL,
    "ZavuMessageId" character varying(200),
    "Error" text,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT "PK_envio_aviso_logs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_envio_aviso_logs_avisos_AvisoId" FOREIGN KEY ("AvisoId") REFERENCES public.avisos ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_envio_aviso_logs_profiles_ProfileId" FOREIGN KEY ("ProfileId") REFERENCES public.profiles ("Id") ON DELETE CASCADE
);

CREATE INDEX idx_avisos_estado_proxima_ejecucion ON public.avisos ("Estado", "ProximaEjecucion");

CREATE INDEX idx_avisos_roles_destino ON public.avisos USING gin ("RolesDestino");

CREATE INDEX "IX_avisos_CreadoPor" ON public.avisos ("CreadoPor");

CREATE INDEX "IX_envio_aviso_logs_AvisoId_ProfileId_Canal" ON public.envio_aviso_logs ("AvisoId", "ProfileId", "Canal");

CREATE INDEX "IX_envio_aviso_logs_ProfileId" ON public.envio_aviso_logs ("ProfileId");

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260731175108_AddAvisos', '8.0.6');

COMMIT;

