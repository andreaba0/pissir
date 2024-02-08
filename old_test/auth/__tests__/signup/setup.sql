insert into user_account(
    id,
    registered_provider,
    sub
) values (
    1,
    'test_provider',
    '1234567890'
), (
    2,
    'test_provider',
    '0987654321'
), (
    3,
    'test_provider',
    '222333444'
);

insert into presentation_letter(
    user_account,
    presentation_id,
    given_name,
    family_name,
    email,
    tax_code,
    company_vat_number,
    company_industry_sector,
    created_at
) values (
    1,
    'fd3b3e3e-3e3e-3e3e-3e3e-3e3e3e3e3e3e',
    'John',
    'Doe',
    'john.doe@gmail.com',
    '1234567890123456',
    '12345678901',
    'WA',
    '2021-01-01T01:00:00Z'
), (
    2,
    'fd3b3e3e-afaf-3e3e-3e3e-3e3e3e3e3e3e',
    'Jane',
    'Doe',
    'jane.doe@gmail.com',
    '6543210987654321',
    '65432109876',
    'FA',
    '2021-01-01T01:00:00Z'
);