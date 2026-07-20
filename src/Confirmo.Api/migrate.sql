START TRANSACTION;

ALTER TABLE public.depositos ADD "Cuo" character varying(500);

CREATE INDEX "IX_depositos_Cuo" ON public.depositos ("Cuo");

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260720213025_AddCuoToDepositos', '8.0.6');

COMMIT;

