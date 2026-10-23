-- one time only sql queries here

UPDATE activities
set max_count = 1, date_time = NOW() - INTERVAL 1 DAY
WHERE activity_id = 9