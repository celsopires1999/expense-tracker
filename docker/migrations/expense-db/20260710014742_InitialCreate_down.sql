START TRANSACTION;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    DROP TABLE public.expense_tags;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    DROP TABLE public.expenses;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    DROP TABLE public.tags;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    DROP TABLE public.categories;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    DROP TABLE public.payment_methods;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    DELETE FROM public."__EFMigrationsHistory"
    WHERE migration_id = '20260710014742_InitialCreate';
    END IF;
END $EF$;
COMMIT;

