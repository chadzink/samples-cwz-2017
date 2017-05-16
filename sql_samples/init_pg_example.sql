CREATE EXTENSION pgcrypto;

------------------------
CREATE TABLE users (
    id integer NOT NULL,
    username character varying NOT NULL,
    email character varying NOT NULL,
    password_digest character varying NOT NULL,
    reset_password_token character varying,
    reset_password_token_exp timestamp without time zone,
    created_at timestamp without time zone,
    updated_at timestamp without time zone,
    first_name character varying,
    last_name character varying,
    role_id integer,
    session_id uuid NOT NULL
);

CREATE SEQUENCE users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE users_id_seq OWNED BY users.id;
ALTER TABLE ONLY users ALTER COLUMN id SET DEFAULT nextval('users_id_seq'::regclass);
ALTER TABLE ONLY users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);
CREATE UNIQUE INDEX index_users_on_username ON users USING btree (username);

--DROP FUNCTION add_user(username character varying, email character varying, password character varying,first_name character varying, last_name character varying);
CREATE OR REPLACE FUNCTION add_user(_username character varying, _email character varying, _password character varying, _first_name character varying, _last_name character varying)
RETURNS bool AS $$
BEGIN
	INSERT INTO users
		(username,email,password_digest,first_name,last_name,session_id,created_at,updated_at)
	VALUES
		(_username,_email,crypt(_password,gen_salt('md5')),_first_name,_last_name,gen_random_uuid(),current_timestamp,current_timestamp);
	RETURN EXISTS(SELECT * FROM users WHERE username = _username);
EXCEPTION WHEN others THEN
	RETURN false;
END;
$$ LANGUAGE plpgsql;

-- LOAD DEFAULT ADMIN USER
SELECT add_user('admin','chadz.rm@gmail.com','Password','Chad','Zink');

--DROP FUNCTION check_password(character varying,character varying);
CREATE OR REPLACE FUNCTION check_password(_username character varying, _password character varying) RETURNS uuid AS $$
BEGIN
	IF EXISTS(SELECT id FROM users WHERE username = _username AND (password_digest = crypt(_password, password_digest)))
	THEN
		UPDATE users SET session_id = gen_random_uuid();
	END IF;
	RETURN (SELECT session_id FROM users WHERE username = _username AND (password_digest = crypt(_password, password_digest)));
END;
$$ LANGUAGE plpgsql;
------------------------

------------------------
CREATE SEQUENCE user_roles_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE TABLE user_roles (
  id integer NOT NULL PRIMARY KEY DEFAULT nextval('user_roles_id_seq'::regclass),
  label character varying(150) NULL,
  created_at timestamp without time zone,
  updated_at timestamp without time zone
);
ALTER SEQUENCE module_roles_id_seq OWNED BY module_roles.id;

INSERT INTO user_roles (label,created_at,updated_at)
  VALUES ('Admin', current_timestamp, current_timestamp);
INSERT INTO user_roles (label,created_at,updated_at)
  VALUES ('Wildling', current_timestamp, current_timestamp);
INSERT INTO user_roles (label,created_at,updated_at)
  VALUES ('Client', current_timestamp, current_timestamp);
INSERT INTO user_roles (label,created_at,updated_at)
  VALUES ('Guest', current_timestamp, current_timestamp);

UPDATE users
  SET role_id = (SELECT id FROM user_roles WHERE label = 'Admin' LIMIT 1)
WHERE username = 'admin'
------------------------

------------------------
CREATE TABLE app_modules (
  id integer NOT NULL,
  title character varying(100) NULL,
  icon_cls character varying(250) NULL,
  items character varying(1000) NULL,
  created_at timestamp without time zone,
  updated_at timestamp without time zone
);

CREATE SEQUENCE app_modules_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE app_modules_id_seq OWNED BY app_modules.id;
ALTER TABLE ONLY app_modules ALTER COLUMN id SET DEFAULT nextval('app_modules_id_seq'::regclass);
ALTER TABLE ONLY app_modules
    ADD CONSTRAINT app_modules_pkey PRIMARY KEY (id);

INSERT INTO app_modules (title,icon_cls,items,created_at,updated_at)
  VALUES ('Dashboard', 'fa-tachometer', '[{ xtype: "panel" }]', current_timestamp, current_timestamp);
INSERT INTO app_modules (title,icon_cls,items,created_at,updated_at)
  VALUES ('Planning', 'fa-cubes', '[{ xtype: "panel" }]', current_timestamp, current_timestamp);
INSERT INTO app_modules (title,icon_cls,items,created_at,updated_at)
  VALUES ('Projects', 'fa-building', '[{ xtype: "panel" }]', current_timestamp, current_timestamp);
INSERT INTO app_modules (title,icon_cls,items,created_at,updated_at)
  VALUES ('Settings', 'fa-cogs', '[{ xtype: "panel" }]', current_timestamp, current_timestamp);
INSERT INTO app_modules (title,icon_cls,items,created_at,updated_at)
  VALUES ('Teams', 'fa-users', '[{ xtype: "panel" }]', current_timestamp, current_timestamp);
------------------------

------------------------
CREATE SEQUENCE module_roles_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE TABLE module_roles (
  id integer NOT NULL PRIMARY KEY DEFAULT nextval('app_modules_id_seq'::regclass),
  app_module_id integer NOT NULL,
  user_role_id integer NOT NULL,
  display_order integer NOT NULL,
  created_at timestamp without time zone,
  updated_at timestamp without time zone
);

ALTER SEQUENCE module_roles_id_seq OWNED BY module_roles.id;

