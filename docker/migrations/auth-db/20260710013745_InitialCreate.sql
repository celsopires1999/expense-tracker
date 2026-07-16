CREATE TABLE public.roles (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    CONSTRAINT pk_roles PRIMARY KEY (id)
);

CREATE TABLE public.users (
    id uuid NOT NULL,
    email text NOT NULL,
    first_name text NOT NULL,
    last_name text NOT NULL,
    password_hash text NOT NULL,
    CONSTRAINT pk_users PRIMARY KEY (id)
);

CREATE TABLE public.user_roles (
    user_id uuid NOT NULL,
    role_id uuid NOT NULL,
    CONSTRAINT pk_user_roles PRIMARY KEY (user_id, role_id),
    CONSTRAINT fk_user_roles_roles_role_id FOREIGN KEY (role_id) REFERENCES public.roles (id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_users_user_id FOREIGN KEY (user_id) REFERENCES public.users (id) ON DELETE CASCADE
);

INSERT INTO public.roles (id, name)
VALUES ('11111111-1111-1111-1111-111111111111', 'Admin');
INSERT INTO public.roles (id, name)
VALUES ('22222222-2222-2222-2222-222222222222', 'Viewer');
INSERT INTO public.roles (id, name)
VALUES ('33333333-3333-3333-3333-333333333333', 'Standard');

CREATE UNIQUE INDEX ix_roles_name ON public.roles (name);
CREATE INDEX ix_user_roles_role_id ON public.user_roles (role_id);
CREATE UNIQUE INDEX ix_users_email ON public.users (email);
