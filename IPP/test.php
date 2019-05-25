<?php
/**
 *  IPP 2nd part of project - file test.php
 *
 *  This script is usable for automatic testing of parse.php and interpret.py
 *
 *  Author: Zdenek Dolezal
 *  Login: xdolez82
 *  Faculty of information technology BUT
*/

const PARAM_ERROR = 10;
const FOPENR_ERROR = 11;
const FOPENW_ERROR = 12;
const EXIT_OK = 0;

class TestBench{
    private static $tests = array();
    private static $total = 0;
    private static $sucessful = 0;
    private static $parsePath;
    private static $interpretPath;


    //creates and adds new test to internal array, $srcPath is path to src file of test
    public static function addTest($srcPath){
        array_push(self::$tests, Test::create($srcPath, self::$total++));
    }
    public static function testParseAll(){
        foreach(self::$tests as $test){
            $test->parseTest(self::$parsePath);
            self::incSuccessful($test);
        }
    }
    public static function testInterpretAll(){
        foreach(self::$tests as $test){
            $test->interpretTest(self::$interpretPath);
            self::incSuccessful($test);
        }
    }
    public static function testAll(){
        foreach(self::$tests as $test){
            $test->completeTest(self::$parsePath, self::$interpretPath);
            self::incSuccessful($test);
        }
    }
    public static function printHTML(){
        self::printHTMLHeader();
        echo "<table style='width: 100%; border-spacing: 0px; padding-left: 10px;'>\n";
        foreach(self::$tests as $test){
            $test->printHTML();
        }
        echo "</table>\n";
        self::printHTMLEnd();
    }
    public static function setParsePath($path){
        self::$parsePath = $path;
    }
    public static function setInterpretPath($path){
        self::$interpretPath = $path;
    }
    private static function incSuccessful(Test $test){
        if ($test->wasSuccessful()){
            self::$sucessful++;
        }
    }
    private static function printHTMLHeader(){
        echo "<!DOCTYPE html>
<html lang='cs' dir='ltr'>
<head>
    <meta charset='UTF-8'>
    <title>Summary</title>
</head>
<script>
function toggleResults(){
    elems = document.getElementsByClassName('res1')
    for (var i = 0; i < elems.length; i++){
        elems[i].style.display = elems[i].style.display == '' ? 'none' : '';
    }
    btn = document.getElementById('toggle');
    if (btn.innerText === 'Show Failed'){
        btn.innerText = 'Show All';
    }else{
        btn.innerText = 'Show Failed';
    }
}
</script>
<body style='width: 100%; margin: 0px;'>
    <div style='text-align:center;'>
        <table style='width: 100%; border-bottom: solid 2px;'>
            <tr>
                <td style='text-align: left; float: left;'><h1 style='margin: 15px;'>TEST SUMMARY</h1></td>
                <td style='text-align: left; float: right;'><p style='margin: 15px;'><b>Author:</b> Zdeněk Doležal <br><b>Login:</b> xdolez82</td>
            </tr>
        </table>
        <br>
        <table style='width: 100%; border-spacing: 0px; table-layout: fixed; border-bottom: solid 2px; font-size: 20px;'>
            <tr>
                <td>TOTAL:</td>
                <td>PASSED:</td>
                <td>FAILED:</td>
            </tr>
            <tr style='height: 40px;'>
                <td style='background-color: #ffff99;'><b>".self::$total."</td>
                <td style='background-color: #99ff99;'><b>".self::$sucessful."</td>
                <td style='background-color: #ff6666;'><b>".(self::$total - self::$sucessful)."</td>
            </tr>
        </table>
    </div>
    <table style='position:sticky; top:0; width: 100%; background-color: #fff'><tr><td><h3>INDIVIDUAL TEST RESULTS</h3><td><button id='toggle'onClick='toggleResults()'style='background-color: #000000; color: #FFFFFF; padding: 10px; float:right;''>Show Failed</button></table>";

    }
    private static function printHTMLEnd(){
        echo "</body>\n";
        echo "</html>\n";
    }
}
class Test{
    private $id;                    //internal id of test
    private $name;                  //name of test (name of testfile)
    private $srcPath;               //path to src file
    private $rcPath;                //path to rc file
    private $outPath;               //path to out file
    private $inPath;                //path to in file
    private $returnCodeResult;
    private $outputComparisonResult;
    private $nonzeroReturnCodeExpected;

