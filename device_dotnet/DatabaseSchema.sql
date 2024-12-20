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
    Timestamp TEXT UNIQUE,
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
        LAG (msg.Temperature, 1) OVER (ORDER BY msg.Id ) AS tempLag1,
        LAG (msg.Temperature, 2) OVER (ORDER BY msg.Id ) AS tempLag2,
        LAG (msg.Temperature, 3) OVER (ORDER BY msg.Id ) AS tempLag3,
        LAG (msg.Temperature, 4) OVER (ORDER BY msg.Id ) AS tempLag4,
        LAG (msg.Temperature, 5) OVER (ORDER BY msg.Id ) AS tempLag5,
        LAG (msg.Temperature, 6) OVER (ORDER BY msg.Id ) AS tempLag6,
        LAG (msg.Temperature, 7) OVER (ORDER BY msg.Id ) AS tempLag7,
        LAG (msg.Temperature, 8) OVER (ORDER BY msg.Id ) AS tempLag8,
        LAG (msg.Temperature, 9) OVER (ORDER BY msg.Id ) AS tempLag9,
        LAG (msg.Temperature, 10) OVER (ORDER BY msg.Id ) AS tempLag10,
        LAG (msg.Temperature, 11) OVER (ORDER BY msg.Id ) AS tempLag11,
        LAG (msg.Temperature, 12) OVER (ORDER BY msg.Id ) AS tempLag12,
        LAG (msg.Temperature, 13) OVER (ORDER BY msg.Id ) AS tempLag13,
        LAG (msg.Temperature, 14) OVER (ORDER BY msg.Id ) AS tempLag14,
        LAG (msg.Temperature, 15) OVER (ORDER BY msg.Id ) AS tempLag15,
        LAG (msg.Temperature, 16) OVER (ORDER BY msg.Id ) AS tempLag16,
        LAG (msg.Temperature, 17) OVER (ORDER BY msg.Id ) AS tempLag17,
        LAG (msg.Temperature, 18) OVER (ORDER BY msg.Id ) AS tempLag18,
        LAG (msg.Temperature, 19) OVER (ORDER BY msg.Id ) AS tempLag19,
        LAG (msg.Temperature, 20) OVER (ORDER BY msg.Id ) AS tempLag20,
        LAG (msg.Temperature, 21) OVER (ORDER BY msg.Id ) AS tempLag21,
        LAG (msg.Temperature, 22) OVER (ORDER BY msg.Id ) AS tempLag22,
        LAG (msg.Temperature, 23) OVER (ORDER BY msg.Id ) AS tempLag23,
        LAG (msg.Temperature, 24) OVER (ORDER BY msg.Id ) AS tempLag24,
        LAG (msg.Temperature, 25) OVER (ORDER BY msg.Id ) AS tempLag25,
        LAG (msg.Temperature, 26) OVER (ORDER BY msg.Id ) AS tempLag26,
        LAG (msg.Temperature, 27) OVER (ORDER BY msg.Id ) AS tempLag27,
        LAG (msg.Temperature, 28) OVER (ORDER BY msg.Id ) AS tempLag28,
        LAG (msg.Temperature, 29) OVER (ORDER BY msg.Id ) AS tempLag29,
        LAG (msg.Temperature, 30) OVER (ORDER BY msg.Id ) AS tempLag30,
        LAG (msg.Temperature, 31) OVER (ORDER BY msg.Id ) AS tempLag31,
        LAG (msg.Temperature, 32) OVER (ORDER BY msg.Id ) AS tempLag32,
        LAG (msg.Temperature, 33) OVER (ORDER BY msg.Id ) AS tempLag33,
        LAG (msg.Temperature, 34) OVER (ORDER BY msg.Id ) AS tempLag34,
        LAG (msg.Temperature, 35) OVER (ORDER BY msg.Id ) AS tempLag35,
        LAG (msg.Temperature, 36) OVER (ORDER BY msg.Id ) AS tempLag36,
        LAG (msg.Temperature, 37) OVER (ORDER BY msg.Id ) AS tempLag37,
        LAG (msg.Temperature, 38) OVER (ORDER BY msg.Id ) AS tempLag38,
        LAG (msg.Temperature, 39) OVER (ORDER BY msg.Id ) AS tempLag39,
        LAG (msg.Temperature, 40) OVER (ORDER BY msg.Id ) AS tempLag40,
        LAG (msg.Temperature, 41) OVER (ORDER BY msg.Id ) AS tempLag41,
        LAG (msg.Temperature, 42) OVER (ORDER BY msg.Id ) AS tempLag42,
        LAG (msg.Temperature, 43) OVER (ORDER BY msg.Id ) AS tempLag43,
        LAG (msg.Temperature, 44) OVER (ORDER BY msg.Id ) AS tempLag44,
        LAG (msg.Temperature, 45) OVER (ORDER BY msg.Id ) AS tempLag45,
        LAG (msg.Temperature, 46) OVER (ORDER BY msg.Id ) AS tempLag46,
        LAG (msg.Temperature, 47) OVER (ORDER BY msg.Id ) AS tempLag47,
        LAG (msg.Temperature, 48) OVER (ORDER BY msg.Id ) AS tempLag48,
        LAG (msg.Temperature, 49) OVER (ORDER BY msg.Id ) AS tempLag49,
        LAG (msg.Temperature, 50) OVER (ORDER BY msg.Id ) AS tempLag50,
        LAG (msg.Temperature, 51) OVER (ORDER BY msg.Id ) AS tempLag51,
        LAG (msg.Temperature, 52) OVER (ORDER BY msg.Id ) AS tempLag52,
        LAG (msg.Temperature, 53) OVER (ORDER BY msg.Id ) AS tempLag53,
        LAG (msg.Temperature, 54) OVER (ORDER BY msg.Id ) AS tempLag54,
        LAG (msg.Temperature, 55) OVER (ORDER BY msg.Id ) AS tempLag55,
        LAG (msg.Temperature, 56) OVER (ORDER BY msg.Id ) AS tempLag56,
        LAG (msg.Temperature, 57) OVER (ORDER BY msg.Id ) AS tempLag57,
        LAG (msg.Temperature, 58) OVER (ORDER BY msg.Id ) AS tempLag58,
        LAG (msg.Temperature, 59) OVER (ORDER BY msg.Id ) AS tempLag59,
        LAG (msg.Temperature, 60) OVER (ORDER BY msg.Id ) AS tempLag60,
        LAG (msg.Temperature, 61) OVER (ORDER BY msg.Id ) AS tempLag61,
        LAG (msg.Temperature, 62) OVER (ORDER BY msg.Id ) AS tempLag62,
        LAG (msg.Temperature, 63) OVER (ORDER BY msg.Id ) AS tempLag63,
        LAG (msg.Temperature, 64) OVER (ORDER BY msg.Id ) AS tempLag64,
        LAG (msg.Temperature, 65) OVER (ORDER BY msg.Id ) AS tempLag65,
        LAG (msg.Temperature, 66) OVER (ORDER BY msg.Id ) AS tempLag66,
        LAG (msg.Temperature, 67) OVER (ORDER BY msg.Id ) AS tempLag67,
        LAG (msg.Temperature, 68) OVER (ORDER BY msg.Id ) AS tempLag68,
        LAG (msg.Temperature, 69) OVER (ORDER BY msg.Id ) AS tempLag69,
        LAG (msg.Temperature, 70) OVER (ORDER BY msg.Id ) AS tempLag70,
        LAG (msg.Temperature, 71) OVER (ORDER BY msg.Id ) AS tempLag71,
        LAG (msg.Temperature, 72) OVER (ORDER BY msg.Id ) AS tempLag72,
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
        ON STRFTIME('%Y-%m-%d %H:00:00', msg.Timestamp) = STRFTIME('%Y-%m-%d %H:00:00', currentMeteo.Timestamp)
    INNER JOIN meteodata AS back1Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', msg.Timestamp, '-1 hour') = STRFTIME('%Y-%m-%d %H:00:00', back1Hour.Timestamp)
    INNER JOIN meteodata AS back2Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', msg.Timestamp, '-2 hour') = STRFTIME('%Y-%m-%d %H:00:00', back2Hour.Timestamp)
    INNER JOIN meteodata AS back3Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', msg.Timestamp, '-3 hour') = STRFTIME('%Y-%m-%d %H:00:00', back3Hour.Timestamp)
    INNER JOIN meteodata AS back4Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', msg.Timestamp, '-4 hour') = STRFTIME('%Y-%m-%d %H:00:00', back4Hour.Timestamp)
    INNER JOIN meteodata AS back5Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', msg.Timestamp, '-5 hour') = STRFTIME('%Y-%m-%d %H:00:00', back5Hour.Timestamp)
    INNER JOIN meteodata AS back6Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', msg.Timestamp, '-6 hour') = STRFTIME('%Y-%m-%d %H:00:00', back6Hour.Timestamp)
    INNER JOIN meteodata AS nextHour
        ON STRFTIME('%Y-%m-%d %H:00:00', msg.Timestamp, '+1 hour') = STRFTIME('%Y-%m-%d %H:00:00', nextHour.Timestamp);

