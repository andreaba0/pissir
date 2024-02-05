alter table user_wsp add foreign key (user_role) references user_role(user_role);
alter table farmer add foreign key (user_role) references user_role(user_role);

alter table user_wsp add foreign key (tax_code) references person(tax_code);
alter table farmer add foreign key (tax_code) references person(tax_code);

alter table water_company add foreign key (industry_sector) references industry_sector(sector_name);
alter table farm add foreign key (industry_sector) references industry_sector(sector_name);

alter table water_company add foreign key (vat_number) references company(vat_number);
alter table farm add foreign key (vat_number) references company(vat_number);

alter table work_relation add foreign key (tax_code) references person(tax_code);
alter table work_relation add foreign key (vat_number) references company(vat_number);

alter table offer add foreign key (vat_number) references company(vat_number);

alter table buy_order add foreign key (offer_id) references offer(id);
alter table buy_order add foreign key (farm_field_id) references farm_field(id);

alter table farm_field add foreign key (vat_number) references company(vat_number);
alter table farm_field add foreign key (irrigation_type) references irrigation_type(irrigation_name);

alter table object_logger add foreign key (sensor_type) references sensor_type(id);
alter table object_logger add foreign key (farm_field_id) references farm_field(id);

alter table umdty_sensor_log add foreign key (object_id) references object_logger(id);
alter table umdty_sensor_log add foreign key (sensor_type) references sensor_type(id);

alter table tmp_sensor_log add foreign key (object_id) references object_logger(id);
alter table tmp_sensor_log add foreign key (sensor_type) references sensor_type(id);

alter table actuator_log add foreign key (object_id) references object_logger(id);