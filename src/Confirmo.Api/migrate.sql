START TRANSACTION;

ALTER TABLE public.depositos ADD "FechaRegistroOriginal" timestamp with time zone DEFAULT (now());

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260923191005_AddFechaRegistroOriginal', '8.0.6');

COMMIT;