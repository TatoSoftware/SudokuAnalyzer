// Demonstrates Javascript and Active Server Pages working together. 
// In general the menu items are processed on the server in C Sharp and the user interface activity is handled in
// Javascript.  Note the trickly logic to communicate the user interface changes with server code.
// 
// =================GLOBAL JAVASCRIPT CONSTANTS (all caps) ========================================
var gCOLOR_SELECTED = '#A4D5D8';      // inner color of p Key, very light blue #92CDD0
var gCOLOR_BG_DEFAULT = '#FFFFFF';   // white
var gCOLOR_BG_FLASH = '#E6F3F4';    //blue center of menu keys 3 shades lighter Than old value of 80C4C8
var gCOLOR_ERROR_CELLS = '#FF6347';      // Tomato
var gTEXTBOXCHARS = 13;     // total number of characters in a SK cell - ALWAYS
//
// ===================== GLOBAL VARIABLES ==================================================== 
var gColor_Saved = '#FFFFFF';   // white default start
var gLastIDClicked = "";      // id of last cell clicked eg tbASPr3c2
var gEventCount = 0;
var gFlashList = new Array();      // list of cells ID'S that have been flashed
var gFlashScalars = "";             // value set at load
var gFlashMulti = "";               // value set at load
var gInteralDuration = 5 * 60 * 1000;        // 5 minutes recommend by someone session timeout  
var myTimerVar = setInterval(function () { myTimer() }, gInteralDuration);   //used to keep godaddy server from timing out
// define solution sets. Every puzzle has 27 solution sets, 9 rows, 9 columns and 9 boxes
// when solved every solution set must contain all the numbers 1 to 9 with no duplicates.
// cells numbered 0 to 80 top left to bottom right
// the cell number is referred to CellNmbr
var gSS = [[0,1,2,3,4,5,6,7,8],   //ss0 row 0
        [9, 10, 11, 12, 13, 14, 15, 16, 17], //ss1 row 1
        [18, 19, 20, 21, 22, 23, 24, 25, 26],   //ss2 row 2
        [27, 28, 29, 30, 31, 32, 33, 34, 35],   //ss3 row 3
        [36, 37, 38, 39, 40, 41, 42, 43, 44],   //ss4 row 
        [45, 46, 47, 48, 49, 50, 51, 52, 53],   //ss5 row 5
        [54, 55, 56, 57, 58, 59, 60, 61, 62],   //ss6 row 6
        [63, 64, 65, 66, 67, 68, 69, 70, 71],   //ss7 row 7
        [72, 73, 74, 75, 76, 77, 78, 79, 80],   //ss8 row 8
        [0, 9, 18, 27, 36, 45, 54, 63, 72], //ss9 col 0
        [1, 10, 19, 28, 37, 46, 55, 64, 73],    //ss10 col 1
        [2, 11, 20, 29, 38, 47, 56, 65, 74],    //ss11 col 2
        [3, 12, 21, 30, 39, 48, 57, 66, 75],    //ss12 col 3
        [4, 13, 22, 31, 40, 49, 58, 67, 76],    //ss13 col4
        [5, 14, 23, 32, 41, 50, 59, 68, 77],    //ss14 col 5
        [6, 15, 24, 33, 42, 51, 60, 69, 78],    //ss15 col 6
        [7, 16, 25, 34, 43, 52, 61, 70, 79],    //ss16 col 7
        [8, 17, 26, 35, 44, 53, 62, 71, 80],     //ss17 col 8
        [0, 1, 2, 9, 10, 11, 18, 19, 20],   // ss18 box 0
        [3, 4, 5, 12, 13, 14, 21, 22, 23],    //ss19 box 1  
        [6, 7, 8, 15, 16, 17, 24, 25, 26],      //ss20 box 3
        [27, 28, 29, 36, 37, 38, 45, 46, 47],   //ss21 box 4
        [30, 31, 32, 39, 40, 41, 48, 49, 50],   //ss22 box 5
        [33, 34, 35, 42, 43, 44, 51, 52, 53],   //ss23 box 6
        [54, 55, 56, 63, 64, 65, 72, 73, 74],   //ss24 box 7
        [57, 58, 59, 66, 67, 68, 75, 76, 77],   //ss25 box 8
        [60, 61, 62, 69, 70, 71, 78, 79, 80]];         //ss26 box 9

    var gaErrList_P = new Array();
    var gaErrList_SS = new Array();
    var gTotalScalarsFound = 0;
