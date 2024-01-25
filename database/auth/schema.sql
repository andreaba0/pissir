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
    sector_name varchar(2) primary key check(sector_name in ('WA', 'FA'))
);


/*
    User role is automatically assigned based on the company he works for.
    So, no specific table column is needed to store the user role.
    A user can work for 1 company only.
*/
create table person(
    global_id uuid unique not null default gen_random_uuid(),
    tax_code varchar(16) not null unique,
    account_id bigint primary key,
    given_name text not null,
    family_name text not null,
    email text not null,
    company_vat_number varchar(11) not null,
    unique (account_id, company_vat_number)
);

create table company(
    vat_number varchar(11) primary key,
    industry_sector varchar(2) not null,
    unique (vat_number, industry_sector)
);

create table profile_company(
    vat_number varchar(11) primary key,
    industry_sector varchar(2) not null,
    company_name text not null,
    working_email_address text not null,
    working_phone_number varchar(10) not null,
    working_address text not null,
    unique (vat_number, industry_sector)
);

create table presentation_letter (
    user_account bigint not null,
    presentation_id uuid not null unique default gen_random_uuid(),
    given_name text not null,
    family_name text not null,
    email text not null,
    tax_code varchar(16) not null,
    company_vat_number varchar(11) not null,
    company_industry_sector varchar(2) not null,
    created_at timestamptz not null default now(),
    primary key (user_account)
);

create table api_acl (
    person_id bigint not null,
    company_vat_number varchar(11) not null,
    company_industry_sector varchar(2) not null check(company_industry_sector = 'FA'),
    date_start timestamptz not null,
    date_end timestamptz not null check(date_end > date_start),
    primary key (person_id, date_start, date_end)
);

create table api_acl_request (
    person_id bigint not null,
    company_vat_number varchar(11) not null,
    company_industry_sector varchar(2) not null check(company_industry_sector = 'FA'),
    date_start timestamptz not null,
    date_end timestamptz not null check(date_end > date_start),
    primary key (person_id, date_start, date_end)
);

create table rsa (
    id text primary key,
    d bytea not null,
    dp bytea not null,
    dq bytea not null,
    exponent bytea not null,
    inverse_q bytea not null,
    modulus bytea not null,
    p bytea not null,
    q bytea not null,
    created_at timestamptz not null
);