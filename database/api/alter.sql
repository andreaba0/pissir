alter table company_far
add foreign key (vat_number, industry_sector) references company(vat_number, industry_sector) on delete cascade;

alter table company_wsp
add foreign key (vat_number, industry_sector) references company(vat_number, industry_sector) on delete cascade;

alter table secret_key
add foreign key (company_vat_number) references company_far(vat_number) on delete cascade;

alter table offer add foreign key (vat_number) references company_wsp(vat_number) on delete cascade;
 
alter table buy_order add foreign key (offer_id) references offer(id);
alter table buy_order add foreign key (farm_field_id) references farm_field(id) on delete cascade;

alter table daily_water_limit add foreign key (vat_number) references company_far(vat_number) on delete cascade;

alter table farm_field add foreign key (vat_number) references company_far(vat_number) on delete cascade;

alter table farm_field_versioning add foreign key (field_id) references farm_field(id) on delete cascade;
alter table farm_field_versioning add foreign key (vat_number) references company_far(vat_number) on delete cascade;
alter table farm_field_versioning add foreign key (irrigation_type) references irrigation_type(irrigation_name) on delete cascade;
alter table farm_field_versioning add foreign key (crop_type) references consumption_fact(crop) on delete cascade;

alter table object_logger add foreign key (farm_field_id) references farm_field(id) on delete cascade;

alter table umdty_sensor_log add foreign key (object_id, object_type) references object_logger(id, object_type) on delete cascade;

alter table tmp_sensor_log add foreign key (object_id, object_type) references object_logger(id, object_type) on delete cascade;

alter table actuator_log add foreign key (object_id, object_type) references object_logger(id, object_type) on delete cascade;