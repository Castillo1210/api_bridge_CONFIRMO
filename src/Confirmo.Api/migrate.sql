START TRANSACTION;

ALTER TABLE public.profiles ADD "DeviceId" character varying(200);

INSERT INTO public.__ef_migrations ("MigrationId", "ProductVersion")
VALUES ('20260714213637_AddDeviceIdToProfiles', '8.0.6');

COMMIT;