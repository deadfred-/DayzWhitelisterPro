USE whitelist;

DELIMITER $$

DROP PROCEDURE IF EXISTS proc_GetWhitelisted$$
CREATE DEFINER = 'root'@'localhost'
PROCEDURE proc_GetWhitelisted()
BEGIN
  SELECT *
  FROM
    whitelist.whitelist;
END
$$

DELIMITER ;