// body level events
function myjsHourglass() {
    // triggered via body tag in site.master
    // don't put event handler in code as body style may not be defined when the code is executed
    document.body.style.cursor = 'wait';
}
function myjsBodyLoad() {
    ReadCookies();          //varlid for session only (more work here)
}
// =================================== PROCESS P KEYS 1...9 ==========================

function OnClickEnter(varSourceID) {
    // enter in button is pressed  looks ok
    gEventCount++;      // count all events for stats
    //  Make sure a SK cell is currently active / has been clicked
    if (gLastIDClicked == "") { return; }
    document.getElementById(gLastIDClicked).style.backgroundColor = gCOLOR_SELECTED;    //could already be red
    PlaySound("clickonly");
    // 
    var sKeyID = new String(varSourceID);
    // possible values are Yes - entry is part of the initial puzzle so don't change it
    //attribute set when building case
    var StartValue = document.getElementById(gLastIDClicked).getAttribute("StartValue");
    if (StartValue == "Yes") { return; }

    // A SK cell is currently active
    // Now determine which Enter Key was clicked
    var idScan = sKeyID.search("buJS");     //eg loc in id of [extra asp sfuff ..buJP3] zero based
    var idPos = idScan + 4;                 // intposition of key id eg 3
    var sEnterKeyValue = sKeyID.substr(idPos, 1);    // value of key clicked
    // idKey contains the new value to be entered into the cell.  If it is wrong indicate so
    // gLastIdChecked is the cell to be changed tbASPr1c7
    // lookup the solution value passed from the server side and compare it to the value of the key pressed
    if (bCheckSolution(gLastIDClicked, sEnterKeyValue) == false)
        document.getElementById(gLastIDClicked).style.backgroundColor = gCOLOR_ERROR_CELLS;

    UpdateTargetCell(sEnterKeyValue);
    UpdateKeyDisplay(sEnterKeyValue);
    // final check generate well done message if puzzle solved
    PreEdit();
}
function OnClickPencil(varSourceID) {
    // fired pencil in button is pressed
    // Key means command key eg  1 2 3 4 5 6 7 8  9 or X
    // Cell means the contents of a sudoku cell
    // ID means the variable contains the ID of a control
    // a cell must always contain 123 456789  no commas, either a number or space
    //     the 9 must always be in col 8 etc.

    var vKeyMap = new Array();      // map command keys to candidates in text boxes
    vKeyMap[0] = 1;  //set/reset 1 candidate, found a intpositon 1 in textbox
    vKeyMap[1] = 2;  // candidate 2
    vKeyMap[2] = 3;  // candidate 3
    vKeyMap[3] = 6;  // candidate 4
    vKeyMap[4] = 7;  // candidate 5
    vKeyMap[5] = 8;  // candidate 6
    vKeyMap[6] = 11;  // candidate 7
    vKeyMap[7] = 12;  // candidate 8
    vKeyMap[8] = 13;  // candidate 9 found in intpos 13 in textbox
    vLASTKEY = 8;       // last index in above array

    gEventCount++;      // count all events for stats

    //  Make sure a SK cell is currently active / has been clicked
    if (gLastIDClicked == "") { return; }
    // possible values are Yes - entry is part of the initial puzzle so don't change it
    //attribute set when building case
    var StartValue = document.getElementById(gLastIDClicked).getAttribute("StartValue");
    if (StartValue == "Yes") { return; }

    PlaySound("clickonly");
    // 
    var sKeyID = new String(varSourceID);
    // A SK cell is currently active
    // Now determine which pencil Key was clicked
    var idScan = sKeyID.search("buJP");     //eg loc in id of [extra asp sfuff ..buJP3] zero based
    var idPos = idScan + 4;                 // intposition of key id eg 3
    var idKey = sKeyID.substr(idPos, 1);    // value of key clicked

    var CandidatePtr = vKeyMap[idKey - 1];
    var SKCell = document.getElementById(gLastIDClicked).value;
    //
    // so idKey is now the key number 
    // now extract the contents of the SK cell we want to update
    if (SKCell.length == 1) {
        // cell contents is a scalar, not the standard string, re-create the standard string
        var PValue = SKCell;
        PValueLoc = vKeyMap[PValue - 1];
        // it appears the unicode character 10 (LF)  occupies 2 bytes, go figure
        var NoPValues = "    " + String.fromCharCode(10) + "    " + String.fromCharCode(10) + "    ";
        var strLen = NoPValues.length;
        // stuff pvalue into the string at location pvalueloc
        SKCell = NoPValues.substr(0, PValueLoc) + PValue + NoPValues.substr(PValueLoc + 1, strLen - 1);
    } // end of scalar logic

    var SKCellArray = SKCell.split("");
    //   eg the selected cell contains  
    //    123456789 and the key clicked is 5 return 1234 6789
    //    1234 6789 and the key clicked is 5 return 123456789
    if (idKey == "X") {
        // The X key indicates cell needs to be cleared
        for (i = 0; i <= vLASTKEY; i++) {
            SKCellArray[vKeyMap[i]] = " ";   //don't wipe out the carriage returns
        }
    }
    else {
        // toggle the value in the specified intposition
        if (SKCellArray[CandidatePtr] == " ") {
            SKCellArray[CandidatePtr] = idKey;
        }
        else {
            SKCellArray[CandidatePtr] = " ";
        }
    } // idkey test
    // update target cell with altered array, this is where the state of the cell is stored.
    var newSKCell = "";
    // just convert array back to a string
    for (i = 0; i <= gTEXTBOXCHARS; i++) {
        newSKCell = newSKCell + SKCellArray[i];  /* only 11 elements in sk cell array, not 13 therefore undefined */
    }
    UpdateTargetCell(newSKCell);
    UpdateKeyDisplay(newSKCell);
}

