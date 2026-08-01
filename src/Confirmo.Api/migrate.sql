START TRANSACTION;

CREATE TABLE public.avisos_imagenes_galeria (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "ObjectName" character varying(500) NOT NULL,
    "ContentType" character varying(50) NOT NULL,
    "Nombre" character varying(200),
    "CreadoPor" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
    "Activo" boolean NOT NULL DEFAULT TRUE,
    CONSTRAINT "PK_avisos_imagenes_galeria" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_avisos_imagenes_galeria_profiles_CreadoPor" FOREIGN KEY ("CreadoPor") REFERENCES public.profiles ("Id") ON DELETE RESTRICT
);

CREATE INDEX idx_avisos_imagenes_galeria_activo_created ON public.avisos_imagenes_galeria ("Activo", "CreatedAt");

CREATE INDEX "IX_avisos_imagenes_galeria_CreadoPor" ON public.avisos_imagenes_galeria ("CreadoPor");

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260801191012_AddImagenGaleria', '8.0.6');

COMMIT;

