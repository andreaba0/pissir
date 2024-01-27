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

alter table profile_company
add foreign key (vat_number) references company(vat_number);

alter table company_far
add foreign key (vat_number, industry_sector) references company(vat_number, industry_sector);

alter table company_wsp
add foreign key (vat_number, industry_sector) references company(vat_number, industry_sector);

alter table presentation_letter
add foreign key (user_account) references user_account(id);

alter table api_acl
add foreign key (person_fa) references person_fa(account_id);

alter table api_acl_request
add foreign key (person_fa) references person_fa(account_id);