function OnClickSKCell(varSourceID) {
    // fired when a sudoku input cell is clicked on
    // record id of the last sk cell that was clicked on
    gEventCount++;      // count all events for stats
    ShowButtons();   //show number buttons
    var sCellID = new String(varSourceID);      //eg tbASPr0c1
    // more robust code than before, leave error cells in red
    if (document.getElementById(gLastIDClicked) != null) {
        var ColorLastClicked = document.getElementById(gLastIDClicked).style.backgroundColor;
        if (document.getElementById(gLastIDClicked).style.backgroundColor != gCOLOR_ERROR_CELLS)
            document.getElementById(gLastIDClicked).style.backgroundColor = gCOLOR_BG_DEFAULT;
    }
    //
    document.getElementById(sCellID).style.backgroundColor = gCOLOR_SELECTED;  
    gLastIDClicked = sCellID;

    UpdateKeyDisplay(document.getElementById(sCellID).value);
    //
    // new feature highlight all cells with the same scalar value
    //
    StopFlashing(sCellID);     //clear any previous flashing, except in the current cell
    // this option controlled by the Options Menu
    ReadCookies();  // user options may have changed
    if (CountCandidates(sCellID) == 1) {
        if (gFlashScalars == "Y") FlashScalars(sCellID);  // flash all scalars of the same value
    }
    else  {
        if (gFlashMulti == "Y") FlashMulti(sCellID);  // flash all scalars of the same value
    }
    // make sure there are no conflicts introducted
    PreEdit();
}
function OnClickPanel(varSourceID) {
    // click anywhere in the panel and remove all highlighting from the grid, should help useability
    gEventCount++;      // count all events for stats
    HideButtons(); //hide number buttons
    for (r = 0; r <= 8; r++) {
        for (c = 0; c <= 8; c++) {
            sCellID = "tbASPr" + r + "c" + c;        
            document.getElementById(sCellID).style.backgroundColor = gCOLOR_BG_DEFAULT;
        } // next col
    }  //next row

}
function FlashScalars(vCellID) {
    // only call if current cell is a scalar
    // find all other cells with the same scalar and 
    // highlight them by turning the background a specific color (called flashing in the code)
    // new rule, do not touch currently selected cell
    var sCellID = "";
    var bFound = false;
    //gFlashList.length = 0;     //list of cells that are to be flashed / unflashed
    //if (CountCandidates(vCellID) != 1) return;      //only function if the currently clicked cell is a scalar
    var rowID = Number(vCellID.substr(6, 1));      //id of current cell selected
    var colID = Number(vCellID.substr(8, 1));
    var sPTarget = GetScalarValue(vCellID);
    // build the list of cells to flash
    for (r = 0; r <= 8; r++) {
        for (c = 0; c <= 8; c++) {
            sCellID = "tbASPr" + r + "c" + c;
            if(isTargetInThisCell(sCellID, sPTarget) ){     //v12 works - All cells with same p value flashing
                MultiAdd(rowID, colID, r, c);
            }
        } // next col
    }  //next row
    // only flash if more than one scalar was found, list will have have min of one entry even if only one scalar
    if (gFlashList.length == 0) return;
    
    var iLastEntry = gFlashList.length - 1;
    for (i = 0; i <= iLastEntry; i++) {
        // set background color to all cells in the list
        sCellID = gFlashList[i];
        document.getElementById(sCellID).style.backgroundColor = gCOLOR_BG_FLASH;
    }
    // the background will be reset on the next click...
}
function FlashMulti(vCellID) {
    //
    // if the current cell selected contains 2-9 candidates highlight all the candidates in the 
    // same row, column or region
    // highlight them by turning the background a specific color (called flashing in the code)
    // build the list of cells to act upon
    //
    // used to lookup the definition of the box that the currently clicked cell is in
    // index via either the row or column number to get the top and bottom of the row or col in the box
    // eg the cell in r1,c1 is in the top left box r0,c0 to r2,c2
    var vBOXFROM = new Array(0, 0, 0, 3, 3, 3, 6, 6, 6);
    var vBOXTO = new Array(2, 2, 2, 5, 5, 5, 8, 8, 8);

    var sCellID = "";
    gFlashList.length = 0;     //list of cells that are to be flashed / unflashed
    var iCandidateCnt =CountCandidates(vCellID);    
    //if (iCandidateCnt < 2) return;      //only for cells with multiple values
    // typical cell id is of the form tbASPr3c8  zerr based
    if (vCellID.length != 9) return;                            //must be of this form
    var rowID = Number(vCellID.substr(6, 1));
    var colID = Number(vCellID.substr(8, 1));
    // list all cells in the current row
    for (c = 0; c <= 8; c++) {
        MultiAdd(rowID, colID, rowID, c);
    }
    // list all cells in the current column
    for (r = 0; r <= 8; r++) {
        MultiAdd(rowID, colID, r, colID);
    }
    // highlight the box as well
    var iColFr = vBOXFROM[colID];
    var iColTo = vBOXTO[colID];
    var iRowFr = vBOXFROM[rowID];
    var iRowTo = vBOXTO[rowID];
    for (r = iRowFr; r <= iRowTo; r++) {
        for (c = iColFr; c <= iColTo; c++) {
            MultiAdd(rowID, colID, r, c);
        }
    }
    // only flash if more than one entry was found
    if (gFlashList.length == 0) return;
    var iLastEntry = gFlashList.length -1 ;

    for (i = 0; i <= iLastEntry; i++) {
        // set background color to all cells in the list
        sCellID = gFlashList[i];
        document.getElementById(sCellID).style.backgroundColor = gCOLOR_BG_FLASH;
    }
    // the background will be reset on the next click...
}
function MultiAdd(rowID, colID, r, c) {
    // add to the list everything except the current cell, it is too hard to see when the others
    // are highlighted 
    if (rowID == r && colID == c) return;
    iNewEntry = gFlashList.length;
    gFlashList[iNewEntry] = "tbASPr" + r + "c" + c;
}
function StopFlashing(vCurrCellID)
    // backgrounds are now flashing, just turn them off
    // flashing means the background color has changed to highlight the cell
    {
    var i = 0;
        var sCellID = "";
        if (gFlashList.length == 0) return;
        // return scalar background to the default
        for (i = 0; i <= gFlashList.length - 1; i++) {
            // set background color to all cells in the list
            sCellID = gFlashList[i];
            if (sCellID != vCurrCellID) {    //current cell needs to be highlighed, don't revert it
             document.getElementById(sCellID).style.backgroundColor = gCOLOR_BG_DEFAULT;
            }
            gFlashList[i] = null;
        }
    // zap the (old) list of cells to highlight
        gFlashList.length = 0;
    }
