create table allowed_audience (
    registered_provider text not null,
    audience text not null,
    primary key (registered_provider, audience)
);

create table registered_provider (
    provider_name text primary key,
    configuration_uri text not null unique
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

create table presentation_letter (
    user_account bigint not null,
    presentation_id uuid not null unique default gen_random_uuid(),
    given_name text not null,
    family_name text not null,
    email text not null,
    tax_code varchar(16) not null,
    company_vat_number varchar(11) not null,
    company_industry_sector varchar(3) not null,
    created_at timestamptz not null default now(),
    primary key (user_account)
);

create table api_acl (
    person_fa bigserial not null,
    sdate timestamptz not null,
    edate timestamptz not null check(edate > sdate),
    primary key (person_fa, sdate, edate)
);

create table api_acl_request (
    acl_id uuid not null primary key,
    person_fa bigint not null,
    sdate timestamptz not null,
    edate timestamptz not null check(edate > sdate),
    created_at timestamptz not null default now()
);

create table rsa (
    id uuid primary key,
    d bytea not null,
    dp bytea not null,
    dq bytea not null,
    exponent bytea not null,
    inverse_q bytea not null,
    modulus bytea not null,
    p bytea not null,
    q bytea not null,
    created_at timestamptz not null default now()
);