    /*
    Checks the src file and other files according to src file name. If the test
    files are incomplete function creates the missing ones and if the test is okay
    and complete, then it creates new object
    */
    public static function create($srcPath, $id){
        if (file_exists($srcPath)){
            $pathInfo = pathinfo($srcPath);
            $pathWOExt = $pathInfo["dirname"]."/".$pathInfo["filename"];
            $testName = $pathInfo["filename"];
            // Part where missing files are created
            if (!file_exists($pathWOExt.".rc")){
                $rcFile = fopen($pathWOExt.".rc", "w");
                if ($rcFile === false){
                    fwrite(STDERR, "Error: Could not create .rc file for test: $testName\n");
                    exit(FOPENW_ERROR);
                }
                fwrite($rcFile, "0\n");
                fclose($rcFile);
            }
            if (!file_exists($pathWOExt.".out")){
                $outFile = fopen($pathWOExt.".out", "w");
                if ($outFile === false){
                    fwrite(STDERR, "Error: Could not create .out file for test: $testName\n");
                    exit(FOPENW_ERROR);
                }
                fclose($outFile);
            }
            if (!file_exists($pathWOExt.".in")){
                $inFile = fopen($pathWOExt.".in", "w");
                if ($inFile === false){
                    fwrite(STDERR, "Error: Could not create .in file for test: $testName\n");
                    exit(FOPENW_ERROR);
                }
                fclose($inFile);
            }
            return new Test($testName, $pathWOExt, $id);
        }else{
            fwrite(STDERR, "Error: Test file $srcPath is not readable\n");
            exit(FOPENR_ERROR);

        }
    }
    // Assignement of object variables according to path where all files are located
    function __construct($name, $path, $id){
        $this->name = $name;
        $this->id = $id;
        $this->srcPath = $path.".src";
        $this->rcPath = $path.".rc";
        $this->outPath = $path.".out";
        $this->inPath = $path.".in";
        //storing result if returncode was correct and output was the same
        $this->nonzeroReturnCodeExpected = false;
        $this->returnCodeValue = 0;
        $this->returnCodeResult = false;
        $this->outputComparisonResult = false;

    }
    /*
    Executes test for parse script only, creates one temporery file which is deleted
    afterwards. Compares parse file output with expected output via jExam
    */
    public function parseTest($parsePath){
        $this->checkTestReadability();
        if (!file_exists($parsePath)){
            fwrite(STDERR, "Error: invalid parse file location: $parsePath\n");
            exit(FOPENR_ERROR);
        }

        $parseOutputFilePath = $this->createTmpFilePath();
        $parseOutputFile = $this->createTmpFile($parseOutputFilePath, "w");

        exec("php7.3 $parsePath < \"$this->srcPath\" > $parseOutputFilePath", $output, $retval);
        $this->setReturnCodeResult($retval);

        exec("java -jar /pub/courses/ipp/jexamxml/jexamxml.jar \"$this->outPath\" \"$parseOutputFilePath\"  /dev/null -D /pub/courses/ipp/jexamxml/options", $jexamOut, $jexamRetval);
        if (file_exists($this->outPath.".log")){
            unlink($this->outPath.".log");
        }
        $this->setOutputResult($jexamRetval);
        fclose($parseOutputFile);
        unlink($parseOutputFilePath);
    }
    /*
    Executes test for interpret only, creates temporary files to store result and
    uses shell utillity diff to compare output with expected output
    */
    public function interpretTest($interpretPath){
        $this->checkTestReadability();
        if (!file_exists($interpretPath)){
            fwrite(STDERR, "Error: invalid interpret file location: $interpretPath\n");
            exit(FOPENR_ERROR);
        }
        $interpretOutputFilePath = $this->createTmpFilePath();
        $interpretOutputFile = $this->createTmpFile($interpretOutputFilePath, "w");

        exec("python3.6 \"$interpretPath\" --input=\"$this->inPath\" --source=\"$this->srcPath\" > \"$interpretOutputFilePath\"", $output, $retval);
        $this->setReturnCodeResult($retval);

        exec("diff \"$interpretOutputFilePath\" \"$this->outPath\"", $diffOut, $diffRetval);
        $this->setOutputResult($diffRetval);
        fclose($interpretOutputFile);
        unlink($interpretOutputFilePath);
    }
    /*
    Executes complete test: Runs parse.php, gets it result and runs interpret with it and with specified
    input by .in file, then compares return values and output
    */
    public function completeTest($parsePath, $interpretPath){
        $this->checkTestReadability();
        if (!file_exists($parsePath) || !file_exists($interpretPath)){
            fwrite(STDERR, "Error: invalid file location: $parsePath or $interpretPath\n");
            exit(FOPENR_ERROR);
        }
        $parseOutputFilePath = $this->createTmpFilePath();
        $parseOutputFile = $this->createTmpFile($parseOutputFilePath, "w");

        exec("php7.3 \"$parsePath\" < \"$this->srcPath\" > \"$parseOutputFilePath\"", $parseOutput, $parseRetval);
        $this->setReturnCodeResult($parseRetval);

        if ($parseRetval === 0){
            $interpretOutputFilePath = $this->createTmpFilePath();
            $interpretOutputFile = $this->createTmpFile($interpretOutputFilePath, "w");
            exec("python3.6 $interpretPath --input=\"$this->inPath\" --source=\"$parseOutputFilePath\" > $interpretOutputFilePath", $intOutput, $intRetval);
            $this->setReturnCodeResult($intRetval);
            exec("diff \"$this->outPath\" \"$interpretOutputFilePath\"", $diffOut, $diffRetval);
            $this->setOutputResult($diffRetval);
            fclose($interpretOutputFile);
            unlink($interpretOutputFilePath);
        }
        fclose($parseOutputFile);
        unlink($parseOutputFilePath);
    }

