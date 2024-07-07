from postgres:14.0-alpine

copy schema.sql /tmp/schema.sql
copy alter.sql /tmp/alter.sql
run cat /tmp/schema.sql /tmp/alter.sql > /docker-entrypoint-initdb.d/init.sql
