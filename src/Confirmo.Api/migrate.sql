START TRANSACTION;

CREATE TABLE public.motivos_rechazo (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Texto" character varying(150) NOT NULL,
    "Orden" integer NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT "PK_motivos_rechazo" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_motivos_rechazo_Orden" ON public.motivos_rechazo ("Orden");

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20261112172004_AddMotivosRechazo', '8.0.6');

COMMIT;

