USE whitelist;

DELIMITER $$

DROP PROCEDURE IF EXISTS proc_CheckWhiteList$$
CREATE DEFINER = 'dayz'@'localhost'
PROCEDURE proc_CheckWhiteList(IN p_guid VARCHAR(32))
BEGIN
  SELECT *
  FROM
    whitelist
  WHERE
    whitelist.whitelist.identifier = p_guid
    AND whitelist.whitelist.whitelisted = 1;
END
$$

DELIMITER ;