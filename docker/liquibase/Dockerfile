FROM liquibase/liquibase:latest

USER root
RUN curl -fsSL -o /tmp/postgresql.jar \
    https://jdbc.postgresql.org/download/postgresql-42.7.4.jar && \
    cp /tmp/postgresql.jar /liquibase/internal/lib/postgresql.jar && \
    chown liquibase:liquibase /liquibase/internal/lib/postgresql.jar && \
    rm /tmp/postgresql.jar
USER liquibase