INSERT INTO module_roles (app_module_id,user_role_id,display_order,created_at,updated_at)
SELECT
	(SELECT id FROM app_modules WHERE title = 'Dashboard' LIMIT 1) app_module_id,
	(SELECT id FROM user_roles WHERE label = 'Admin' LIMIT 1) user_role_id,
	1 display_order,
	current_timestamp created_at,
	current_timestamp updated_at
UNION ALL
SELECT
	(SELECT id FROM app_modules WHERE title = 'Planning' LIMIT 1) app_module_id,
	(SELECT id FROM user_roles WHERE label = 'Admin' LIMIT 1) user_role_id,
	2 display_order,
	current_timestamp created_at,
	current_timestamp updated_at
UNION ALL
SELECT
	(SELECT id FROM app_modules WHERE title = 'Projects' LIMIT 1) app_module_id,
	(SELECT id FROM user_roles WHERE label = 'Admin' LIMIT 1) user_role_id,
	3 display_order,
	current_timestamp created_at,
	current_timestamp updated_at
UNION ALL
SELECT
	(SELECT id FROM app_modules WHERE title = 'Settings' LIMIT 1) app_module_id,
	(SELECT id FROM user_roles WHERE label = 'Admin' LIMIT 1) user_role_id,
	4 display_order,
	current_timestamp created_at,
	current_timestamp updated_at
UNION ALL
SELECT
	(SELECT id FROM app_modules WHERE title = 'Teams' LIMIT 1) app_module_id,
	(SELECT id FROM user_roles WHERE label = 'Admin' LIMIT 1) user_role_id,
	5 display_order,
	current_timestamp created_at,
	current_timestamp updated_at;
------------------------

------------------------
CREATE SEQUENCE project_types_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE TABLE project_types (
  id integer NOT NULL PRIMARY KEY DEFAULT nextval('project_types_id_seq'::regclass),
  label character varying(100) NULL,
  parent_project_type_id integer NULL,
  is_billable boolean NULL,
  current_team_id integer NULL,
  created_at timestamp without time zone,
  updated_at timestamp without time zone
);

ALTER SEQUENCE project_types_id_seq OWNED BY project_types.id;

INSERT INTO project_types
  (label,parent_project_type_id,is_billable,current_team_id,created_at,updated_at)
VALUES
  ('Software Development', 0, true, 0, current_timestamp, current_timestamp);

INSERT INTO project_types
  (label,parent_project_type_id,is_billable,current_team_id,created_at,updated_at)
VALUES
  ('Creative', 0, true, 0, current_timestamp, current_timestamp);

INSERT INTO project_types
  (label,parent_project_type_id,is_billable,current_team_id,created_at,updated_at)
VALUES
  ('Technical Design', 0, true, 0, current_timestamp, current_timestamp);
------------------------

------------------------
CREATE SEQUENCE project_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

CREATE TABLE projects (
  id integer NOT NULL PRIMARY KEY DEFAULT nextval('project_id_seq'::regclass),
  label character varying(100) NULL,
  description character varying(1000) NULL,
  display_order integer NULL,
  parent_project_id integer NULL,
  project_type_id  integer NULL,
  client_contact_user_id  integer NULL,
  projec_manager_user_id integer NULL,
  tech_lead_user_id integer NULL,
  design_lead_user_id integer NULL,
  estimate_id integer NULL,
  timecard_clients character varying(150)[] NULL,
  timecard_project_codes character varying(25)[] NULL,
  timecard_projects character varying(150)[] NULL,
  timecard_tasks character varying(150)[] NULL,
  active boolean NULL,
  created_at timestamp without time zone,
  updated_at timestamp without time zone
);

ALTER SEQUENCE project_id_seq OWNED BY projects.id;

INSERT INTO projects (
  label,
  description,
  display_order,
  parent_project_id,
  project_type_id,
  timecard_clients,
  timecard_project_codes,
  timecard_projects,
  timecard_tasks,
  active,
  created_at,
  updated_at
)
SELECT
  'Sample Project' "label",
  'Sample Accounting for System' description,
  1 display_order,
  0 parent_project_id,
  --(SELECT id FROM project_types WHERE label = 'Creative' LIMIT 1) project_type_id,
  (SELECT id FROM project_types WHERE label = 'Software Development' LIMIT 1) project_type_id,
  ARRAY['Sample'] timecard_clients,
  ARRAY['DE','CE'] timecard_project_codes,
  ARRAY[NULL] timecard_projects,
  ARRAY[NULL] timecard_tasks,
  true active,
  current_timestamp created_at,
  current_timestamp updated_at
------------------------

------------------------
-- CREATE TIMECARD DETAILS TABLE
CREATE TABLE timecard_details (
  effective_date date NULL,
  client character varying(150) NULL,
  project character varying(150) NULL,
  project_code character varying(25) NULL,
  task character varying(150) NULL,
  notes character varying(1000) NULL,
  hours double precision NULL,
  started_at time without time zone NULL,
  ended_at time without time zone NULL,
  billable character varying(3) NULL,
  invoiced character varying(3) NULL,
  approved character varying(3) NULL,
  first_name character varying(100) NULL,
  last_name character varying(100) NULL,
  department character varying(150) NULL,
  employee character varying(3) NULL,
  billable_rate double precision NULL,
  billable_amount double precision NULL,
  cost_rate double precision NULL,
  cost_amount double precision NULL,
  currency character varying(100) NULL,
  external_reference_url character varying(250) NULL
);
------------------------

--http://www.sqlines.com/postgresql/how-to/return_result_set_from_stored_procedure
