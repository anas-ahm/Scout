CREATE TABLE Activity (
    activity_id     INT      AUTO_INCREMENT     PRIMARY KEY,
    name     VARCHAR(255)     NOT NULL,
    start_time     DATETIME      NOT NULL,
    description     VARCHAR(2500) NULL,      
    max_count     INT UNSIGNED    NULL,      -- if NULL, there's no limit on members
    min_age     INT UNSIGNED     NULL       -- if NULL, there's no age limit
);

CREATE TABLE Member (
    member_id    AUTO_INCREMENT PRIMARY KEY,
    first_name   VARCHAR(255) NOT NULL,
    last_name    VARCHAR(255) NOT NULL,
    date_of_birth DATE         NOT NULL,
    phone_number CHAR(8) NOT NULL
    CHECK (LEN(phone_number) = 8),  
    is_leader    BOOL         NOT NULL DEFAULT FALSE,
    username    VARCHAR(255) NOT NULL UNIQUE,
    password    VARCHAR(255) NOT NULL   
);

CREATE TABLE Badge (
    badge_id     AUTO_INCREMENT PRIMARY KEY,
    name        VARCHAR(255) NOT NULL UNIQUE,
    description VARCHAR(2500) NULL
);

CREATE TABLE ActivityMember (
    activity_id       INT NOT NULL REFERENCES Activity(activity_id) ON DELETE CASCADE,
    member_id         INT NOT NULL REFERENCES Member(member_id)     ON DELETE CASCADE,
    PRIMARY KEY (ActivityID, MemberID)
);

CREATE TABLE MemberBadge (
    member_id      INT NOT NULL REFERENCES Member(member_id) ON DELETE CASCADE,
    badge_id       INT NOT NULL REFERENCES Badge(badge_id)   ON DELETE CASCADE,
    PRIMARY KEY (member_id, badge_id)
);

CREATE TABLE ActivityRequiredBadges (
    activity_id INT NOT NULL REFERENCES Activity(activity_id) ON DELETE CASCADE,
    badge_id    INT NOT NULL REFERENCES Badge(badge_id)       ON DELETE CASCADE,
    PRIMARY KEY (activity_id, badge_id) 
);