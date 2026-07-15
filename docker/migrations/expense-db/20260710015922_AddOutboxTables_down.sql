START TRANSACTION;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015922_AddOutboxTables') THEN
    DROP TABLE public.outbox_message;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015922_AddOutboxTables') THEN
    DROP TABLE public.inbox_state;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015922_AddOutboxTables') THEN
    DROP TABLE public.outbox_state;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015922_AddOutboxTables') THEN
    DELETE FROM public."__EFMigrationsHistory"
    WHERE migration_id = '20260710015922_AddOutboxTables';
    END IF;
END $EF$;
COMMIT;