    public function wasSuccessful(){
        return ($this->outputComparisonResult !== false && $this->returnCodeResult !== false)
                || ($this->returnCodeResult !== false && $this->nonzeroReturnCodeExpected === true);

    }
    public function printHTML(){
        echo "<tr class='res".$this->wasSuccessful()."'><td>\n";
        echo "<div style='font-size:17px; float: left;'>\n";
        echo "<b>Test $this->id: </b>$this->name: <span style='color: ".$this->getResultColor($this->wasSuccessful()).";'><b>".$this->combinedResultToString()."</b></span>\n";
        echo "</div>";
        echo "<div id='row$this->id","exp' style='padding-top: 5px;'>\n<br>";
        echo "<b>├Path:</b> $this->srcPath<br>\n";
        echo "<b>├Output comparison:</b> <span style='color:".$this->getResultColor($this->outputComparisonResult).";'>"
        .$this->outputCompResultToString($this->outputComparisonResult)."</span><br>\n";
        echo "<b>└Return comparison:</b> <span style='color:".$this->getResultColor($this->returnCodeResult).";'>"
        .$this->rcCompResultToString($this->returnCodeResult)."</span> ".$this->getExpectedReturnCode()."/$this->returnCodeValue\n<br>";
        echo "</div></div>\n";
        echo "</tr></td>\n";
    }
    private function checkTestReadability(){
        if (!is_readable($this->srcPath)){
            fwrite(STDERR, "Error: Unreadable src file: $this->srcPath\n");
            exit(FOPENR_ERROR);
        }
        if (!is_readable($this->outPath)){
            fwrite(STDERR, "Error: Unreadable out file: $this->outFile\n");
            exit(FOPENR_ERROR);
        }
        if (!is_readable($this->inPath)){
            fwrite(STDERR, "Error: Unreadable in file: $this->inFile\n");
            exit(FOPENR_ERROR);
        }


    }
    private function setOutputResult($out){
        if (intval($out) === 0){
            $this->outputComparisonResult = true;
        }else{
            $this->outputComparisonResult = false;
        }
    }
    private function setReturnCodeResult($rc){
        $this->returnCodeValue = $rc;
        //dont overwrite
        //if ($this->returnCodeResult === false) return;
        if (intval($rc) === $this->getExpectedReturnCode()){
            $this->returnCodeResult = true;
        }else{
            $this->returnCodeResult = false;
        }
    }
    private function createTmpFile($path, $mode){
        $file = fopen($path, $mode);
        if ($file === false){
            fwrite(STDERR, "Error: Couldn't create temporary file\n");
            exit(FOPENW_ERROR);
        }
        return $file;
    }
    private function createTmpFilePath(){
        $filePath = "./test.tmp";
        while (file_exists($filePath)){
            $filePath = "./test".rand(0, 999999).".tmp";
        }
        return $filePath;
    }
    private function getExpectedReturnCode(){
        $rcFile = fopen($this->rcPath, "r");
        if ($rcFile === false){
            fwrite(STDERR, "Error: cannot open file: $this->rcPath\n");
            exit(FOPENR_ERROR);
        }
        $rc = intval(fgets($rcFile));
        if ($rc !== 0) $this->nonzeroReturnCodeExpected = true;
        fclose($rcFile);
        return $rc;
    }
    private function getResultColor($result){
        return ($result === true) ? "#00ff00" : "#ff6666";
    }
    private function outputCompResultToString(){
        if ($this->nonzeroReturnCodeExpected){
            return "NOT TESTED (Error code expected!)";
        }else{
            return ($this->outputComparisonResult) ? "OK" : "FAIL";
        }
    }
    private function rcCompResultToString(){
        return ($this->returnCodeResult) ? "OK" : "FAIL";
    }
    private function combinedResultToString(){
        return ($this->wasSuccessful()) ? "OK" : "FAIL";
    }
}
function printHelp(){
    echo "HELP for script test.php\n";
    echo "---------------------------------------------------------\n";
    echo "Script serves as automatic test bench for parse.php and \n";
    echo "interpret.py scripts. Script goes through given directory\n";
    echo "and uses its contents to automaticly test functionality of\n";
    echo "both programs and generates HTML summary to standard output.\n";
    echo "Without directory parameter actual directory is used\n";
    echo "\n";
    echo "Script parameters:\n";
    echo "  --help                  displays help (this)\n";
    echo "  --directory='path'      tries to find tests in 'path' directory\n";
    echo "  --recursive             finds test in given directory recursively\n";
    echo "  --parse-script='file'   explicitly specified parse.php file\n";
    echo "  --int-script='file'     explicitly specified interpret.py file\n";
    echo "  --parse-only            testing only parse.php\n";
    echo "  --int-only              testing only interpret.py\n";
    echo "---------------------------------------------------------\n";
}