// ------------------------------------------------------------------------------------
function PlaySound(sType) {
    // clicks on tablets but not on devices with mouse...
    // returns true if user is using one of the following mobile browsers
    // for help with the expression see http://www.w3schools.com/js/js_obj_regexp.asp
    var isMobile = navigator.userAgent.match(/(iPad)|(iPhone)|(iPod)|(android)|(webOS)/i);
    if (isMobile) {
        document.getElementById('au1').play();
    }
}
function UpdateKeyDisplay(newSKCell) {
    // Change the appearance of the PENCIL keys depending on what values are in the
    // currently pencilled into the selected cell
    // key id is buJS1 ... buJS9 .. buJSX
    var sPValuesOn = " p values set are ";
    if (newSKCell == "") { return; }       //should not happen  
    if (newSKCell == null) { return; }       //should not happen
    for (i = 1; i <= 9; i++) {
        if (newSKCell.search(i) == -1) {
            document.getElementById("buJP" + i).style.opacity = "0.3";  //dim the image
        }
        else {
            document.getElementById("buJP" + i).style.opacity = "1.0";  //show number is a candidate
            sPValuesOn = sPValuesOn + i + ",";
        }
    }
}

// ----------------------------------------------------------------------------------------------
function UpdateTargetCell(newSKCell) {
    // 
    document.getElementById(gLastIDClicked).value = newSKCell;
    // by defn any cell with more than one candidate or no candidates is not an error cell
    if (CountCandidates(gLastIDClicked) != 1) {
        document.getElementById(gLastIDClicked).style.backgroundColor = gCOLOR_SELECTED;
    };  
    // log cell changed and id so the text box can be updated on the server
    LogCellChange(gLastIDClicked, newSKCell);
    //
    UpdateCellClass(gLastIDClicked);
}
function UpdateCellClass(varLastIDClicked) {
    // look at what is in the SK cell and assign the correct Class so that it is displayed correctly.
    var varSK_ID = varLastIDClicked;
    var CandidateCount = 0;
    var strSingleValue = "";

    CandidateCount = CountCandidates(varSK_ID);
    // the text box display is controlled entirely by CSS classes 
    if (CandidateCount == 1) {
        //   
        document.getElementById(varSK_ID).className = "clASPOneP clMyToolTip";
        document.getElementById(varSK_ID).value = GetScalarValue(varSK_ID);
        //  
    } else {
        document.getElementById(varSK_ID).className = "clASPMulti clMyToolTip";
    }
}
function CountCandidates(vCellID) {
    // return number of candidates remaining in this cell
    varCellValue = document.getElementById(vCellID).value;
    var SKCellArray = varCellValue.split("");
    var CandidateCount = 0;
    var strCandidate = "";
    var strSingleValue = "";
    for (i = 0; i <= gTEXTBOXCHARS; i++) {
        strCandidate = SKCellArray[i];
        if (/[1-9]/.test(strCandidate)) {       // VIP see this test for the characters 1 to 9!!!
            CandidateCount++;
            strSingleValue = strCandidate;
        }
    }
    return CandidateCount;
}
function isTargetInThisCell(vCellID, sP)
{
    // If the scalar passed in either the value of the cell or a candidate in the cell return true
    //  sP = 7          used for v12 flashing of all related cells
    //  cell    test
    //  7       true
    //  123     false
    //  12378   true
    varCellValue = document.getElementById(vCellID).value;
    var SKCellArray = varCellValue.split("");
    var bFound = false;
    var strCandidate = "";
    for (i = 0; i <= gTEXTBOXCHARS; i++) {
        strCandidate = SKCellArray[i];
        if (strCandidate == sP) {
            bFound = true;
        }
    }
    return bFound;
}
function GetScalarValue(vCellID) {
    // if the cell contains one Pvalue return that P value, if not return null
    varCellValue = document.getElementById(vCellID).value;
    var SKCellArray = varCellValue.split("");
    var CandidateCount = 0;
    var strCandidate = "";
    var strSingleValue = "";
    for (i = 0; i <= gTEXTBOXCHARS; i++) {
        strCandidate = SKCellArray[i];
        if (/[1-9]/.test(strCandidate)) {       // VIP see this test for the characters 1 to 9!!!
            CandidateCount++;
            strSingleValue = strCandidate;
        }
    }
    if (CandidateCount != 1) strSingleValue = "";
    return strSingleValue;
}
// ===================================== PROCESS MENU KEYS ==============================================
function OnClickMenu(varSourceID) {
    // fired when a menu item is clicked
    // naming convention liMenuXXXX where XXXX describes the menu item selected eg liMenuEasy
    // this convention is enforced in C# Page load event as well) for security reasons
    // *** if a Pre edit of the grid is required before the command is sent to the server assign it a class name of clPreEdit
    gEventCount++;      // count all events for stats
    HideButtons();  //hide number buttons
    var idKey = varSourceID.substr(0, 6);
    if (idKey != "liMenu") return;            // ignore any item without the proper name
    var vClassName = document.getElementById(varSourceID).className;
    onServerTimeout();  //start the clock, we have 5 minutes before server times out
    // pre edit of grid is required do the check before sending the page back to the server
    if (vClassName == "clPreEdit") {
        if (PreEdit() != true) return;      //if preedit fails don't do postback 
    }
    // all pre-editing done handle all other menu items
    var sEventCount = gEventCount.toString();
    gEventCount = 0;
    __doPostBack(varSourceID, sEventCount);
}
function OnClickPrint(varSourceID) {
    // Printing, use css to modify main page and go directly to the browsers print engine
    gEventCount++;      // count all events for stats
    var idKey = varSourceID.substr(0, 6);
    if (idKey == "liMenu") {        // dbl check the control id
        window.print();
    }
}
// ======================= PROCESS CLIENT SIDE OPTIONS ==========================
function OnClickOption(varSourceID) {
    // fired when the main menu item OPTIONS is clicked,
    // just display the options elements panel
    gEventCount++;      // count all events for stats
    HideButtons(); //hide number buttons
    UpdateMessagePanel("Options");

}
function onClickSave(vSourceID) {
    // save the options selected based on the checkbox states
    // ID           checked value      Name in cookie  Value in cookie
    // idOptRegions        true                 Opt1            Y
    // idOptRegions        false                Opt1            N
    // idOptScalars        true                 Opt2            Y
    // idOptScalars        false                Opt2            N
    //
    if (sGetCheckBoxValue("idOptRegions") == "Y") {
        setCookie("Opt1", "Y");
        gFlashMulti = "Y";              // keep global variables in sync
    } else {
        setCookie("Opt1", "N");
        gFlashMulti = "N";
    }
    if (sGetCheckBoxValue("idOptScalers") == "Y") {
        setCookie("Opt2", "Y");
        gFlashScalars = "Y";
    } else {
        setCookie("Opt2", "N");
        gFlashScalars = "N";
    }
    UpdateMessagePanel("ASP");
}
function onClickCancel(vSourceID) {
    // make no changes to the options saved, return to the ASP messages
    UpdateMessagePanel("ASP");
}    
function sGetCheckBoxValue(sID) {
    // the checkbox value is not used, only the checked / unchecked status
    // return Y (checked) or N (not checked)
    sReturn = "";
    if (document.getElementById(sID) == null) {
        sReturn = "N";      // probably not necessary
    } else {
        if (document.getElementById(sID).checked) {
            sReturn = "Y";
        } else {
            sReturn = "N";
        }
    }
    return sReturn;
}
function ShowButtons() {
    document.getElementById("idButtonsDiv").style.display = "inline"; //show number buttons 
}
function HideButtons() {
    document.getElementById("idButtonsDiv").style.display = "none";     //number buttoms hidden initially
}
// ======================================SUPPORT FUNCTIONS ========================================
function LogCellChange(varSKCellID, varContents) {
    // this needs to be done because ASP does not send changed values back to the server if the control is readonly
    // stuff into the hidden field the definiton of the entire case
    // the format of the text in the texbox is  tbASPr0c0.Text = " 123" & CrLf & " 456" & CrLf & " 789"
    // eg  tbASPr0c0 [123CrLf456CrLf789]
    // add new entry to the end of the list
    var CurrLog = document.getElementById("aspHFChangeLog").value;
    document.getElementById("aspHFChangeLog").value = CurrLog + "ID=" + varSKCellID + " NewValue=[" + varContents + "],";
}
function goBack() {
    // simulate brower back button.
    window.history.back()
}
//============================================= COOKIE FUNCTIONS ==============================
function setCookie(v_name, v_value) {
    //sets the cookie variable name to the value specified,  it updates the variable, does not add to end of a string
    //can call multiple times, it figures it out and maintains the value of the cookie variable
    document.cookie = v_name + "=" + v_value;
}
function ReadCookies() {
    gFlashMulti = ReadCookie("Opt1");
    gFlashScalars = ReadCookie("Opt2");
}
function ReadCookie(vCookieID) {
    var sCookieValue = "";
    var sReturn = "";
    sCookieValue = ReadCookieFile(vCookieID);
    sCookieDefault = "Y";        // default if no cookie file or not changed by user
    if (sCookieValue != null && sCookieValue != "") sReturn = sCookieValue; else sReturn = sCookieDefault;
    return sReturn;
}
function ReadCookieFile(v_name) {        // refw3schools.com
    // copied from the web site, parses all cookies delimited by ;
    // then exracts the value of the cookie variable name supplied
    // if v_name is not in the cookie file, the return value is 'undefined'
    var i, x, y, ARRcookies = document.cookie.split(";");
    y = "";
    for (i = 0; i < ARRcookies.length; i++) {
        x = ARRcookies[i].substr(0, ARRcookies[i].indexOf("="));
        y = ARRcookies[i].substr(ARRcookies[i].indexOf("=") + 1);
        x = x.replace(/^\s+|\s+$/g, "");
        if (x == v_name && y.length ==1) {      //security check, all values are one character
            return unescape(y);
        }
    }
}
// =================================== timer functions ==================================================
function myTimer() {
    // designed to be fired after n minutes of being idle so that the godaddy server doesn't forget about the session
    // reference : Godaddy’s Shared hosting time out is 20 minutes for asp.net and cannot be changed (apparently)
    var varDate = new Date();
    var varTime = varDate.toLocaleTimeString();
    //wake up Godaddy, code from OnClickMenu
    __doPostBack("evClientTimer", varTime);
}
function onServerTimeout() {
    // The timer needs to be started whenever the server responds and woken up n minutes later
    //  godaddy waits only n minutes before timing out. so madly clicking numbers in the grid does not 
    //  have any impact on godadddy timing out.
    //  start the timer when the server responds, if n minutes has passed wake up the server again.
    clearInterval(myTimerVar);
    myTimerVar = setInterval(function () { myTimer() }, gInteralDuration);   //used to keep godaddy server from timing out
}
// ======================================== noe functions ==============================================
function PreEdit() {
    //
    // check every solution set for conflicts
    // will contain the list of solution sets and p values in conflict that need to be highlighted
    //  eg  arrays are parallel so we don't use two dimensions.
    // entry  p=7 ss=26 means there are more than one 7 in the solution set number 26
    var iEntry = 0;
    var blnAllOK = true;
    gaErrList_P.length = 0;
    gaErrList_SS.length = 0;
    gTotalScalarsFound = 0;
    // check the current puzzle with the known solution
    for (iCell = 0; iCell < 81 ; iCell++) {
        var sCellid = CellNmbrToCellID(iCell);
        var SKCellValue = document.getElementById(sCellid).value;
        if (SKCellValue.length == 1) {
            if(bCheckSolution(sCellid,SKCellValue) == false) {
                document.getElementById(sCellid).style.backgroundColor = gCOLOR_ERROR_CELLS;
                blnAllOK = false;
            }
        }
    }
    //  now check everthing else
    gSS.forEach(GetEachSS);     //look at all solution sets
    var NoOfEntries = gaErrList_P.length;
    if (gaErrList_P.length > 0) {
        for (iEntry = 0; iEntry < NoOfEntries; iEntry++) {
            HighlightErrCells(gaErrList_SS[iEntry], gaErrList_P[iEntry]);
            blnAllOK = false;
        }
    } else {
        // if the stats report still says correct conflicts and their aren't any then remove the message. 
        //need 3*81 scalars for a solved puzzle
        if (gTotalScalarsFound == 243) DisplaySolvedMessage();               
    };

    if (blnAllOK)       
        DisplayConflictMessage(false);      // there are no conflicts
    else
        DisplayConflictMessage(true);       // there is a conflict

    return blnAllOK;
}
function GetEachSS(element, index, gSSArray) {
    //called 27 times, once for each solution set
    var i = 0;
    var aScalarCounts = new Array(0,0,0,0,0,0,0,0,0,0);    // counts how many scalars found in a SS
    // pass one count every scalar in the ss
    for (i = 0; i <= 8 ; i++) {
        var myCellID = CellNmbrToCellID(gSSArray[index][i]);
        var p = GetScalarValue(myCellID);
        //works perfectly
        if (p != "") {
            gTotalScalarsFound++;       // when we have 81*3 we know the puzzle is solved.
            var iP = parseInt(p);
            aScalarCounts[iP]++;
        }
    }
    // pass two, if a scalar occurs more than once, we have a conflict and need to highlight it
    for (iP = 1; iP <= 9; iP++) {
        if (aScalarCounts[iP] > 1) {
            gaErrList_P.push(iP);
            gaErrList_SS.push(index);
        }
    }
}

