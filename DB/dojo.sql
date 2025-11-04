-- Таблиця користувачів
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    exp_points INTEGER DEFAULT 0,
    level INTEGER DEFAULT 1,
    current_streak INTEGER DEFAULT 0,
    last_completion_date DATE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Таблиця цілей (Goals/Plan)
CREATE TABLE goals (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    description TEXT NOT NULL,
    deadline TIMESTAMP,
    progress FLOAT DEFAULT 0,
    is_completed BOOLEAN DEFAULT FALSE,
    goal_created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


-- Тригер для автоматичного оновлення updated_at при UPDATE goals
CREATE OR REPLACE FUNCTION update_goal_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_update_goal_updated_at
BEFORE UPDATE ON goals
FOR EACH ROW
EXECUTE FUNCTION update_goal_updated_at();

-- Таблиця завдань (Tasks/ToDo)
CREATE TABLE tasks (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    goal_id INTEGER REFERENCES goals(id) ON DELETE SET NULL,
    description TEXT NOT NULL,
    is_completed BOOLEAN DEFAULT FALSE,
    due_date DATE,
    priority INTEGER DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP
);

-- Індекси для tasks
CREATE INDEX idx_tasks_user_id ON tasks(user_id);
CREATE INDEX idx_tasks_goal_id ON tasks(goal_id);
CREATE INDEX idx_tasks_due_date ON tasks(due_date);

-- Таблиця вкладень (Attachments)
CREATE TABLE attachments (
    id SERIAL PRIMARY KEY,
    task_id INTEGER NOT NULL REFERENCES tasks(id) ON DELETE CASCADE,
    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(512)
);

-- Таблиця помодоро-логів (Pomodoros)
CREATE TABLE pomodoros (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP,
    duration_minutes INTEGER,
    work_cycles INTEGER DEFAULT 0
);

-- Тригер для автоматичного розрахунку duration_minutes
CREATE OR REPLACE FUNCTION calc_pomodoro_duration()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.end_time IS NOT NULL THEN
        NEW.duration_minutes := EXTRACT(MINUTE FROM (NEW.end_time - NEW.start_time));
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_calc_pomodoro_duration
BEFORE INSERT OR UPDATE ON pomodoros
FOR EACH ROW
EXECUTE FUNCTION calc_pomodoro_duration();

-- Індекс для pomodoros
CREATE INDEX idx_pomodoros_user_id_start_time ON pomodoros(user_id, start_time);

ALTER TABLE users
RENAME COLUMN password_hash TO password;


ALTER TABLE goals
DROP COLUMN is_completed;

ALTER TABLE goals
RENAME COLUMN goal_created_at TO created_at;

INSERT INTO users (email, password, exp_points, level) 
VALUES ('john.doe@example.com', 'pass_hash_1', 150, 2);

INSERT INTO users (email, password, exp_points, level, current_streak, last_completion_date) 
VALUES ('jane.smith@example.com', 'pass_hash_2', 50, 1, 3, '2025-10-28');

INSERT INTO users (email, password) 
VALUES ('michael.b@example.com', 'pass_hash_3');

INSERT INTO users (email, password, exp_points, level) 
VALUES ('sophia.w@example.com', 'pass_hash_4', 1200, 5);

INSERT INTO users (email, password) 
VALUES ('david.k@example.com', 'pass_hash_5');

INSERT INTO goals (user_id, description, deadline, progress) 
VALUES (1, 'Learn SQL by the end of the year', '2025-12-31 23:59:59', 0.25);

INSERT INTO goals (user_id, description, progress) 
VALUES (1, 'Launch a new personal project', 0.10);

INSERT INTO goals (user_id, description, deadline, progress) 
VALUES (2, 'Read 5 books on design', '2025-11-30 23:59:59', 0.60);

INSERT INTO goals (user_id, description) 
VALUES (3, 'Start running 3 times a week');

INSERT INTO goals (user_id, description, deadline, progress) 
VALUES (4, 'Master CUDA for matrix multiplication', '2026-01-15 18:00:00', 0.05);

INSERT INTO goals (user_id, description, progress) 
VALUES (4, 'Refactor old C# code', 0.50);

INSERT INTO goals (user_id, description) 
VALUES (2, 'Organize a trip to the mountains');

INSERT INTO goals (user_id, description, deadline, progress) 
VALUES (5, 'Create a presentation on TypeScript', '2025-11-10 12:00:00', 0.90);

INSERT INTO tasks (user_id, goal_id, description, due_date, priority) 
VALUES (1, 1, 'Complete a course on JOINs', '2025-11-05', 2);

INSERT INTO tasks (user_id, goal_id, description, is_completed, completed_at) 
VALUES (1, 1, 'Install PostgreSQL and pgAdmin', TRUE, '2025-10-25 10:00:00');

INSERT INTO tasks (user_id, goal_id, description, is_completed, completed_at) 
VALUES (2, 3, 'Read "Dont Make Me Think"', TRUE, '2025-10-20 18:00:00');

INSERT INTO tasks (user_id, goal_id, description, due_date, priority) 
VALUES (2, 3, 'Read "The Design of Everyday Things"', '2025-11-15', 1);

INSERT INTO tasks (user_id, description, due_date, priority) 
VALUES (3, 'Buy groceries for the week', '2025-10-30', 3);

INSERT INTO tasks (user_id, description, is_completed, completed_at) 
VALUES (3, 'Pay the internet bill', TRUE, '2025-10-29 14:30:00');

INSERT INTO tasks (user_id, goal_id, description, priority) 
VALUES (4, 5, 'Set up the CUDA development environment', 2);

INSERT INTO tasks (user_id, goal_id, description, due_date) 
VALUES (4, 5, 'Write basic matrix multiplication', '2025-12-01');

INSERT INTO tasks (user_id, goal_id, description, is_completed, completed_at) 
VALUES (5, 8, 'Create slides 1-5 (Intro, Basics)', TRUE, '2025-10-28 17:00:00');

INSERT INTO tasks (user_id, goal_id, description, due_date, priority) 
VALUES (5, 8, 'Create slides 6-10 (Features, Interfaces)', '2025-11-01', 1);

INSERT INTO tasks (user_id, description, due_date) 
VALUES (1, 'Reply to email from "Mr. Andriy"', '2025-10-30');

INSERT INTO tasks (user_id, goal_id, description) 
VALUES (4, 6, 'Analyze old repository with Prims algorithm');

INSERT INTO attachments (task_id, file_name, file_path) 
VALUES (8, 'matrix_mul.cu', '/uploads/cuda/matrix_mul.cu');

INSERT INTO attachments (task_id, file_name, file_path) 
VALUES (9, 'slides_part1.pptx', '/uploads/presentations/slides_part1.pptx');

INSERT INTO attachments (task_id, file_name, file_path) 
VALUES (1, 'join_examples.sql', '/uploads/sql/join_examples.sql');

INSERT INTO attachments (task_id, file_name) 
VALUES (5, 'shopping_list.txt');

INSERT INTO pomodoros (user_id, start_time, end_time, work_cycles) 
VALUES (1, '2025-10-29 10:00:00', '2025-10-29 10:25:00', 1);

INSERT INTO pomodoros (user_id, start_time, end_time, work_cycles) 
VALUES (1, '2025-10-29 10:30:00', '2025-10-29 10:55:00', 1);

INSERT INTO pomodoros (user_id, start_time, end_time, work_cycles) 
VALUES (4, '2025-10-28 09:00:00', '2025-10-28 10:50:00', 4);

INSERT INTO pomodoros (user_id, start_time, end_time, work_cycles) 
VALUES (2, '2025-10-27 15:00:00', '2025-10-27 15:25:00', 1);

INSERT INTO pomodoros (user_id, start_time, end_time, work_cycles) 
VALUES (5, '2025-10-26 20:00:00', '2025-10-26 20:50:00', 2);

INSERT INTO pomodoros (user_id, start_time) 
VALUES (4, '2025-10-29 14:00:00');