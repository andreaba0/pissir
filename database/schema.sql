create table utente(
    codice_fiscale varchar(16) primary key,
    nome text not null,
    congnome text not null,
    email text not null unique
);
create table utente_idrico(
    codice_fiscale varchar(16) primary key
);
create table utente_agricolo(
    codice_fiscale varchar(16) primary key
);
create table login(
    codice_fiscale varchar(16) primary key,
    password text not null,
    salt text not null,
    oauth_enabled boolean not null default true,
);
create table azienda();
create table azienda_idrica();
create table azienda_agricola();
create table offerta();
create table acquisto();
create table assegnazione();
create table campo();
create table gestore();
create table sensore();
create table log_sensore();
create table attuatore();
create table log_attuatore();
create table verifica_utente_agricolo();