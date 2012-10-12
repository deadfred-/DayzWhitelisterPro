USE whitelist;

DELIMITER $$

DROP PROCEDURE IF EXISTS proc_AddWhiteListed$$
CREATE DEFINER = 'dayz'@'localhost'
PROCEDURE proc_AddWhiteListed(IN p_name  VARCHAR(255),
                              IN p_email VARCHAR(255),
                              IN p_GUID  VARCHAR(128)
                              )
BEGIN
  INSERT INTO whitelist.whitelist (`identifier`, `email`, `name`) VALUES (p_GUID, p_email, p_name);
END
$$

DELIMITER ;