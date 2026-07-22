START TRANSACTION;

ALTER TABLE public.depositos ADD "ImagenUrl" character varying(500);

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260722145846_AddImagenUrlToDepositos', '8.0.6');

COMMIT;