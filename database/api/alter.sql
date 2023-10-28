alter table utente_idrico add foreign key (role) references user_role(role);
alter table utente_agricolo add foreign key (role) references user_role(role);

alter table utente_idrico add foreign key (codice_fiscale) references utente(codice_fiscale);
alter table utente_agricolo add foreign key (codice_fiscale) references utente(codice_fiscale);

alter table azienda_idrica add foreign key (categoria) references company_category(category);
alter table azienda_agricola add foreign key (categoria) references company_category(category);

alter table azienda_idrica add foreign key (partita_iva) references azienda(partita_iva);
alter table azienda_agricola add foreign key (partita_iva) references azienda(partita_iva);

alter table work_relation add foreign key (codice_fiscale) references utente(codice_fiscale);
alter table work_relation add foreign key (partita_iva) references azienda(partita_iva);

alter table offer add foreign key (partita_iva) references azienda(partita_iva);

alter table buy_order add foreign key (offer_id) references offer(id);
alter table buy_order add foreign key (campo_id) references campo(id);

alter table campo add foreign key (partita_iva) references azienda(partita_iva);
alter table campo add foreign key (tipo_irrigazione) references tipo_irrigazione(tipo);

alter table object_logger add foreign key (sensor_type) references sensor_type(id);
alter table object_logger add foreign key (campo_id) references campo(id);

alter table umdty_sensor_log add foreign key (object_id) references object_logger(id);
alter table umdty_sensor_log add foreign key (sensor_type) references sensor_type(id);

alter table tmp_sensor_log add foreign key (object_id) references object_logger(id);
alter table tmp_sensor_log add foreign key (sensor_type) references sensor_type(id);

alter table actuator_log add foreign key (object_id) references object_logger(id);