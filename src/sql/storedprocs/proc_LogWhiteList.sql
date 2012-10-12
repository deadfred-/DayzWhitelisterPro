USE whitelist;

DELIMITER $$

DROP PROCEDURE IF EXISTS proc_LogWhiteList$$
CREATE DEFINER = 'dayz'@'localhost'
PROCEDURE proc_LogWhiteList(IN p_name    VARCHAR(255),
                            IN p_GUID    VARCHAR(128),
                            IN p_logtype INT
                            )
BEGIN
  INSERT INTO whitelist.log (name, GUID, `timestamp`, logtype) VALUES (p_name, p_GUID, now(), p_logtype);
END
$$

DELIMITER ;