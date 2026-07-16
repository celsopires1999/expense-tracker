ALTER TABLE public.expenses ADD status character varying(20) NOT NULL DEFAULT 'Pending';

UPDATE expenses SET status = 'Paid';
