CREATE TABLE activities (
    activity_id INT AUTO_INCREMENT PRIMARY KEY,
    name        VARCHAR(255)  NOT NULL,
    start_time  DATETIME      NOT NULL,
    description VARCHAR(2500) NULL,
    max_count   INT UNSIGNED  NULL, -- if NULL, there's no limit on members
    min_age     INT UNSIGNED  NULL  -- if NULL, there's no age limit
);

CREATE TABLE members (
    member_id     INT AUTO_INCREMENT PRIMARY KEY,
    first_name    VARCHAR(255) NOT NULL,
    last_name     VARCHAR(255) NOT NULL,
    date_of_birth DATE         NOT NULL,
    phone_number  CHAR(8)      NOT NULL CHECK (CHAR_LENGTH(phone_number) = 8),
    is_leader     BOOLEAN      NOT NULL DEFAULT FALSE,
    username      VARCHAR(255) NOT NULL UNIQUE,
    password      VARCHAR(255) NOT NULL
);

CREATE TABLE badges (
    badge_id    INT AUTO_INCREMENT PRIMARY KEY,
    name        VARCHAR(255)  NOT NULL UNIQUE,
    description VARCHAR(4095) NULL
);

CREATE TABLE activity_members (
    activity_id INT NOT NULL REFERENCES activities(activity_id) ON DELETE CASCADE,
    member_id   INT NOT NULL REFERENCES members(member_id)      ON DELETE CASCADE,
    PRIMARY KEY (activity_id, member_id)
);

CREATE TABLE member_badges (
    member_id INT NOT NULL REFERENCES members(member_id) ON DELETE CASCADE,
    badge_id  INT NOT NULL REFERENCES badges(badge_id)   ON DELETE CASCADE,
    PRIMARY KEY (member_id, badge_id)
);

CREATE TABLE activity_required_badges (
    activity_id INT NOT NULL REFERENCES activities(activity_id) ON DELETE CASCADE,
    badge_id    INT NOT NULL REFERENCES badges(badge_id)        ON DELETE CASCADE,
    PRIMARY KEY (activity_id, badge_id)
);