START TRANSACTION;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260714212316_AddExpenseStatus') THEN
    ALTER TABLE public.expenses DROP COLUMN status;
    END IF;
END $EF$;
DO $EF$
BEGIN
    IF EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260714212316_AddExpenseStatus') THEN
    DELETE FROM public."__EFMigrationsHistory"
    WHERE migration_id = '20260714212316_AddExpenseStatus';
    END IF;
END $EF$;
COMMIT;

