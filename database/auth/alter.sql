alter table allowed_audience
add foreign key (registered_provider) references registered_provider(provider_name);

alter table user_account
add foreign key (registered_provider) references registered_provider(provider_name);

alter table person
add foreign key (account_id) references user_account(id);

alter table person
add foreign key (company_vat_number) references company(vat_number);

alter table company
add foreign key (industry_sector) references industry_sector(sector_name);

alter table presentation_letter
add foreign key (user_account) references user_account(id);

alter table presentation_letter
add foreign key (company_industry_sector) references industry_sector(sector_name);

alter table presentation_letter
add constraint enforce_company_type foreign key (company_vat_number, company_industry_sector) references company(vat_number, industry_sector);



/*
    The following 4 foreign key constraints are needed to ensure that only FARM workers can request and get temporary access to the API.
    Water Service Provider users have access to the API by default, so they don't need to request access.
*/
alter table api_acl
add constraint enforce_user_relationship foreign key (person_id, company_vat_number) references person(account_id, company_vat_number);
alter table api_acl
add constraint enforce_work_relationship foreign key (company_vat_number, company_industry_sector) references company(vat_number, industry_sector);
alter table api_acl_request
add constraint enforce_user_relationship foreign key (person_id, company_vat_number) references person(account_id, company_vat_number);
alter table api_acl_request
add constraint enforce_work_relationship foreign key (company_vat_number, company_industry_sector) references company(vat_number, industry_sector);