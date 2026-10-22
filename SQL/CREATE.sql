CREATE TABLE activities (
    activity_id INT AUTO_INCREMENT PRIMARY KEY,
    name        VARCHAR(255) NOT NULL,
    date_time   DATETIME NOT NULL,
    description VARCHAR(2047) NULL,
    max_count   INT UNSIGNED NULL, -- if NULL, there's no member limit
    min_age     INT UNSIGNED NULL  -- if NULL, there's no age limit
);

CREATE TABLE members (
    member_id     INT AUTO_INCREMENT PRIMARY KEY,
    first_name    VARCHAR(255) NOT NULL,
    last_name     VARCHAR(255) NOT NULL,
    date_of_birth DATE NOT NULL,
    phone_number  CHAR(8) NOT NULL
        CHECK (CHAR_LENGTH(phone_number) = 8),
    is_leader     BOOLEAN NOT NULL DEFAULT FALSE,
    username      VARCHAR(255) NOT NULL UNIQUE,
    password      VARCHAR(255) NOT NULL
);

CREATE TABLE badges (
    badge_id    INT AUTO_INCREMENT PRIMARY KEY,
    name        VARCHAR(255) NOT NULL UNIQUE,
    description VARCHAR(2047) NULL
);

CREATE TABLE activity_members (
    activity_id INT NOT NULL,
    member_id   INT NOT NULL,

    PRIMARY KEY (activity_id, member_id),

    CONSTRAINT fk_activity_members_activity
        FOREIGN KEY (activity_id)
        REFERENCES activities(activity_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_activity_members_member
        FOREIGN KEY (member_id)
        REFERENCES members(member_id)
        ON DELETE CASCADE
);

CREATE TABLE member_badges (
    member_id INT NOT NULL,
    badge_id  INT NOT NULL,

    PRIMARY KEY (member_id, badge_id),

    CONSTRAINT fk_member_badges_member
        FOREIGN KEY (member_id)
        REFERENCES members(member_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_member_badges_badge
        FOREIGN KEY (badge_id)
        REFERENCES badges(badge_id)
        ON DELETE CASCADE
);

CREATE TABLE required_activity_badges (
    activity_id INT NOT NULL,
    badge_id    INT NOT NULL,

    PRIMARY KEY (activity_id, badge_id),

    CONSTRAINT fk_required_activity_badges_activity
        FOREIGN KEY (activity_id)
        REFERENCES activities(activity_id)
        ON DELETE CASCADE,

    CONSTRAINT fk_required_activity_badges_badge
        FOREIGN KEY (badge_id)
        REFERENCES badges(badge_id)
        ON DELETE CASCADE
);
