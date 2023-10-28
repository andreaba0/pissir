create table user_role (
    role varchar(3) primary key check(role in ('GSI', 'GAA'))
);
create table company_category (
    category varchar(2) primary key check(category in ('AA', 'AI'))
);
create table utente(
    codice_fiscale varchar(16) primary key,
    nome text not null,
    congnome text not null,
    role varchar(3) not null,
    unique (codice_fiscale, role)
);
create table utente_idrico(
    codice_fiscale varchar(16) primary key
    role varchar(3) not null check(role = 'GAA')
);
create table utente_agricolo(
    codice_fiscale varchar(16) primary key,
    role varchar(3) not null check(role = 'GSI')
);
create table azienda(
    partita_iva varchar(11) primary key,
    name text not null,
    indirizzo text not null,
    telefono varchar(10) not null,
    email text not null,
    categoria varchar(2) not null,
    unique (partita_iva, categoria)
);
create table azienda_idrica(
    partita_iva varchar(11) primary key,
    categoria varchar(2) not null check(categoria = 'AI')
);
create table azienda_agricola(
    partita_iva varchar(11) primary key,
    categoria varchar(2) not null check(categoria = 'AA')
);
create table work_relation(
    codice_fiscale varchar(16) references utente(codice_fiscale),
    partita_iva varchar(11) references azienda(partita_iva),
    primary key (codice_fiscale, partita_iva)
);
create table offer(
    id varchar(26) primary key,
    partita_iva varchar(11) not null,
    data_annuncio date not null,
    prezzo_litro float not null,
    qty float not null,
    unique(partita_iva, data_annuncio, prezzo_litro)
);
create table buy_order(
    offer_id varchar(26),
    campo_id varchar(26),
    qty float not null,
    primary key (offer_id, campo_id)
);
create table campo(
    id varchar(26) primary key,
    partita_iva varchar(11),
    metri_quadrati float not null,
    coltura text not null,
    tipo_irrigazione text not null
);
create table tipo_irrigazione(
    tipo text primary key
);
create table sensor_type(
    type text primary key
);
create table gestore(

);
create table object_logger(
    id varchar(26) primary key,
    type text not null,
    campo_id varchar(26) not null,
    unique (id, type)
);
create table umdty_sensor_log(
    object_id varchar(26),
    sensor_type text,
    log_time timestamptz,
    umdty float not null
)
create table tmp_sensor_log(
    object_id varchar(26),
    sensor_type text,
    log_time timestamptz,
    tmp float not null
);
create table actuator_log(
    object_id varchar(26),
    actuator_type text,
    log_time timestamptz,
    is_active boolean not null
);