create table allowed_audience (
    registered_provider text not null,
    audience text not null,
    primary key (registered_provider, audience)
);

create table registered_provider (
    provider_name text primary key,
    configuration_uri text not null unique,
    is_active boolean not null default true
);

create table user_account (
    id bigserial primary key,
    registered_provider text not null,
    sub text not null,
    unique (registered_provider, sub)
);

create table industry_sector (
    sector_name varchar(3) primary key check(sector_name in ('WSP', 'FAR'))
);

create table user_role (
    role_name varchar(2) primary key check(role_name in ('WA', 'FA'))
);

create table person(
    global_id uuid unique not null default gen_random_uuid(),
    tax_code varchar(16) not null unique,
    account_id bigint primary key,
    given_name text not null,
    family_name text not null,
    email text not null,
    person_role varchar(2) not null,
    unique (account_id, person_role)
);

create table person_fa (
    account_id bigint primary key,
    role_name varchar(2) not null check(role_name = 'FA'),
    company_vat_number varchar(11) not null
);

create table person_wa (
    account_id bigint primary key,
    role_name varchar(2) not null check(role_name = 'WA'),
    company_vat_number varchar(11) not null
);

create table company(
    vat_number varchar(11) primary key,
    industry_sector varchar(3) not null,
    company_name text,
    working_email_address text,
    working_phone_number varchar(10),
    working_address text,
    unique (vat_number, industry_sector)
);

create table company_far (
    vat_number varchar(11) primary key,
    industry_sector varchar(3) not null check(industry_sector = 'FAR')
);

create table company_wsp (
    vat_number varchar(11) primary key,
    industry_sector varchar(3) not null check(industry_sector = 'WSP')
);

create table offer(
    id varchar(26) primary key,
    vat_number varchar(11) not null,
    company_industry_sector varchar(2) not null check(company_industry_sector = 'WA'),
    publish_date date not null,
    price_liter float not null,
    qty float not null,
    unique(vat_number, publish_date, price_liter)
);
create table buy_order(
    offer_id varchar(26),
    farm_field_id varchar(26),
    qty float not null,
    primary key (offer_id, farm_field_id)
);
create table farm_field(
    id varchar(26) primary key,
    vat_number varchar(11),
    company_industry_sector varchar(2) not null check(company_industry_sector = 'FA'),
    square_meters float not null,
    crop_type text not null,
    irrigation_type text not null
);
create table irrigation_type(
    irrigation_name text primary key
);
create table sensor_type(
    id text primary key
);
create table object_logger(
    id varchar(26) primary key,
    sensor_type text not null,
    farm_field_id varchar(26) not null,
    unique (id, sensor_type)
);
create table umdty_sensor_log(
    object_id varchar(26),
    sensor_type text,
    log_time timestamptz not null,
    umdty float not null check (umdty >= 0 and umdty <= 100)
);
create table tmp_sensor_log(
    object_id varchar(26),
    sensor_type text,
    log_time timestamptz not null,
    tmp float not null
);
create table actuator_log(
    object_id varchar(26),
    log_time timestamptz not null,
    is_active boolean not null
);