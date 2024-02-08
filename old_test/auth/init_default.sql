insert into user_role(
    role_name
) values (
    'WA'
), (
    'FA'
);

insert into industry_sector(
    sector_name
) values (
    'WSP'
), (
    'FAR'
);

insert into registered_provider(
    provider_name,
    configuration_uri
) values (
    'test_provider',
    '<provider_uri>'
);

insert into allowed_audience(
    registered_provider,
    audience
) values (
    'test_provider',
    'internal_workspace@appweb.andreabarchietto.it'
);