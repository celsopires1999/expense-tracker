CREATE TABLE IF NOT EXISTS public."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    CREATE TABLE public.categories (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_categories PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    CREATE TABLE public.payment_methods (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_payment_methods PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    CREATE TABLE public.tags (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_tags PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    CREATE TABLE public.expenses (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        description character varying(255) NOT NULL,
        amount numeric(18,2) NOT NULL,
        date date NOT NULL,
        category_id uuid NOT NULL,
        payment_method_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone,
        CONSTRAINT pk_expenses PRIMARY KEY (id),
        CONSTRAINT fk_expenses_categories_category_id FOREIGN KEY (category_id) REFERENCES public.categories (id) ON DELETE CASCADE,
        CONSTRAINT fk_expenses_payment_methods_payment_method_id FOREIGN KEY (payment_method_id) REFERENCES public.payment_methods (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    CREATE TABLE public.expense_tags (
        expense_id uuid NOT NULL,
        tag_id uuid NOT NULL,
        CONSTRAINT pk_expense_tags PRIMARY KEY (expense_id, tag_id),
        CONSTRAINT fk_expense_tags_expenses_expense_id FOREIGN KEY (expense_id) REFERENCES public.expenses (id) ON DELETE CASCADE,
        CONSTRAINT fk_expense_tags_tags_tag_id FOREIGN KEY (tag_id) REFERENCES public.tags (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    INSERT INTO public.categories (id, name)
    VALUES ('33333333-3333-3333-3333-333333333333', 'Alimentação');
    INSERT INTO public.categories (id, name)
    VALUES ('44444444-4444-4444-4444-444444444444', 'Transporte');
    INSERT INTO public.categories (id, name)
    VALUES ('55555555-5555-5555-5555-555555555555', 'Moradia');
    INSERT INTO public.categories (id, name)
    VALUES ('66666666-6666-6666-6666-666666666666', 'Saúde');
    INSERT INTO public.categories (id, name)
    VALUES ('77777777-7777-7777-7777-777777777777', 'Lazer');
    INSERT INTO public.categories (id, name)
    VALUES ('88888888-8888-8888-8888-888888888888', 'Educação');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    INSERT INTO public.payment_methods (id, name)
    VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Crédito');
    INSERT INTO public.payment_methods (id, name)
    VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'Débito');
    INSERT INTO public.payment_methods (id, name)
    VALUES ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Dinheiro');
    INSERT INTO public.payment_methods (id, name)
    VALUES ('dddddddd-dddd-dddd-dddd-dddddddddddd', 'Pix');
    INSERT INTO public.payment_methods (id, name)
    VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'Boleto');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_categories_name ON public.categories (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    CREATE INDEX ix_expense_tags_tag_id ON public.expense_tags (tag_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    CREATE INDEX ix_expenses_category_id ON public.expenses (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    CREATE INDEX ix_expenses_payment_method_id ON public.expenses (payment_method_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_payment_methods_name ON public.payment_methods (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_tags_name ON public.tags (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710014742_InitialCreate') THEN
    INSERT INTO public."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260710014742_InitialCreate', '10.0.7');
    END IF;
END $EF$;
COMMIT;

