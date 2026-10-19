CREATE TABLE Activity (
    ActivityID     INT      IDENTITY(1,1)     PRIMARY KEY,
    Name     VARCHAR(155)     NOT NULL,
    StartTime     DATETIME      NOT NULL,
    Description     VARCHAR(2500) NULL,      
    MaxCount     INT     NULL,      -- if NULL, there's no limit on members
    MinAge     INT     NULL       -- if NULL, there's no age limit
);

CREATE TABLE Member (
    MemberID    INT IDENTITY(1,1) PRIMARY KEY,
    FirstName   VARCHAR(155) NOT NULL,
    LastName    VARCHAR(155) NOT NULL,
    Age         INT          NOT NULL,
    PhoneNumber VARCHAR(155) NOT NULL
    CHECK (LEN(PhoneNumber) = 8),  
    IsLeader    BOOL         NOT NULL DEFAULT FALSE,
    Username    VARCHAR(155) NOT NULL UNIQUE,
    Password    VARCHAR(155) NOT NULL   
);

CREATE TABLE Badge (
    BadgeID     INT IDENTITY(1,1) PRIMARY KEY,
    Name        VARCHAR(155) NOT NULL UNIQUE,
    Description VARCHAR(2500) NULL
);

CREATE TABLE ActivityMember (
    ActivityMemberID INT IDENTITY(1,1) PRIMARY KEY,
    ActivityID       INT NOT NULL REFERENCES Activity(ActivityID) ON DELETE CASCADE,
    MemberID         INT NOT NULL REFERENCES Member(MemberID)     ON DELETE CASCADE,
    UNIQUE (ActivityID, MemberID) 
);

CREATE TABLE MemberBadge (
    MemberBadgeID INT IDENTITY(1,1) PRIMARY KEY,
    MemberID      INT NOT NULL REFERENCES Member(MemberID) ON DELETE CASCADE,
    BadgeID       INT NOT NULL REFERENCES Badge(BadgeID)   ON DELETE CASCADE,
    UNIQUE (MemberID, BadgeID)
);

CREATE TABLE ActivityRequiredBadges (
    ActivityID INT NOT NULL REFERENCES Activity(ActivityID) ON DELETE CASCADE,
    BadgeID    INT NOT NULL REFERENCES Badge(BadgeID)       ON DELETE CASCADE,
    PRIMARY KEY (ActivityID, BadgeID) 
);