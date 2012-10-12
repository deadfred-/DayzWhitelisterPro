USE whitelist;

DELIMITER $$

DROP PROCEDURE IF EXISTS proc_SetWhitelistedStatus$$
CREATE DEFINER = 'root'@'localhost'
PROCEDURE proc_SetWhitelistedStatus(IN p_id          INT,
                                    IN p_whitelisted INT
                                    )
BEGIN
  UPDATE whitelist.whitelist
  SET
    whitelisted = p_whitelisted
  WHERE
    whitelist.id = p_id;
END
$$

DELIMITER ;