CREATE TABLE public.roles (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    CONSTRAINT pk_roles PRIMARY KEY (id)
);

CREATE TABLE public.role_permissions (
    role_id uuid NOT NULL,
    permission character varying(255) NOT NULL,
    CONSTRAINT pk_role_permissions PRIMARY KEY (role_id, permission),
    CONSTRAINT fk_role_permissions_roles_role_id FOREIGN KEY (role_id) REFERENCES public.roles (id) ON DELETE CASCADE
);

INSERT INTO public.roles (id, name)
VALUES ('11111111-1111-1111-1111-111111111111', 'Admin');
INSERT INTO public.roles (id, name)
VALUES ('22222222-2222-2222-2222-222222222222', 'Viewer');
INSERT INTO public.roles (id, name)
VALUES ('33333333-3333-3333-3333-333333333333', 'Standard');

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

CREATE UNIQUE INDEX ix_roles_name ON public.roles (name);
