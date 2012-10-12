USE whitelist;

DELIMITER $$

DROP PROCEDURE IF EXISTS proc_GetWhitelistLog$$
CREATE DEFINER = 'root'@'localhost'
PROCEDURE proc_GetWhitelistLog()
BEGIN
  SELECT log.id
       , log.name
       , log.GUID
       , log.`timestamp`
       , logtypes.description AS type

  FROM
    whitelist.log
  INNER JOIN whitelist.logtypes
  ON log.logtype = logtypes.id

  GROUP BY
    GUID
  ORDER BY
    log.id DESC
  ;
END
$$

DELIMITER ;