START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260714212316_AddExpenseStatus') THEN
    ALTER TABLE public.expenses ADD status character varying(20) NOT NULL DEFAULT 'Pending';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260714212316_AddExpenseStatus') THEN
    UPDATE expenses SET status = 'Paid';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260714212316_AddExpenseStatus') THEN
    INSERT INTO public."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260714212316_AddExpenseStatus', '10.0.7');
    END IF;
END $EF$;
COMMIT;

