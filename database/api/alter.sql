alter table company
add foreign key (industry_sector) references industry_sector(sector_name);

alter table company_far
add foreign key (vat_number, industry_sector) references company(vat_number, industry_sector);

alter table company_wsp
add foreign key (vat_number, industry_sector) references company(vat_number, industry_sector);

alter table offer add foreign key (company_industry_sector) references industry_sector(sector_name);
alter table offer add foreign key (vat_number) references company_wsp(vat_number);
 
alter table buy_order add foreign key (offer_id) references offer(id);
alter table buy_order add foreign key (farm_field_id) references farm_field(id);

alter table daily_water_limit add foreign key (vat_number) references company_far(vat_number);

alter table farm_field add foreign key (vat_number) references company_far(vat_number);
alter table farm_field add foreign key (irrigation_type) references irrigation_type(irrigation_name);

alter table object_logger add foreign key (farm_field_id) references farm_field(id);

alter table umdty_sensor_log add foreign key (object_id) references object_logger(id);
alter table umdty_sensor_log add foreign key (object_id, object_type) references object_logger(id, object_type);

alter table tmp_sensor_log add foreign key (object_id) references object_logger(id);
alter table tmp_sensor_log add foreign key (object_id, object_type) references object_logger(id, object_type);

alter table actuator_log add foreign key (object_id) references object_logger(id);
alter table actuator_log add foreign key (object_id, object_type) references object_logger(id, object_type);