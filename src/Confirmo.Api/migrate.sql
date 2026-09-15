START TRANSACTION;

ALTER TABLE public.depositos ADD "NumeroTarjeta" character varying(30);

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260915194424_AddNumeroTarjetaToDepositos', '8.0.6');

COMMIT;
