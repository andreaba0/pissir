create table industry_sector (
    sector_name varchar(3) primary key check(sector_name in ('WSP', 'FAR'))
);

create table company(
    vat_number varchar(11) primary key,
    industry_sector varchar(3) not null,
    unique(vat_number, industry_sector)
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
    company_industry_sector varchar(3) not null check(company_industry_sector = 'WSP'),
    publish_date date not null,
    price_liter float not null check(price_liter >= 0),
    available_liters float not null check(available_liters >= 0),
    purchased_liters float not null check(purchased_liters >= 0) default 0,
    unique(vat_number, publish_date, price_liter)
);

alter table offer
add constraint purchase_coherence check (available_liters >= purchased_liters and available_liters - purchased_liters >= 0);

create table buy_order(
    offer_id varchar(26),
    farm_field_id varchar(26),
    qty float not null check(qty >= 0),
    primary key (offer_id, farm_field_id)
);

create table daily_water_limit(
    vat_number varchar(11) not null,
    company_industry_sector varchar(3) not null check(company_industry_sector = 'FAR'),
    consumption_sign smallint not null check(consumption_sign in (-1, 1)), --: -1 for unlimited [-inf; 0], 1 for limited [0; +limit]
    available float not null check (available >= 0),
    consumed float not null check (consumed >= 0),
    on_date date not null,
    primary key (vat_number, on_date)
);

alter table daily_water_limit 
add constraint consumption_coherence check (available >= (consumed * consumption_sign) and available - (consumed * consumption_sign) >= 0);


create table farm_field(
    id varchar(26) primary key,
    vat_number varchar(11),
    company_industry_sector varchar(2) not null check(company_industry_sector = 'FAR'),
    square_meters float not null,
    crop_type text not null,
    irrigation_type text not null,
    created_at timestamptz not null default now(),
);

create table irrigation_type(
    irrigation_name text primary key
);

create table object_logger(
    id varchar(26) primary key,
    object_type text not null check(object_type in ('UMDTY', 'TMP', 'ACTUATOR')),
    farm_field_id varchar(26) not null,
    unique (id, object_type)
);
create table umdty_sensor_log(
    object_id varchar(26),
    object_type text not null check(object_type = 'UMDTY'),
    log_time timestamptz not null,
    umdty float not null check (umdty >= 0 and umdty <= 100),
    primary key (object_id, log_time)
);
create table tmp_sensor_log(
    object_id varchar(26),
    object_type text not null check(object_type = 'TMP'),
    log_time timestamptz not null,
    tmp float not null,
    primary key (object_id, log_time)
);
create table actuator_log(
    object_id varchar(26),
    log_time timestamptz not null,
    is_active boolean not null,
    object_type text not null check(object_type = 'ACTUATOR'),
    primary key (object_id, log_time)
);

create table consumption_fact(
    crop text not null,
    liters_mq float not null,
    primary key (crop)
);

insert into consumption_fact (crop, liters_mq) values
('rice', 2000),
('grain', 1500),
('corn', 1800),
('wheat', 1600),
('barley', 1700),
('soy', 1400),
('sunflower', 1200),
('sugar_beet', 1300),
('potato', 1100),
('tomato', 1000),
('onion', 900),
('cotton', 800),
('tobacco', 700),
('olive', 600),
('vine', 500),
('fruit', 400),
('vegetable', 300),
('flower', 200),
('greenhouse', 100);


CREATE VIEW combined_sensor_log AS
SELECT object_id, object_type, log_time, umdty AS value
FROM umdty_sensor_log
UNION ALL
SELECT object_id, object_type, log_time, tmp AS value
FROM tmp_sensor_log;