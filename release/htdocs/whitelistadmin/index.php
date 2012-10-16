<?php
/********************************************************************
*	Title:			Dayz Whitelister Pro
*	Author:			Hambeast 
*	Date:			2012-10-11
*	Description:	Admin control panel for Dayz!
*
*********************************************************************/


include("include/db.php");
?>

<html>
	<head>
		<LINK href="include/css/table.css" rel="stylesheet" type="text/css">
		<script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/1.8.2/jquery.min.js"></script>
		<script type="text/javascript" src="include/js/jquery.tablesorter.js"></script>
		<script type="text/javascript">
			$(document).ready(function()
				{
					$("#WhiteListed").tablesorter();
					$("#Log").tablesorter();
				}
			);
		</script>
		
		<meta http-equiv="cache-control" content="max-age=0" />
		<meta http-equiv="cache-control" content="no-cache" />
		<meta http-equiv="expires" content="0" />
		<meta http-equiv="expires" content="Tue, 01 Jan 1980 1:00:00 GMT" />
		<meta http-equiv="pragma" content="no-cache" />
	</head>
	<body>
		<div>
			<h1>Dayz Whitelist Tool by: Hambeast  Funding By: Big Tobacco</h1>
		</div>
		<?php
		// Did we get a valid request?
		if (ISSET($_POST['action']))
		{
			// Add WhiteList User
			if ($_POST['action'] == "Add")
			{
				$totalFields = 0;
			
				
				if (isset($_POST['name']))
				{
					$name = $_POST['name'];
					$totalFields++;
				}
				
				if (isset($_POST['email']))
				{
					$email = $_POST['email'];
					$totalFields++;
				}
				
				if (isset($_POST['identifier']))
				{
					$identifier = $_POST['identifier'];
					$totalFields++;
				}
				
				// check to see if we got all of our values
				if ($totalFields == 3)
				{
					// Open MySQL connection
					$mysqli = new MySQLI($host,$user,$pass,$db);
					
					// prepare our query
					$stmt = $mysqli->prepare("CALL proc_AddWhiteListed(?,?,?)");
					$stmt->bind_param("sss", $name, $email, $identifier);
					
					// execute statement -- Add whitelist users to our DB
					$stmt->execute();
					
					print "<div>you added $name $email $identifier</div>";
					
					// Close MySQL connection
					$mysqli = null;
					$stmt = null;
				}
				else
				{
					echo "<div>Error:Requiered Fields Missing</div>";
				}
			}
			// Remove Whitelist User
			if ($_POST['action'] == "On\\Off")
			{
				if (isset($_POST['id']))
				{
					$id = $_POST['id'];
					
					$whiteliststatus = $_POST['whiteliststatus'];
					if ($whiteliststatus == 0)
					{
						$status = 1;
					} 
					else 
					{
						$status = 0;
					}
					
					// Open MySQL connection
					$mysqli = new MySQLI($host,$user,$pass,$db);
					
					// prepare the statement
					$stmt = $mysqli->prepare("CALL proc_SetWhitelistedStatus(?,?)");
					$stmt->bind_param("ii", $id, $status);
					
					// execute statement
					$stmt->execute();
					
					// Close MySQL connection
					$mysqli = null;
					$stmt = null;
				}
			}
		}	
		?>
		<table id="WhiteListed" class="tablesorter">
		<thead>
		<tr>
			<th>Action</th>
			<th>Enabled</th>
			<th>Name</th>
			<th>Email</th>
			<th>GUID</th>
			
		</tr>
		</thead>
		<tbody>
			<form action="" method="POST">
			<tr>
				<td><input type="submit" name="action" value="Add"></td>
				<td></td>
				<td><input type="text" name="name"></td>
				<td><input type="text" name="email"></td>
				<td><input type="text" name="identifier"></td>
			</tr>
			</form>
			<?php
			// Open MySQL connection
			$mysqli = new MySQLI($host,$user,$pass,$db);
			$query = $mysqli->query("CALL proc_GetWhitelisted()");
			
			// populate our current whitelisted users table.  This includes option to remove user.
			while ($row = $query->fetch_array()){
			
				// write enabled html
				$wlStatus = $row['whitelisted'];
				
				if ($wlStatus == 1)
				{
					$statusImg = "Green_Button.png";

				}
				else
				{
					$statusImg = "Red_Button.png";
				}
						
				echo "<form action=\"\" method=\"POST\">";
				echo "<tr>";
				echo "<td><input type=\"hidden\" name=\"id\" value=\"" . $row['id'] . "\"><input type=\"submit\" name=\"action\" value=\"On\\Off\"></td>"; 			// id
				echo "<td><input type=\"hidden\" name=\"whiteliststatus\" value=\"" . $row['whitelisted'] . "\"><img src=\"include/images/" . $statusImg .  "\"></td>";	// enabled
				echo "<td>" . $row['name'] . "</td>"; 			// name
				echo "<td>" . $row['email'] . "</td>"; 			// email
				echo "<td>" . $row['identifier'] . "</td>"; 	// guid
				echo "</tr>";
				echo "";
				echo "</form>";
			}

			// close our MYSQL connection
			$mysqli = null;
			$query = null;
			?>
		</tbody>
		</table>
		<div>
		<form action="" method="POST">
			<input type="submit" name="action" value="GetLog">
		</form>
		</div>
		<div>
		<table id="Log" class="tablesorter">
			<thead>
			<tr>
				<th>ID</th>
				<th>Name</th>
				<th>GUID</th>
				<th>Timestamp</th>
				<th>Type</th>
			</tr>
			</thead>
		
		<tbody>
			<?php
			
			if (isset($_POST['action']))
			{
				if ($_POST['action'] == "GetLog")
				{
					// Open MySQL connection
					$mysqli = new MySQLI($host,$user,$pass,$db);
					$query = $mysqli->query("CALL proc_GetWhitelistLog()");
					
					// populate our current whitelisted users table.  This includes option to remove user.
					while ($row = $query->fetch_array()){
						
						echo "<tr>";
						echo "<td>" . $row['id'] . "</td>"; // id
						echo "<td>" . $row['name'] . "</td>"; 			// name
						echo "<td>" . $row['GUID'] . "</td>"; 			// email
						echo "<td>" . $row['timestamp'] . "</td>"; 	// guid
						echo "<td>" . $row['type'] . "</td>";
						echo "</tr>";
						echo "</form>";
					}
					// close our MYSQL connection
					$mysqli = null;
					$query = null;
				}	
			}
			?>
		</tbody>
		</div>
	</body>
</html>