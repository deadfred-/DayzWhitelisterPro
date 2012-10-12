-- Create whitelist databse

DROP DATABASE IF EXISTS whitelist;
CREATE DATABASE IF NOT EXISTS whitelist
CHARACTER SET latin1
COLLATE latin1_swedish_ci;

-- Grant access to Dayz Users

GRANT ALL PRIVILEGES ON whitelist.* TO 'dayz'@'localhost';

-- Generate our Whitelist log table

USE whitelist;
DROP TABLE IF EXISTS log;
CREATE TABLE IF NOT EXISTS log(
  id INT(11) NOT NULL AUTO_INCREMENT,
  name VARCHAR(255) NOT NULL,
  GUID VARCHAR(32) NOT NULL,
  `timestamp` DATETIME NOT NULL,
  logtype INT(11) UNSIGNED NOT NULL,
  PRIMARY KEY (id)
)
ENGINE = INNODB
AUTO_INCREMENT = 227
AVG_ROW_LENGTH = 246
CHARACTER SET latin1
COLLATE latin1_swedish_ci;

-- Generate our Whitelist logtypes table
USE whitelist;
DROP TABLE IF EXISTS logtypes;
CREATE TABLE IF NOT EXISTS logtypes(
  id INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
  description VARCHAR(255) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE INDEX description (description)
)
ENGINE = INNODB
AUTO_INCREMENT = 3
AVG_ROW_LENGTH = 8192
CHARACTER SET latin1
COLLATE latin1_swedish_ci;

-- Insert types into whitelist types
INSERT INTO whitelist.logtypes (description) VALUES ('Authorized Login');
INSERT INTO whitelist.logtypes (description) VALUES ('Kicked');

-- Generate our Whitelist master table
USE whitelist;
DROP TABLE IF EXISTS whitelist;
CREATE TABLE IF NOT EXISTS whitelist(
  id INT(11) NOT NULL AUTO_INCREMENT,
  identifier VARCHAR(255) NOT NULL COMMENT 'guid or IP',
  email VARCHAR(255) DEFAULT NULL,
  name VARCHAR(255) NOT NULL,
  whitelisted INT(1) UNSIGNED NOT NULL DEFAULT 1,
  PRIMARY KEY (id)
)
ENGINE = INNODB
AUTO_INCREMENT = 13
AVG_ROW_LENGTH = 4096
CHARACTER SET latin1
COLLATE latin1_swedish_ci;