function HighlightErrCells(vSS, vP) {
 //   Highlight all error cells in a solution set
    var iCell = 0;
    for (iCell = 0; iCell <= 8 ; iCell++) {
        var myCellID = CellNmbrToCellID(gSS[vSS][iCell]);
        var p = parseInt(GetScalarValue(myCellID));
        //
        if (p == vP) {
            document.getElementById(myCellID).style.backgroundColor = gCOLOR_ERROR_CELLS;
        }
    }
}
function HighlightErrCell(vCellID) {
// highlight only one error cell
    document.getElementById(vCellID).style.backgroundColor = gCOLOR_ERROR_CELLS;
}
function CellNmbrToCellID(vCellNmbr) {
    // eg cell number 80 returns tbASPr8c8, works
    var r = Math.floor(vCellNmbr / 9);      // round down integer division
    var c = vCellNmbr - (r * 9);
    var vCellID = "tbASPr" + r + "c" + c;
    return vCellID;
}
function CellIDToCellNmbr(vCellID) {
    // eg tbASPr8c8 returns 80
    if (vCellID.length == 9) {
        var r = parseInt(vCellID.substr(6, 1));
        var c = parseInt(vCellID.substr(8, 1));
        return r * 9 + c;
    } else {
        return -1;
    }
}
function bCheckSolution(vCellID, vCellValue) {
    // answers the question. In cell number 22 is 7 part of the solution
    // returns true if 7 is part of the solution or if no solution is defined
    // cell id is the form tbASPr2c3
    var sSolution = document.getElementById("aspHFSolution").value;
    var iCellNmbr = CellIDToCellNmbr(vCellID);
    var sCheck = sSolution.substr(iCellNmbr, 1);
    if (sCheck == " ") return true;     // user input case so do not flag as wrong

    if (/[1-9]/.test(sCheck)) {         //really dont' need to check this
        if (vCellValue == sCheck) return true;
    }
    return false;
}
  

