create table registered_provider (
    name text primary key,
    iss text not null,
);

create table signin_credential (
    registered_provider text not null,
    id text not null,
    primary key (provider, id)
);

create table user_account (
    id text primary key,
    name text,
    surname text not null,
    email text not null,
    registered_provider text not null
);

create table presentation_letter (
    registered_provider text not null,
    name text not null,
    surname text not null,
    email text not null,
    signin_credential_id text not null,
    primary key (registered_provider, signin_credential_id),
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