function recursiveSearch($path, &$files){
    $items = scandir($path);
    foreach($items as $item){
        if (is_dir("$path/$item")){
            if ($item !== '.' && $item !== '..'){
                recursiveSearch("$path/$item", $files);
            }
        }else{
            array_push($files, "$path/$item");
        }

    }
}
function filterSrc($files){
    $filteredFiles = array();
    foreach($files as $file){
        $pathInfo = pathinfo($file);
        if (array_key_exists("extension", $pathInfo)){
            if ($pathInfo["extension"] == "src"){
                array_push($filteredFiles, $file);
            }
        }
    }
    return $filteredFiles;
}

/* Main */
$longOpts = ["help","directory:","recursive","parse-script:","int-script:", "parse-only", "int-only"];
$params = getopt("", $longOpts);
if (($argc > 1 && sizeof($params) === 0) || $params === false){
    fwrite(STDERR, "Error: Invalid arguments\n");
    exit(PARAM_ERROR);
}
$path = realpath(".");
$parsePath = "./parse.php";
$interpretPath = "./interpret.py";
$files = array();

if (array_key_exists("help", $params)){
    if ($argc === 2){
        printHelp();
        exit(EXIT_OK);
    }else{
        fwrite(STDERR, "Error: Help is not usable with other parameters\n");
        exit(PARAM_ERROR);
    }
}
// setting the path variable
if (array_key_exists("directory", $params)){
    if (is_dir($params["directory"])){
        $path = realpath($params["directory"]);
    }else{
        fwrite(STDERR, "Error: Wrong path given to parameter directory\n");
        exit(PARAM_ERROR);
    }
}
// parse.php file entered explicitly
if (array_key_exists("parse-script", $params)){
    if (array_key_exists("int-only", $params)){
        fwrite(STDERR, "Error: Parameter --parse-script='file' isn't usable with --int-only\n");
        exit(PARAM_ERROR);
    }else{
        if (is_file($params["parse-script"])){
            $parsePath = realpath($params["parse-script"]);
        }else{
            fwrite(STDERR, "Error: File given to parameter --parse-script doesnt exist\n");
            exit(PARAM_ERROR);
        }
    }
}
// interpret.py file entered explicitly
if (array_key_exists("int-script", $params)){
    if (array_key_exists("parse-only", $params)){
        fwrite(STDERR, "Error: Parameter --int-script='file' isn't usable with --parse-only\n");
        exit(PARAM_ERROR);
    }else{
        if (is_file($params["int-script"])){
            $interpretPath = realpath($params["int-script"]);
        }else{
            fwrite(STDERR, "Error: File given to parameter --int-script doesnt exist\n");
            exit(PARAM_ERROR);
        }
    }
}
// recursive search in specified folder
if (array_key_exists("recursive", $params)){
    recursiveSearch($path, $files);
}else{
    // search in specified folder
    $items = scandir($path);
    foreach($items as $item){
        if (is_file("$path/$item")){
            array_push($files, "$path/$item");
        }
    }
}
TestBench::setParsePath($parsePath);
TestBench::setInterpretPath($interpretPath);
foreach(filterSrc($files) as $srcFile){
    TestBench::addTest($srcFile);
}
if (array_key_exists("parse-only", $params)){
    TestBench::testParseAll();
}else if (array_key_exists("int-only", $params)){
    TestBench::testInterpretAll();
}else{
    TestBench::testAll();
}
TestBench::printHTML();
exit(EXIT_OK);
?>
