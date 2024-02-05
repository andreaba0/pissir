create table industry_sector (
    sector_name varchar(2) primary key check(sector_name in ('WA', 'FA'))
);
create table person(
    internal_id bigint primary key,
    global_id uuid unique not null,
    company_vat_number varchar(11) not null
);
create table company(
    vat_number varchar(11) primary key,
    company_name text not null,
    industry_sector varchar(2) not null,
    unique (vat_number, industry_sector)
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