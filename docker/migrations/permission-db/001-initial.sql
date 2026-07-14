CREATE TABLE IF NOT EXISTS public."__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013800_InitialCreate') THEN
    CREATE TABLE public.roles (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        CONSTRAINT pk_roles PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013800_InitialCreate') THEN
    CREATE TABLE public.role_permissions (
        role_id uuid NOT NULL,
        permission character varying(255) NOT NULL,
        CONSTRAINT pk_role_permissions PRIMARY KEY (role_id, permission),
        CONSTRAINT fk_role_permissions_roles_role_id FOREIGN KEY (role_id) REFERENCES public.roles (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013800_InitialCreate') THEN
    INSERT INTO public.roles (id, name)
    VALUES ('11111111-1111-1111-1111-111111111111', 'Admin');
    INSERT INTO public.roles (id, name)
    VALUES ('22222222-2222-2222-2222-222222222222', 'Viewer');
    INSERT INTO public.roles (id, name)
    VALUES ('33333333-3333-3333-3333-333333333333', 'Standard');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013800_InitialCreate') THEN
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('categories:create', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('categories:delete', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('categories:read', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('categories:update', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:create', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:delete', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:delete:all', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:read', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:read:all', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:update', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:update:all', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('payment-methods:create', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('payment-methods:delete', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('payment-methods:read', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('payment-methods:update', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('tags:create', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('tags:delete', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('tags:read', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('tags:update', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('users:access', '11111111-1111-1111-1111-111111111111');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('categories:read', '22222222-2222-2222-2222-222222222222');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:read', '22222222-2222-2222-2222-222222222222');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:read:all', '22222222-2222-2222-2222-222222222222');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('payment-methods:read', '22222222-2222-2222-2222-222222222222');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('tags:read', '22222222-2222-2222-2222-222222222222');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('users:access', '22222222-2222-2222-2222-222222222222');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('categories:read', '33333333-3333-3333-3333-333333333333');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:create', '33333333-3333-3333-3333-333333333333');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:delete', '33333333-3333-3333-3333-333333333333');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:read', '33333333-3333-3333-3333-333333333333');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('expenses:update', '33333333-3333-3333-3333-333333333333');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('payment-methods:read', '33333333-3333-3333-3333-333333333333');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('tags:read', '33333333-3333-3333-3333-333333333333');
    INSERT INTO public.role_permissions (permission, role_id)
    VALUES ('users:access', '33333333-3333-3333-3333-333333333333');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013800_InitialCreate') THEN
    CREATE UNIQUE INDEX ix_roles_name ON public.roles (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710013800_InitialCreate') THEN
    INSERT INTO public."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260710013800_InitialCreate', '10.0.7');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015920_AddOutboxTables') THEN
    CREATE TABLE public.inbox_state (
        id bigint GENERATED BY DEFAULT AS IDENTITY,
        message_id uuid NOT NULL,
        consumer_id uuid NOT NULL,
        lock_id uuid NOT NULL,
        row_version bytea,
        received timestamp with time zone NOT NULL,
        receive_count integer NOT NULL,
        expiration_time timestamp with time zone,
        consumed timestamp with time zone,
        delivered timestamp with time zone,
        last_sequence_number bigint,
        CONSTRAINT pk_inbox_state PRIMARY KEY (id),
        CONSTRAINT ak_inbox_state_message_id_consumer_id UNIQUE (message_id, consumer_id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015920_AddOutboxTables') THEN
    CREATE TABLE public.outbox_state (
        outbox_id uuid NOT NULL,
        lock_id uuid NOT NULL,
        row_version bytea,
        created timestamp with time zone NOT NULL,
        delivered timestamp with time zone,
        last_sequence_number bigint,
        CONSTRAINT pk_outbox_state PRIMARY KEY (outbox_id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015920_AddOutboxTables') THEN
    CREATE TABLE public.outbox_message (
        sequence_number bigint GENERATED BY DEFAULT AS IDENTITY,
        enqueue_time timestamp with time zone,
        sent_time timestamp with time zone NOT NULL,
        headers text,
        properties text,
        inbox_message_id uuid,
        inbox_consumer_id uuid,
        outbox_id uuid,
        message_id uuid NOT NULL,
        content_type character varying(256) NOT NULL,
        message_type text NOT NULL,
        body text NOT NULL,
        conversation_id uuid,
        correlation_id uuid,
        initiator_id uuid,
        request_id uuid,
        source_address character varying(256),
        destination_address character varying(256),
        response_address character varying(256),
        fault_address character varying(256),
        expiration_time timestamp with time zone,
        CONSTRAINT pk_outbox_message PRIMARY KEY (sequence_number),
        CONSTRAINT fk_outbox_message_inbox_state_inbox_message_id_inbox_consumer_ FOREIGN KEY (inbox_message_id, inbox_consumer_id) REFERENCES public.inbox_state (message_id, consumer_id),
        CONSTRAINT fk_outbox_message_outbox_state_outbox_id FOREIGN KEY (outbox_id) REFERENCES public.outbox_state (outbox_id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015920_AddOutboxTables') THEN
    CREATE INDEX ix_inbox_state_delivered ON public.inbox_state (delivered);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015920_AddOutboxTables') THEN
    CREATE INDEX ix_outbox_message_enqueue_time ON public.outbox_message (enqueue_time);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015920_AddOutboxTables') THEN
    CREATE INDEX ix_outbox_message_expiration_time ON public.outbox_message (expiration_time);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015920_AddOutboxTables') THEN
    CREATE UNIQUE INDEX ix_outbox_message_inbox_message_id_inbox_consumer_id_sequence_ ON public.outbox_message (inbox_message_id, inbox_consumer_id, sequence_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015920_AddOutboxTables') THEN
    CREATE UNIQUE INDEX ix_outbox_message_outbox_id_sequence_number ON public.outbox_message (outbox_id, sequence_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015920_AddOutboxTables') THEN
    CREATE INDEX ix_outbox_state_created ON public.outbox_state (created);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory" WHERE "migration_id" = '20260710015920_AddOutboxTables') THEN
    INSERT INTO public."__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260710015920_AddOutboxTables', '10.0.7');
    END IF;
END $EF$;
COMMIT;

