START TRANSACTION;

ALTER TABLE public.avisos ADD "ZavuPlantillaCodigo" text;

CREATE TABLE public.zavu_plantillas (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Nombre" character varying(200) NOT NULL,
    "Codigo" character varying(100) NOT NULL,
    "TemplateId" character varying(200) NOT NULL,
    "Activo" boolean NOT NULL DEFAULT TRUE,
    "CreadoPor" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    CONSTRAINT "PK_zavu_plantillas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_zavu_plantillas_profiles_CreadoPor" FOREIGN KEY ("CreadoPor") REFERENCES public.profiles ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_zavu_plantillas_Codigo" ON public.zavu_plantillas ("Codigo");

CREATE INDEX "IX_zavu_plantillas_CreadoPor" ON public.zavu_plantillas ("CreadoPor");

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260821231047_modavisos', '8.0.6');

COMMIT;