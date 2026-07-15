START TRANSACTION;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013745_InitialCreate') THEN
    DROP TABLE public.user_roles;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013745_InitialCreate') THEN
    DROP TABLE public.roles;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013745_InitialCreate') THEN
    DROP TABLE public.users;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013745_InitialCreate') THEN
    DELETE FROM public."__EFMigrationsHistory"
    WHERE migration_id = '20260710013745_InitialCreate';
    END IF;
END $EF$;
COMMIT;

