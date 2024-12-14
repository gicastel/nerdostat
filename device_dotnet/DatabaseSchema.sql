CREATE TABLE IF NOT EXISTS messages (
    Id INTEGER PRIMARY KEY,
    Timestamp TEXT,
    Temperature REAL,
    Humidity REAL,
    HeaterOn INTEGER,
    IsHeaterOn TEXT
);

CREATE INDEX IF NOT EXISTS messages_Timestamp ON messages(Timestamp);

CREATE TABLE IF NOT EXISTS models (
    Id INTEGER PRIMARY KEY,
    Timestamp TEXT,
    RMSE REAL
);

CREATE TABLE IF NOT EXISTS meteodata (
    Id INTEGER PRIMARY KEY,
    Timestamp UNIQUE TEXT,
    Temperature REAL,
    Humidity REAL,
    Precipitation REAL,
    CloudCover REAL
);

CREATE INDEX IF NOT EXISTS meteodata_Timestamp ON meteodata(Timestamp);

DROP VIEW IF EXISTS traindata;

CREATE VIEW traindata AS
    SELECT
        msg.Temperature,
        SIN(2 * PI() * CAST(STRFTIME("%m", msg.Timestamp) AS REAL) / 12) AS month,
        SIN(2 * PI() * CAST(STRFTIME("%j", msg.Timestamp) AS REAL) / 366) AS day,
        SIN(2 * PI() * CAST(STRFTIME("%H", msg.Timestamp) AS REAL) / 24) AS hour,
        LAG (msg.Temperature, 1) OVER (ORDER BY Id DESC) AS tempLag1,
        LAG (msg.Temperature, 2) OVER (ORDER BY Id DESC) AS tempLag2,
        LAG (msg.Temperature, 3) OVER (ORDER BY Id DESC) AS tempLag3,
        LAG (msg.Temperature, 4) OVER (ORDER BY Id DESC) AS tempLag4,
        LAG (msg.Temperature, 5) OVER (ORDER BY Id DESC) AS tempLag5,
        LAG (msg.Temperature, 6) OVER (ORDER BY Id DESC) AS tempLag6,
        LAG (msg.Temperature, 7) OVER (ORDER BY Id DESC) AS tempLag7,
        LAG (msg.Temperature, 8) OVER (ORDER BY Id DESC) AS tempLag8,
        LAG (msg.Temperature, 9) OVER (ORDER BY Id DESC) AS tempLag9,
        LAG (msg.Temperature, 10) OVER (ORDER BY Id DESC) AS tempLag10,
        LAG (msg.Temperature, 11) OVER (ORDER BY Id DESC) AS tempLag11,
        LAG (msg.Temperature, 12) OVER (ORDER BY Id DESC) AS tempLag12,
        LAG (msg.Temperature, 13) OVER (ORDER BY Id DESC) AS tempLag13,
        LAG (msg.Temperature, 14) OVER (ORDER BY Id DESC) AS tempLag14,
        LAG (msg.Temperature, 15) OVER (ORDER BY Id DESC) AS tempLag15,
        LAG (msg.Temperature, 16) OVER (ORDER BY Id DESC) AS tempLag16,
        LAG (msg.Temperature, 17) OVER (ORDER BY Id DESC) AS tempLag17,
        LAG (msg.Temperature, 18) OVER (ORDER BY Id DESC) AS tempLag18,
        LAG (msg.Temperature, 19) OVER (ORDER BY Id DESC) AS tempLag19,
        LAG (msg.Temperature, 20) OVER (ORDER BY Id DESC) AS tempLag20,
        LAG (msg.Temperature, 21) OVER (ORDER BY Id DESC) AS tempLag21,
        LAG (msg.Temperature, 22) OVER (ORDER BY Id DESC) AS tempLag22,
        LAG (msg.Temperature, 23) OVER (ORDER BY Id DESC) AS tempLag23,
        LAG (msg.Temperature, 24) OVER (ORDER BY Id DESC) AS tempLag24,
        currentMeteo.Temperature AS currentTemp,
        currentMeteo.Humidity AS currentHumidity,
        currentMeteo.Precipitation AS currentPrecipitation,
        currentMeteo.CloudCover AS currentCloudCover,
        back1Hour.Temperature AS back1HourTemp,
        back1Hour.Humidity AS back1HourHumidity,
        back1Hour.Precipitation AS back1HourPrecipitation,
        back1Hour.CloudCover AS back1HourCloudCover,
        back2Hour.Temperature AS back2HourTemp,
        back2Hour.Humidity AS back2HourHumidity,
        back2Hour.Precipitation AS back2HourPrecipitation,
        back2Hour.CloudCover AS back2HourCloudCover,
        back3Hour.Temperature AS back3HourTemp,
        back3Hour.Humidity AS back3HourHumidity,
        back3Hour.Precipitation AS back3HourPrecipitation,
        back3Hour.CloudCover AS back3HourCloudCover,
        back4Hour.Temperature AS back4HourTemp,
        back4Hour.Humidity AS back4HourHumidity,
        back4Hour.Precipitation AS back4HourPrecipitation,
        back4Hour.CloudCover AS back4HourCloudCover,
        back5Hour.Temperature AS back5HourTemp,
        back5Hour.Humidity AS back5HourHumidity,
        back5Hour.Precipitation AS back5HourPrecipitation,
        back5Hour.CloudCover AS back5HourCloudCover,
        back6Hour.Temperature AS back6HourTemp,
        back6Hour.Humidity AS back6HourHumidity,
        back6Hour.Precipitation AS back6HourPrecipitation,
        back6Hour.CloudCover AS back6HourCloudCover,
        nextHour.Temperature AS nextHourTemp,
        nextHour.Humidity AS nextHourHumidity,
        nextHour.Precipitation AS nextHourPrecipitation,
        nextHour.CloudCover AS nextHourCloudCover
    FROM messages as msg
    INNER JOIN meteodata AS currentMeteo
        ON STRFTIME('%Y-%m-%d %H:00:00', messages.Timestamp) = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back1Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', messages.Timestamp, '-1 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back2Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', messages.Timestamp, '-2 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back3Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', messages.Timestamp, '-3 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back4Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', messages.Timestamp, '-4 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back5Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', messages.Timestamp, '-5 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back6Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', messages.Timestamp, '-6 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS nextHour
        ON STRFTIME('%Y-%m-%d %H:00:00', messages.Timestamp, '+1 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp);

DROP VIEW IF EXISTS predictdata;

CREATE VIEW predictdata AS
    SELECT
        NULL AS Temperature,
        SIN(2 * PI() * CAST(STRFTIME("%m", datetime(current_timestamp, 'localtime')) AS REAL) / 12) AS month,
        SIN(2 * PI() * CAST(STRFTIME("%j", datetime(current_timestamp, 'localtime')) AS REAL) / 366) AS day,
        SIN(2 * PI() * CAST(STRFTIME("%H", datetime(current_timestamp, 'localtime')) AS REAL) / 24) AS hour,
        msg.Temperature AS tempLag1,
        LAG (msg.Temperature, 1) OVER (ORDER BY Id DESC) AS tempLag2,
        LAG (msg.Temperature, 2) OVER (ORDER BY Id DESC) AS tempLag3,
        LAG (msg.Temperature, 3) OVER (ORDER BY Id DESC) AS tempLag4,
        LAG (msg.Temperature, 4) OVER (ORDER BY Id DESC) AS tempLag5,
        LAG (msg.Temperature, 5) OVER (ORDER BY Id DESC) AS tempLag6,
        LAG (msg.Temperature, 6) OVER (ORDER BY Id DESC) AS tempLag7,
        LAG (msg.Temperature, 7) OVER (ORDER BY Id DESC) AS tempLag8,
        LAG (msg.Temperature, 8) OVER (ORDER BY Id DESC) AS tempLag9,
        LAG (msg.Temperature, 9) OVER (ORDER BY Id DESC) AS tempLag10,
        LAG (msg.Temperature, 10) OVER (ORDER BY Id DESC) AS tempLag11,
        LAG (msg.Temperature, 11) OVER (ORDER BY Id DESC) AS tempLag12,
        LAG (msg.Temperature, 12) OVER (ORDER BY Id DESC) AS tempLag13,
        LAG (msg.Temperature, 13) OVER (ORDER BY Id DESC) AS tempLag14,
        LAG (msg.Temperature, 14) OVER (ORDER BY Id DESC) AS tempLag15,
        LAG (msg.Temperature, 15) OVER (ORDER BY Id DESC) AS tempLag16,
        LAG (msg.Temperature, 16) OVER (ORDER BY Id DESC) AS tempLag17,
        LAG (msg.Temperature, 17) OVER (ORDER BY Id DESC) AS tempLag18,
        LAG (msg.Temperature, 18) OVER (ORDER BY Id DESC) AS tempLag19,
        LAG (msg.Temperature, 19) OVER (ORDER BY Id DESC) AS tempLag20,
        LAG (msg.Temperature, 20) OVER (ORDER BY Id DESC) AS tempLag21,
        LAG (msg.Temperature, 21) OVER (ORDER BY Id DESC) AS tempLag22,
        LAG (msg.Temperature, 22) OVER (ORDER BY Id DESC) AS tempLag23,
        LAG (msg.Temperature, 23) OVER (ORDER BY Id DESC) AS tempLag24,
        currentMeteo.Temperature AS currentTemp,
        currentMeteo.Humidity AS currentHumidity,
        currentMeteo.Precipitation AS currentPrecipitation,
        currentMeteo.CloudCover AS currentCloudCover,
        back1Hour.Temperature AS back1HourTemp,
        back1Hour.Humidity AS back1HourHumidity,
        back1Hour.Precipitation AS back1HourPrecipitation,
        back1Hour.CloudCover AS back1HourCloudCover,
        back2Hour.Temperature AS back2HourTemp,
        back2Hour.Humidity AS back2HourHumidity,
        back2Hour.Precipitation AS back2HourPrecipitation,
        back2Hour.CloudCover AS back2HourCloudCover,
        back3Hour.Temperature AS back3HourTemp,
        back3Hour.Humidity AS back3HourHumidity,
        back3Hour.Precipitation AS back3HourPrecipitation,
        back3Hour.CloudCover AS back3HourCloudCover,
        back4Hour.Temperature AS back4HourTemp,
        back4Hour.Humidity AS back4HourHumidity,
        back4Hour.Precipitation AS back4HourPrecipitation,
        back4Hour.CloudCover AS back4HourCloudCover,
        back5Hour.Temperature AS back5HourTemp,
        back5Hour.Humidity AS back5HourHumidity,
        back5Hour.Precipitation AS back5HourPrecipitation,
        back5Hour.CloudCover AS back5HourCloudCover,
        back6Hour.Temperature AS back6HourTemp,
        back6Hour.Humidity AS back6HourHumidity,
        back6Hour.Precipitation AS back6HourPrecipitation,
        back6Hour.CloudCover AS back6HourCloudCover,
        nextHour.Temperature AS nextHourTemp,
        nextHour.Humidity AS nextHourHumidity,
        nextHour.Precipitation AS nextHourPrecipitation,
        nextHour.CloudCover AS nextHourCloudCover
    FROM messages as msg
    INNER JOIN meteodata AS currentMeteo
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime')) = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back1Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-1 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back2Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-2 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back3Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-3 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back4Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-4 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back5Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-5 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS back6Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-6 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp)
    INNER JOIN meteodata AS nextHour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '+1 hour') = STRFTIME('%Y-%m-%d %H:00:00', m.Timestamp);
    LIMIT 1;

DELETE FROM messages WHERE Temperature = -273.15;