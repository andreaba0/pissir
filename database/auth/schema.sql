create table registered_provider_iss (
    provider_uri text primary key, -- corresponds to the iss field in the (openid standard) id_token
    registered_provider text not null
);

create table registered_provider (
    name text primary key
);

create table user_account (
    id bigserial primary key,
    registered_provider text not null,
    unique (registered_provider, id)
);

create table user_role (
    role_name varchar(3) primary key check(role_name in ('WSP', 'FAR'))
);
create table industry_sector (
    sector_name varchar(2) primary key check(sector_name in ('WA', 'FA'))
);

create table person(
    global_id uuid unique not null default gen_random_uuid(),
    account_id bigint primary key,
    tax_code varchar(16) not null unique,
    given_name text not null,
    family_name text not null,
    email text not null,
    person_role varchar(3) not null,
    unique (tax_code, person_role)
);
create table user_wsp(
    person_id bigint primary key,
    person_role varchar(3) not null check(person_role = 'WSP')
);
create table farmer(
    person_id bigint primary key,
    person_role varchar(3) not null check(person_role = 'FAR')
);

create table company(
    vat_number varchar(11) primary key,
    industry_sector varchar(2) not null,
    unique (vat_number, industry_sector)
);

create table water_company(
    vat_number varchar(11) primary key,
    industry_sector varchar(2) not null check(industry_sector = 'WA')
);
create table farm(
    vat_number varchar(11) primary key,
    industry_sector varchar(2) not null check(industry_sector = 'FA')
);


create table presentation_letter (
    user_account bigint not null,
    name text not null,
    surname text not null,
    email text not null,
    tax_code varchar(16) not null,
    company_vat_number varchar(11) not null,
    company_industry_sector varchar(2) not null,
    primary key (user_account, company_vat_number, company_industry_sector)
);

create table api_acl (
    person_id bigint not null,
    date_start timestamptz not null,
    date_end timestamptz not null check(date_end > date_start),
    primary key (person_id, date_start, date_end)
);

create table api_acl_request (
    person_id bigint not null,
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

create table pending_transaction (
    id uuid primary key,
    body_data jsonb not null,
    data_type text not null,
    created_at timestamptz not null,
    updated_at timestamptz not null
);

create table transaction_round_trip_time (
    date_span timestamptz not null primary key,
    round_trip_time numeric not null
);