function DisplayConflictMessage(bYesNo) {

    // need to know what control is currently in the right hand panel. Update each one differently.
    // yes option
    var myConflicts = "Please correct conflicts shown in Red.";
    // no option
    var myNoConflicts = "Conflicts corrected.";    

    var vFoundHelp = null;
    vFoundHelp = document.getElementById('lblASPmess');

    if (vFoundHelp != null) {        // we have the help message in the panel
        if (bYesNo) {
            vFoundHelp.innerText = myConflicts;
        } else {
            if (vFoundHelp.innerText == myConflicts)
                vFoundHelp.innerText = myNoConflicts;
        }
        return;
    }
}
function DisplaySolvedMessage() {
    // need to know what control is currently in the right hand panel. Update each one differently.
    var vFoundHelp = null;
    vFoundHelp = document.getElementById('lblASPmess');

    if (vFoundHelp != null) {           // we have the help message in the panel
        vFoundHelp.innerText = "Puzzle Solved.";
        return;
    }
}

function myDebug(sMess) {
    var varDate = new Date();
    var varTime = varDate.toLocaleTimeString();
    var appname = window.navigator.appName;
    // just a stub in this release
}
function UpdateMessagePanel(sType) {
    //
    // make either the ASP generated elements visible or the options page elements visible
    //
    // asp panel will contain lblASPmess
    var idASP = document.getElementById('lblASPmess');
    if (idASP != null) {        // normally not needed but make it fail proof, if new elements added.
        if (sType == "ASP") {
            document.getElementById('lblASPmess').style.display = "inline";     //visible, the default
            document.getElementById('divOptPage').style.display = "none";       //hidden
        } else {
            document.getElementById('lblASPmess').style.display = "none";
            document.getElementById('divOptPage').style.display = "inline";
            if (gFlashMulti == "Y")
                document.getElementById("idOptRegions").checked = true;
            else
                document.getElementById("idOptRegions").checked = false;
            //
            if (gFlashScalars == "Y")
                document.getElementById("idOptScalers").checked = true;
            else
                document.getElementById("idOptScalers").checked = false;
        }
    }
}