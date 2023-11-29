create table user_role (
    role varchar(3) primary key check(role in ('WSP', 'FAR'))
);
create table industry_sector (
    sector_name varchar(2) primary key check(sector_name in ('WA', 'FA'))
);
create table person(
    tax_code varchar(16) primary key,
    name text not null,
    surname text not null,
    role varchar(3) not null,
    unique (tax_code, role)
);
create table user_wsp(
    tax_code varchar(16) primary key,
    role varchar(3) not null check(role = 'WSP')
);
create table farmer(
    tax_code varchar(16) primary key,
    role varchar(3) not null check(role = 'FAR')
);
create table company(
    vat_number varchar(11) primary key,
    name text not null,
    address text not null,
    phone_number varchar(10) not null,
    email text not null,
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
create table work_relation(
    tax_code varchar(16),
    vat_number varchar(11),
    primary key (tax_code, vat_number)
);
create table offer(
    id varchar(26) primary key,
    vat_number varchar(11) not null,
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
    square_meters float not null,
    crop_type text not null,
    irrigation_type text not null
);
create table irrigation_type(
    name text primary key
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
    umdty float not null
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