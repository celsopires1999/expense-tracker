START TRANSACTION;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013800_InitialCreate') THEN
    DROP TABLE public.role_permissions;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013800_InitialCreate') THEN
    DROP TABLE public.roles;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013800_InitialCreate') THEN
    DELETE FROM public."__EFMigrationsHistory"
    WHERE migration_id = '20260710013800_InitialCreate';
    END IF;
END $EF$;
COMMIT;

