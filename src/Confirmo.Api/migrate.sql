START TRANSACTION;

CREATE UNIQUE INDEX "IX_deposito_regularizaciones_DepositoId" ON public.deposito_regularizaciones ("DepositoId");

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260825225054_UpRegularizaciones', '8.0.6');

COMMIT;

