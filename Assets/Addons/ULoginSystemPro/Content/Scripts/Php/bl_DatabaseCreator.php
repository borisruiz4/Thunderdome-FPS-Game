<?php
include("bl_Common.php");

$localhost    = $_POST['localhost'];
$databaseuser = $_POST['dbuser'];
$databasename = $_POST['dbname'];
$dbpassword   = $_POST['dbpassword'];

$name = strip_tags($_POST['name']);
$type = strip_tags($_POST['type']);

if($type <= 2){
$link = mysqli_connect($localhost, $databaseuser, $dbpassword, $databasename);
if ($link === false) {
    die("ERROR: Could not connect. " . mysqli_connect_error());
}
}else{
    $link = dbConnect();
}


if ($type == 0) {
    
    $sql = file_get_contents ('sql-tables.sql');
    
    if (mysqli_multi_query($link, $sql)) {
        echo 2;
    } else {
        die("ERROR: Could not able to execute. " . mysqli_error($link));
    }
    
} else if ($type == 1) {
    $url = "http" . (!empty($_SERVER['HTTPS']) ? "s" : "") . "://" . $_SERVER['SERVER_NAME'] . $_SERVER['REQUEST_URI'];
    echo $url;
} else if ($type == 2) {
    $codeid = 0;
    $link   = mysqli_connect($localhost, $databaseuser, $dbpassword, $databasename);
    if ($link) {
        $codeid = 1;
        if ($result = $link->query("SHOW TABLES LIKE 'MyGameDB'")) {
            if ($result->num_rows == 1) {
                $codeid = 2;
                $link2=TrydbConnect();
                if($link2){
                    $codeid = 3;
                    mysqli_close($link2);
                }
            }
        } else {
            $codeid = -2;
        }
    } else {
        $codeid = -1;
    }
    echo $codeid;
}
else if($type == 3)
{
    $query = "SHOW TABLES LIKE 'MyGameDB'";
    $result = mysqli_query($link,$query) or die(mysqli_error($link));
    $num = mysqli_num_rows($result);
    if($num >= 1)
    {
     echo "yes";
    }else{
        echo "no";
    }
}else if($type == 4)
{
    $sql = file_get_contents ('sql-tables.sql');   
    if (mysqli_multi_query($link, $sql) or die(mysqli_error($link))) {
        echo "done";
    }
}else if($type == 5)
{
    $sql = file_get_contents ('clan-sql.sql');   
    if (mysqli_multi_query($link, $sql) or die(mysqli_error($link))) {
        echo "done";
    }
}
 else {
    die('request id not defined');
}
mysqli_close($link);
?>