DROP VIEW IF EXISTS predictdata;

CREATE VIEW predictdata AS
    SELECT
        SIN(2 * PI() * CAST(STRFTIME("%m", datetime(current_timestamp, 'localtime')) AS REAL) / 12) AS month,
        SIN(2 * PI() * CAST(STRFTIME("%j", datetime(current_timestamp, 'localtime')) AS REAL) / 366) AS day,
        SIN(2 * PI() * CAST(STRFTIME("%H", datetime(current_timestamp, 'localtime')) AS REAL) / 24) AS hour,
        msg.Temperature AS tempLag1,
        LAG (msg.Temperature, 1) OVER (ORDER BY msg.Id ) AS tempLag2,
        LAG (msg.Temperature, 2) OVER (ORDER BY msg.Id ) AS tempLag3,
        LAG (msg.Temperature, 3) OVER (ORDER BY msg.Id ) AS tempLag4,
        LAG (msg.Temperature, 4) OVER (ORDER BY msg.Id ) AS tempLag5,
        LAG (msg.Temperature, 5) OVER (ORDER BY msg.Id ) AS tempLag6,
        LAG (msg.Temperature, 6) OVER (ORDER BY msg.Id ) AS tempLag7,
        LAG (msg.Temperature, 7) OVER (ORDER BY msg.Id ) AS tempLag8,
        LAG (msg.Temperature, 8) OVER (ORDER BY msg.Id ) AS tempLag9,
        LAG (msg.Temperature, 9) OVER (ORDER BY msg.Id ) AS tempLag10,
        LAG (msg.Temperature, 10) OVER (ORDER BY msg.Id ) AS tempLag11,
        LAG (msg.Temperature, 11) OVER (ORDER BY msg.Id ) AS tempLag12,
        LAG (msg.Temperature, 12) OVER (ORDER BY msg.Id ) AS tempLag13,
        LAG (msg.Temperature, 13) OVER (ORDER BY msg.Id ) AS tempLag14,
        LAG (msg.Temperature, 14) OVER (ORDER BY msg.Id ) AS tempLag15,
        LAG (msg.Temperature, 15) OVER (ORDER BY msg.Id ) AS tempLag16,
        LAG (msg.Temperature, 16) OVER (ORDER BY msg.Id ) AS tempLag17,
        LAG (msg.Temperature, 17) OVER (ORDER BY msg.Id ) AS tempLag18,
        LAG (msg.Temperature, 18) OVER (ORDER BY msg.Id ) AS tempLag19,
        LAG (msg.Temperature, 19) OVER (ORDER BY msg.Id ) AS tempLag20,
        LAG (msg.Temperature, 20) OVER (ORDER BY msg.Id ) AS tempLag21,
        LAG (msg.Temperature, 21) OVER (ORDER BY msg.Id ) AS tempLag22,
        LAG (msg.Temperature, 22) OVER (ORDER BY msg.Id ) AS tempLag23,
        LAG (msg.Temperature, 23) OVER (ORDER BY msg.Id ) AS tempLag24,
        LAG (msg.Temperature, 24) OVER (ORDER BY msg.Id ) AS tempLag25,
        LAG (msg.Temperature, 25) OVER (ORDER BY msg.Id ) AS tempLag26,
        LAG (msg.Temperature, 26) OVER (ORDER BY msg.Id ) AS tempLag27,
        LAG (msg.Temperature, 27) OVER (ORDER BY msg.Id ) AS tempLag28,
        LAG (msg.Temperature, 28) OVER (ORDER BY msg.Id ) AS tempLag29,
        LAG (msg.Temperature, 29) OVER (ORDER BY msg.Id ) AS tempLag30,
        LAG (msg.Temperature, 30) OVER (ORDER BY msg.Id ) AS tempLag31,
        LAG (msg.Temperature, 31) OVER (ORDER BY msg.Id ) AS tempLag32,
        LAG (msg.Temperature, 32) OVER (ORDER BY msg.Id ) AS tempLag33,
        LAG (msg.Temperature, 33) OVER (ORDER BY msg.Id ) AS tempLag34,
        LAG (msg.Temperature, 34) OVER (ORDER BY msg.Id ) AS tempLag35,
        LAG (msg.Temperature, 35) OVER (ORDER BY msg.Id ) AS tempLag36,
        LAG (msg.Temperature, 36) OVER (ORDER BY msg.Id ) AS tempLag37,
        LAG (msg.Temperature, 37) OVER (ORDER BY msg.Id ) AS tempLag38,
        LAG (msg.Temperature, 38) OVER (ORDER BY msg.Id ) AS tempLag39,
        LAG (msg.Temperature, 39) OVER (ORDER BY msg.Id ) AS tempLag40,
        LAG (msg.Temperature, 40) OVER (ORDER BY msg.Id ) AS tempLag41,
        LAG (msg.Temperature, 41) OVER (ORDER BY msg.Id ) AS tempLag42,
        LAG (msg.Temperature, 42) OVER (ORDER BY msg.Id ) AS tempLag43,
        LAG (msg.Temperature, 43) OVER (ORDER BY msg.Id ) AS tempLag44,
        LAG (msg.Temperature, 44) OVER (ORDER BY msg.Id ) AS tempLag45,
        LAG (msg.Temperature, 45) OVER (ORDER BY msg.Id ) AS tempLag46,
        LAG (msg.Temperature, 46) OVER (ORDER BY msg.Id ) AS tempLag47,
        LAG (msg.Temperature, 47) OVER (ORDER BY msg.Id ) AS tempLag48,
        LAG (msg.Temperature, 48) OVER (ORDER BY msg.Id ) AS tempLag49,
        LAG (msg.Temperature, 49) OVER (ORDER BY msg.Id ) AS tempLag50,
        LAG (msg.Temperature, 50) OVER (ORDER BY msg.Id ) AS tempLag51,
        LAG (msg.Temperature, 51) OVER (ORDER BY msg.Id ) AS tempLag52,
        LAG (msg.Temperature, 52) OVER (ORDER BY msg.Id ) AS tempLag53,
        LAG (msg.Temperature, 53) OVER (ORDER BY msg.Id ) AS tempLag54,
        LAG (msg.Temperature, 54) OVER (ORDER BY msg.Id ) AS tempLag55,
        LAG (msg.Temperature, 55) OVER (ORDER BY msg.Id ) AS tempLag56,
        LAG (msg.Temperature, 56) OVER (ORDER BY msg.Id ) AS tempLag57,
        LAG (msg.Temperature, 57) OVER (ORDER BY msg.Id ) AS tempLag58,
        LAG (msg.Temperature, 58) OVER (ORDER BY msg.Id ) AS tempLag59,
        LAG (msg.Temperature, 59) OVER (ORDER BY msg.Id ) AS tempLag60,
        LAG (msg.Temperature, 60) OVER (ORDER BY msg.Id ) AS tempLag61,
        LAG (msg.Temperature, 61) OVER (ORDER BY msg.Id ) AS tempLag62,
        LAG (msg.Temperature, 62) OVER (ORDER BY msg.Id ) AS tempLag63,
        LAG (msg.Temperature, 63) OVER (ORDER BY msg.Id ) AS tempLag64,
        LAG (msg.Temperature, 64) OVER (ORDER BY msg.Id ) AS tempLag65,
        LAG (msg.Temperature, 65) OVER (ORDER BY msg.Id ) AS tempLag66,
        LAG (msg.Temperature, 66) OVER (ORDER BY msg.Id ) AS tempLag67,
        LAG (msg.Temperature, 67) OVER (ORDER BY msg.Id ) AS tempLag68,
        LAG (msg.Temperature, 68) OVER (ORDER BY msg.Id ) AS tempLag69,
        LAG (msg.Temperature, 69) OVER (ORDER BY msg.Id ) AS tempLag70,
        LAG (msg.Temperature, 70) OVER (ORDER BY msg.Id ) AS tempLag71,
        LAG (msg.Temperature, 71) OVER (ORDER BY msg.Id ) AS tempLag72,
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
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime')) = STRFTIME('%Y-%m-%d %H:00:00', currentMeteo.Timestamp)
    INNER JOIN meteodata AS back1Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-1 hour') = STRFTIME('%Y-%m-%d %H:00:00', back1Hour.Timestamp)
    INNER JOIN meteodata AS back2Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-2 hour') = STRFTIME('%Y-%m-%d %H:00:00', back2Hour.Timestamp)
    INNER JOIN meteodata AS back3Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-3 hour') = STRFTIME('%Y-%m-%d %H:00:00', back3Hour.Timestamp)
    INNER JOIN meteodata AS back4Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-4 hour') = STRFTIME('%Y-%m-%d %H:00:00', back4Hour.Timestamp)
    INNER JOIN meteodata AS back5Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-5 hour') = STRFTIME('%Y-%m-%d %H:00:00', back5Hour.Timestamp)
    INNER JOIN meteodata AS back6Hour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '-6 hour') = STRFTIME('%Y-%m-%d %H:00:00', back6Hour.Timestamp)
    INNER JOIN meteodata AS nextHour
        ON STRFTIME('%Y-%m-%d %H:00:00', datetime(current_timestamp, 'localtime'), '+1 hour') = STRFTIME('%Y-%m-%d %H:00:00', nextHour.Timestamp)
   ORDER BY msg.Id DESC
   LIMIT 1;

DELETE FROM messages WHERE Temperature = -273.15;
