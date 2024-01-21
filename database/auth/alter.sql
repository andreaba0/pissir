alter table registered_provider_iss
add foreign key (registered_provider) references registered_provider(name);

alter table user_account
add foreign key (registered_provider) references registered_provider(name);

alter table person
add foreign key (account_id) references user_account(id);

alter table user_wsp
add foreign key (person_id) references person(account_id);

alter table user_wsp
add foreign key (person_role) references user_role(role_name);

alter table farmer
add foreign key (person_id) references person(account_id);

alter table farmer
add foreign key (person_role) references user_role(role_name);

alter table company
add foreign key (industry_sector) references industry_sector(sector_name);

alter table presentation_letter
add foreign key (user_account) references user_account(id);

alter table presentation_letter
add foreign key (company_vat_number, company_industry_sector) references company(vat_number, industry_sector);

alter table api_acl
add foreign key (person_id) references farmer(person_id);

alter table api_acl_request
add foreign key (person_id) references farmer(person_id);

alter table water_company
add foreign key (vat_number) references company(vat_number);

alter table water_company
add foreign key (industry_sector) references industry_sector(sector_name);

alter table farm
add foreign key (vat_number) references company(vat_number);

alter table farm
add foreign key (industry_sector) references industry_sector(sector_name);