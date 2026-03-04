CREATE TABLE change_log (
                           id BIGSERIAL PRIMARY KEY,
                           table_name VARCHAR(255) NOT NULL,
                           operation VARCHAR(255) NOT NULL, -- 'I' (insert), 'U' (update), 'D' (delete)
                           old_data JSONB,
                           new_data JSONB,
                           changed_fields JSONB,
                           changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                           changed_by VARCHAR(255) DEFAULT current_user
);

-- Индексы для быстрого поиска
CREATE INDEX idx_change_log_table ON change_log(table_name);
CREATE INDEX idx_change_log_changed_at ON change_log(changed_at);
CREATE INDEX idx_change_log_operation ON change_log(operation);

CREATE OR REPLACE FUNCTION change_trigger_function()
    RETURNS TRIGGER AS $$
DECLARE
    v_old_data JSONB;
    v_new_data JSONB;
    v_changed_fields JSONB;
BEGIN
    -- Определяем операцию
    IF (TG_OP = 'DELETE') THEN
        v_old_data = row_to_json(OLD)::JSONB;
        v_new_data = NULL;
        v_changed_fields = NULL;
    ELSIF (TG_OP = 'UPDATE') THEN
        v_old_data = row_to_json(OLD)::JSONB;
        v_new_data = row_to_json(NEW)::JSONB;

        -- Определяем измененные поля
        SELECT jsonb_object_agg(
                       key,
                       jsonb_build_object('old', v_old_data->key, 'new', v_new_data->key)
               ) INTO v_changed_fields
        FROM jsonb_each(v_old_data)
        WHERE v_old_data->key IS DISTINCT FROM v_new_data->key;

    ELSIF (TG_OP = 'INSERT') THEN
        v_old_data = NULL;
        v_new_data = row_to_json(NEW)::JSONB;
        v_changed_fields = NULL;
    END IF;

    -- Вставляем запись в аудит
    INSERT INTO change_log (
        table_name,
        operation,
        old_data,
        new_data,
        changed_fields,
        changed_by
    ) VALUES (
                 TG_TABLE_NAME,
                 TG_OP,
                 v_old_data,
                 v_new_data,
                 v_changed_fields,
                 current_user
             );

    -- Для AFTER триггера возвращаем соответствующий результат
    IF (TG_OP = 'DELETE') THEN
        RETURN OLD;
    ELSE
        RETURN NEW;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Пример для таблицы users
CREATE TRIGGER users_change_trigger
    AFTER INSERT OR UPDATE OR DELETE ON "Avatars"
    FOR EACH ROW EXECUTE FUNCTION change_trigger_function();

