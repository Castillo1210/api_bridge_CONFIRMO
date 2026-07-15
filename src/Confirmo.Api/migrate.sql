START TRANSACTION;

ALTER TABLE public.depositos ADD "PendienteRegularizar" boolean NOT NULL DEFAULT FALSE;

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260715174503_AddRegularizacionFinanzasToDepositos', '8.0.6');

COMMIT;

