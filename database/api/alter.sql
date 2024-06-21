alter table allowed_audience
add foreign key (registered_provider) references registered_provider(provider_name);

alter table user_account
add foreign key (registered_provider) references registered_provider(provider_name);

alter table person
add foreign key (account_id) references user_account(id);

alter table person
add foreign key (person_role) references user_role(role_name);

alter table person_fa
add foreign key (account_id, role_name) references person(account_id, person_role);

alter table person_fa
add foreign key (company_vat_number) references company_far(vat_number);

alter table person_wa
add foreign key (account_id, role_name) references person(account_id, person_role);

alter table person_wa
add foreign key (company_vat_number) references company_wsp(vat_number);

alter table company
add foreign key (industry_sector) references industry_sector(sector_name);

alter table company_far
add foreign key (vat_number, industry_sector) references company(vat_number, industry_sector);

alter table company_wsp
add foreign key (vat_number, industry_sector) references company(vat_number, industry_sector);

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