insert into registered_provider (provider_name, configuration_uri) values
('google', 'https://accounts.google.com/.well-known/openid-configuration'),
('facebook', 'https://www.facebook.com/.well-known/openid-configuration');

insert into allowed_audience (registered_provider, audience) values
('google', '330493585576-us7lib6fpk4bg0j1vcti09l0jpso2o4k.apps.googleusercontent.com'),
('facebook', '1664159533990922');