using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
//
// This class is the main interface between the client browser and the backend server.  All puzzle definitions and
// most logic is done on the server side, however the process of updating the puzzle by the user is done in the
// browser and managed by the JavaScript code. 
//
public partial class _Default : System.Web.UI.Page
{
    // page events see book pg 190
    public enum enCommand
    {
        StartNew,
        GetRandomCase,
        GetDailyCase,
        ShowHints,
        Solve,
        CheckIT
    }
    //Value in drop down box indicating solveall cases
    const int MAXITERATIONS = 9999999;
    int mIterations;   // value from control id eg liMenuIter04
    //MAX NUMBER of cells to highight when giving hints
    const int MAX_HINT_CELLS = 81;
    // reference for symbols http://www.w3schools.com/tags/ref_symbols.asp
    const string NEWLINE = "\n";

    const string HTML_BREAK = "<br/>";          //fixed version 10.1
    const string HTML_BULLET = "&#x2727;";      //UNICODE White four-pointed star
    const string HTML_EIGHT_CIRCLED = "&#x2467";  //UNICODE 
    const string HTML_SMILING_FACE= "&#x1F601";      //  UNICODE EMOTICON SMILING FACE
    const string HTML_YIKES = "&#x2717;";       // NICE X UNICODE SYMBOL
    const string HTML_CHECK = "&#x2713;";       // CHECKMARK UNICODE SYMBOL
    protected void Page_Init(object sender, System.EventArgs e)
    {  

        if (Page.IsPostBack == false)
        {
            // populate grid with default daily case on initial load or screen refresh only
            SetUpCommands(enCommand.GetDailyCase,"NoCriteria"); 
        }
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        // postback from Javascript results in this event being called.  
        // the case is ready to process and accessed via the serversidesync routine
        // only works for controls with an id of the form liMenuXXXX
        // eliminates use of asp.net  runat="server" property
        // eliminates the drop down menu's were not too friendly on the iPad
        //
        ClientScript.GetPostBackEventReference(this, string.Empty);   //critical line of code
        if (Page.IsPostBack)
        // reference article http://www.dotnetspider.com/resources/1521-How-call-Postback-from-Javascript.aspx
        {
            // note if postback is a result of clicking on an asp button, myArg will be NULL
            string evTarget = Request.Params.Get("__EVENTTARGET");   //target looks just like the supplied arg to Postback __Page
            string evArg = Request.Params.Get("__EVENTARGUMENT");
            // special and does not evaluate 2nd expression if first is false
            if (evTarget.Length > 5 && evTarget.Substring(0, 6) == "liMenu" )    //make sure it is a control we are interested in
            {
                // postback called from javascript, ready to use
                ServerCommand(evTarget, evArg);
            }
            if (evTarget == "evClientTimer")    
            {
                ServerCommand(evTarget, evArg);
            }
        }
    }

    protected void ServerCommand(string sTarget, string sCMD)
    {
        // client side menu command button clicked, resulting in a javascrript postback to process the command
        // check out the logs which show what is happening
        string sCriteria;

        LogActivity(sTarget, sCMD);

        mIterations = 1;    //new default
        lblASPmess.Visible = true;

        switch (sTarget)
        {
            case "evClientTimer":          
                SolveCommands(enCommand.CheckIT);   //just simulate a check it if no activity for n minutes
                break;
            case "liMenuCreate":
                //start new puzzle, prepare for input
                sCriteria = "";
                SetUpCommands(enCommand.StartNew, sCriteria);
                break;
            case "liMenuDaily":
                sCriteria = "";         //daily puzzle is any technque or level for now
                SetUpCommands(enCommand.GetDailyCase, sCriteria);
                break;
            case "liMenuEasy":
                sCriteria = "L1";
                SetUpCommands(enCommand.GetRandomCase, sCriteria);
                break;
            case "liMenuMed":
                sCriteria = "L2";
                SetUpCommands(enCommand.GetRandomCase, sCriteria);
                break;
            case "liMenuDiff":
                sCriteria = "L3";
                SetUpCommands(enCommand.GetRandomCase, sCriteria);
                break;
            case "liMenuVDiff":
                sCriteria = "L4";
                SetUpCommands(enCommand.GetRandomCase, sCriteria);
                break;
            case "liMenuDiab":
                sCriteria = "L5";
                SetUpCommands(enCommand.GetRandomCase, sCriteria);
                break;
            case "liMenuColor":
                sCriteria = "T1";
                SetUpCommands(enCommand.GetRandomCase, sCriteria);
                break;
            case "liMenuSword":
                sCriteria = "T3";
                SetUpCommands(enCommand.GetRandomCase, sCriteria);
                break;
            case "liMenuXW":
                sCriteria = "T2";
                SetUpCommands(enCommand.GetRandomCase, sCriteria);
                break;
            case "liMenuGR":
                sCriteria = "T4";
                SetUpCommands(enCommand.GetRandomCase, sCriteria);
                break;
            case "liMenuCheck":
                SolveCommands(enCommand.CheckIT);
                break;
            case "liMenuHints":
                //show hints for current puzzle
                mIterations = 1;        //Change from before, used to use n iterations
                SolveCommands(enCommand.ShowHints);
                break;
            case "liMenuIter01":
                mIterations = 1;
                SolveCommands(enCommand.Solve);
                break;
            case "liMenuIter04":
                mIterations = 4;
                SolveCommands(enCommand.Solve);
                break;
            case "liMenuIter08":
                mIterations = 8;
                SolveCommands(enCommand.Solve);
                break;
            case "liMenuIter16":
                mIterations = 16;
                SolveCommands(enCommand.Solve);
                break;
            case "liMenuIterAll":
                mIterations = MAXITERATIONS;
                SolveCommands(enCommand.Solve);
                break;
            case "liMenuAbout":
                ServerSideSync();       //old bug need to record any changes before help button pushed
                sCriteria = "";
                lblASPmess.Visible = true;
                clsWebInterface myWebInterface = new clsWebInterface();
                lblASPmess.Text = myWebInterface.sGetAboutHTML;
                break;
            default:
                break;
        }


    }
// ==================================================================================================================
    private void PopulateSolnTable(XmlDocument myXML, string sAction)
    {
        //
        // the input xml document can be either type xml_IC or xml_CS
        //
        //   actions can be NewCase or Solve
        //
        XmlNodeList nodeList = null;

        clsXML xmlLib = new clsXML();       // new xml library in V12
        int i = 0;

        string sXMLCell = null;
        int[] intCells = new int[81];
        int r = 0;
        int c = 0;
        string pValues = null;
        bool bIsScalar = false;
        bool bIsAllValues = false;
        TextBox tbCurr = default(TextBox);
        string sNewText = null;
        string sOldText = null;

        if (sAction != "Solve") SaveAttributes(myXML);  // except when just recalculating an existing case 
        nodeList = myXML.GetElementsByTagName("cell");

        for (i = 0; i <= nodeList.Count - 1; i++)
        {
            sXMLCell = nodeList[i].InnerXml;
            r = Convert.ToInt32(sXMLCell.Substring(0, 1));
            c = Convert.ToInt32(sXMLCell.Substring(2, 1));
            pValues = sXMLCell.Substring(4);
            //char pos 4 onwards eg 347
            tbCurr = tbGetTextBoxControl(r, c);
            sOldText = tbCurr.Text;
            //previous value in text box
            sNewText = sBuildTextBoxPstring(pValues, ref bIsScalar, ref bIsAllValues);
            tbCurr.Text = sNewText;
            tbCurr.ToolTip = "";
            //no tool tips unless there is a change 
            //
            tbCurr.BackColor = System.Drawing.Color.White;

            // Text box attribute "StartValue"  possible values are:
            //    UserNew    - A new blank puzzle has been prepared for the user, expecting user input here
            //    Yes        - This cell contains the starting or initial value for a puzzle. Will show blue until the puzzle is solved
            //    No         - This is a cell to be solved in the final puzzle.
            //
            // set appropriate text box class for a new puzzle entered by the user


            if (tbCurr.Attributes["StartValue"] == "UserNew")
            {
                if (bIsScalar)
                {
                    tbCurr.CssClass = "clASPStart clMyToolTip";
                    tbCurr.Attributes["StartValue"] = "Yes";        // lock initial case values
                    tbCurr.ReadOnly = true;
                }
                else
                {
                    tbCurr.CssClass = "clASPMulti  clMyToolTip";
                    tbCurr.Attributes["StartValue"] = "No";
                }
            }
            // check for a new case (random or from the dropdown, not one we are in the middle of solving
            if (sAction == "NewCase")
            {
                if (bIsScalar)
                {
                    tbCurr.CssClass = "clASPStart clMyToolTip";
                    tbCurr.Attributes["StartValue"] = "Yes";
                }
                else
                {
                    tbCurr.CssClass = "clASPMulti  clMyToolTip";
                    tbCurr.Attributes["StartValue"] = "No";
                }
            }
            // partially solved case
            if (sAction == "Solve")
            {
                //fixed bug where non changed cells were indicated as changed
                if (sOldText != sNewText)
                {
                    tbCurr.ToolTip = sGetToolTipMess(sOldText, sNewText, sAction);
                    IndicateCellChange(r, c, "Yes");
                }
                //dont change the start value indicator, we are not at the start of a new case
                if (bIsScalar)
                {
                    if (tbCurr.Attributes["StartValue"] == "Yes")
                    {
                        tbCurr.CssClass = "clASPStart clMyToolTip";
                    }
                    else
                    {
                        tbCurr.CssClass = "clASPOneP clMyToolTip";
                    }
                }
                else
                {
                    tbCurr.CssClass = "clASPMulti  clMyToolTip";
                }

            }
            //Solve

        }
    }

    private string sBuildTextBoxPstring(string pValues, ref bool bIsScalar, ref bool bIsAllValues)
    {
        //
        // convert list of p values 
        //    from the form   123789     used by the backend
        //    to the form        123     used by the front end text box's
        //                       
        //                       789
        //
        string sTemplate = " 123" + NEWLINE + " 456" + NEWLINE + " 789";
        string sValue = null;
        string sResult = null;
        string sP = null;
        int iLen = 0;
        int iX = 0;
        sValue = pValues.Trim();
        sResult = sTemplate;
        bIsAllValues = false;
        bIsScalar = false;
        iLen = sValue.Length;
        if (iLen == 1)
        {
            bIsScalar = true;
            sResult = sValue;       //return single p value
        } else if (iLen == 9)
        {
            bIsAllValues = true;
            sResult = sTemplate;
        } else  if (iLen >= 2 & iLen <=8)
        {
            sResult = sTemplate;
            for (iX = 1; iX <= 9; iX++)         //template is preset but zap all p values not in the list
                {
                    sP = Convert.ToString(iX);
                    if (!sValue.Contains(sP))
                    {
                        sResult = sResult.Replace(sP, " ");  
                    }
                }
        }
       return sResult;
    }  //end

    private void IndicateCellChange(int r, int c, string sAction)
    {
        // if sAction is Yes  then change color to indicate so, if not return controls to default values
        //

        TextBox tb = default(TextBox);
        tb = tbGetTextBoxControl(r, c);
        if ((sAction == "Yes"))
        {
            tb.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFF33");  //light yellow
        }
        else
        {
            tb.BackColor = System.Drawing.Color.White;
        }

    }
    private void RemoveCellChangedIndicator()
    {
        // Ver7 change
        // when a command button is clicked, remove the current cell changed indications (tool tip and color change)
        // results in a cleaner interface.
        //
        int r = 0;
        int c = 0;
        TextBox tb = default(TextBox);
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
              //  IndicateCellChange(r, c, "No");
                tb = tbGetTextBoxControl(r, c);   //old tooltip is still there
                tb.BackColor = System.Drawing.Color.White;
                tb.ToolTip = "";
            }
        }

    }
    private void SetUpCommands(enCommand iCmd, string sCriteria)
    {
        // Command processor 
        //

        XmlDocument Xml_IC = new XmlDocument();
        string sCaseID = null;
        string sLevel = null;
        ServerSideSync();  //before doing anything, update the asp controls with the changes made in Javascript
        clsWebInterface myWebInterface = new clsWebInterface();
        string sMess = "";
        // load and set  the default header
        Xml_IC.LoadXml("<case caseid ='none' source='cdf' level='none' hardest='none' reason='none' casename='none' casedef='none' casesolution='none'> </case>");
//
        switch (iCmd)
        {
            case enCommand.StartNew:
                sMess = sBCM("", HTML_CHECK, "Enter puzzle into the grid by clicking on each cell and entering your candidates using the buttons on the bottom of the screen.");
                sMess = sBCM(sMess, HTML_CHECK, "Click Check It! to to see if there is a unique solution based on your input.");
                sMess = sBCM(sMess, HTML_CHECK, "Select Solve 1 Iteration.");
                sMess = sBCM(sMess, HTML_CHECK, "Click Show Hints and then research the technique described to solve the next step.");
                sMess = sBCM(sMess, HTML_CHECK, "Click Solve (1 iteration) to display the solution of the next step.");
                PrepSolnTblForInput();
                LogActivity("StartNewCase", "");
                break;
            case enCommand.GetRandomCase:
                myWebInterface.GetRandomCase(sCriteria, ref Xml_IC, ref sCaseID, ref sLevel);
                PopulateSolnTable(Xml_IC, "NewCase");
         //v12 remove       LogActivity("RandomCaseloaded", sCaseID);
                sMess = sBCM("", HTML_BULLET,"Case number " + sCaseID +  " " + sLevel + " Star "  + " randomly selected.");
                sMess = sBCM(sMess, HTML_BULLET,"New case ready to use.");
                sMess = sBCM(sMess, HTML_CHECK, "Click on any cell to start solving the puzzle or click Show Hints for an idea on where to start or Click Solve (1 iteration) to get a little help.");
                break;
            case enCommand.GetDailyCase:
                myWebInterface.GetDailyCase(ref Xml_IC, ref sCaseID, ref sLevel);
                PopulateSolnTable(Xml_IC, "NewCase"); 
                sMess = sBCM("", HTML_BULLET,"Your Daily Sudoku Puzzle " + sCaseID +  " is now ready.");
                LogActivity("DailyCaseloaded", sCaseID);
                sMess = sBCM(sMess, HTML_CHECK, "Click Check It! To see the techniques needed to solve the puzzle.");
                sMess = sBCM(sMess, HTML_CHECK, "Click on any cell to start solving the puzzle or click Show Hints for an idea on where to start.");
                sMess = sBCM(sMess, HTML_CHECK, "Click on Help to see features in this version.");
                break;
        }
        lblASPmess.Text = sMess;
    }  // end of setup commands
    private void SolveCommands(enCommand iCmd)
    {
        // process hints, checkit and solve i 
        // a case is ready to solve, check it and solve it.  
        XmlDocument Xml_IC = new XmlDocument(); //Input Case      
        XmlDocument Xml_CS = new XmlDocument(); //Case Solution
        XmlDocument Xml_TS = new XmlDocument(); //Trial Solution
        clsXML xmlLib = new clsXML();       // new xml library in V12
        clsSK mySK = new clsSK();
        bool bEmptyCase = false;
        bool bIsValid = false;
        string sStatus = null;
        string sValidFlag = null;
        string sReason = null;
        //
        //before doing anything, update the asp controls with the changes made in Javascript
        //
        ServerSideSync();
        // we have a case to solve do a bunch of standard things that apply to all commands     
        if (bNoInput()) return;         //just do nothing if no input
        BuildXMLFromSolnTable(ref Xml_IC, ref bEmptyCase);
        mySK.LoadFromXML(ref Xml_IC);

        bIsValid = mySK.IsPuzzleValid;
        //only applies if user input is invalid
        if (!bIsValid)
        {
            UpdateResultsPanel(Xml_IC, "Invalid");
            return;
            //try again keep input table enabled
        }
        //if puzzle is solved, ie every cell is a scaler and it all works just 
        //say puzzle is solved.
        //
        // remove cell changed indicators if a new command is entered
        //
        RemoveCellChangedIndicator();

        if (mySK.IsPuzzleSolved)
        {
            FormatSolvedTable();
            lblASPmess.Text = sBCM("", HTML_CHECK, "Puzzle has been solved.");
            return;
        }
        if (iCmd == enCommand.ShowHints)
        {
            // make sure the hints reflect the current table
            BuildXMLFromSolnTable(ref Xml_IC, ref bEmptyCase);
            mySK.LoadFromXML(ref Xml_IC);

            string sHintsMess = null;
            string sFreqMess = null;
            sHintsMess = sGetHintsList(mySK, mIterations, ref Xml_IC);
             clsWebInterface myWebInterface = new clsWebInterface();
             myWebInterface.GetFreqAnalysis(mySK, ref sFreqMess);   // build frequency analysis and add to GetHints html
             sHintsMess = sHintsMess + sFreqMess;
             lblASPmess.Text = sHintsMess;
             return;
        }
        if (iCmd == enCommand.Solve)
        {   
            SolvePuzzle();
            return;
        }
        if (iCmd == enCommand.CheckIT)
        {
            TrialSolution(mySK, ref Xml_TS, ref sStatus, ref sValidFlag, ref sReason);

            lblASPmess.Text = sCheckIt(Xml_TS, sStatus, sValidFlag, sReason);
            return;
        }
    }
    private void ServerSideSync()
    {
        // reverted to asp.net 4.0   static client id format.  Much cleaner
        // use the list of cells changed to update the text box's on the server side. Thus keeping the server and client in sync.
        // very cool in that only the cells changed are updated on the server.
        string sChangeLog = null;
        int iListCnt = 0;
        string[] sCellsUpdated = null;
        //string array to receive parsed change log
        int iSKCell = 0;
        string sControlID = null;
        string sControlValue = null;
        string sCurrEntry = null;
        TextBox tbFound ;
        char cDelim = ',';
        sChangeLog = aspHFChangeLog.Value;
        //updated/hidden on the client

        // IMPORTANT FROM MSDN WEB SITE
        //The Text value of a TextBox control with the ReadOnly property set to true is sent to the server when a postback occurs, 
        //but the server does no processing for a read-only text box. 
        //This prevents a malicious user from changing a Text value that is read-only. 
        //The value of the Text property is preserved in the view state between postbacks unless modified by server-side code.
        //Thus need to repopulate the grid whenever a server is invoked.
        sCellsUpdated = sChangeLog.Split(cDelim);       
        // vip ignore the last entry it is null
        // now update the client
        iListCnt = sCellsUpdated.GetUpperBound(0) - 1;
        string sPValues = null;
        int iPstart = 0;
        int iPend = 0;
        int iPlen = 0;

        for (iSKCell = 0; iSKCell <= iListCnt; iSKCell++)
        {
            sCurrEntry = sCellsUpdated[iSKCell];
            //fixed format vip
            // 		eg sCellsUpdated(0) ID=tbASPr0c1 NewValue=[                ]
            //          sCellsUpdated(1) ID=tbASPr0c1 NewValue=[123             ]
            sControlID = sCurrEntry.Substring(3, 9);
            iPstart = sCurrEntry.IndexOf("[") + 1;
            iPend = sCurrEntry.IndexOf("]") -1;
            iPlen = iPend - iPstart + 1;
            if (iPlen  <= 0)
            {
                //the javascript is not delimiting with [ and ] or the change log text cell has been corrupted/hacked.
                break; 
            }
            sControlValue = sCurrEntry.Substring(iPstart, iPlen);
            tbFound = (TextBox)tblsoln.FindControl(sControlID);     //must use tblsoln to find the control
            tbFound.Text = sControlValue;
            // display purposes only

            sPValues = sGetPValuesFromTextBox(sControlValue);       
            //convert to backend list of pvalues
            if (sPValues.Length == 1)
            {
                // now update the class of the text cell to get it displayed correctly in the browser
                tbFound.Text = sPValues;
                //vip the browser has [   7    ] but we need [7] so it can be displayed as a scalar
                tbFound.CssClass = "clASPOneP clMyToolTip";
            }
            else
            {
                tbFound.CssClass = "clASPMulti  clMyToolTip";
            }
        }
        // finally null out the list of cells updated so we don't do this twice
        aspHFChangeLog.Value = "";

    }

    private void PrepSolnTblForInput()
    {
        //
        //prepare the soln table to receive input from the users
        //
        int r = 0;
        int c = 0;
        string sSpace = " ";
        char cPadding = ' ';
        TextBox tb = default(TextBox);
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                tb = tbGetTextBoxControl(r, c);
                tb.Text = "    " + NEWLINE + "    " + NEWLINE + "    ";
                tb.Attributes["StartValue"] = "UserNew";
                IndicateCellChange(r, c, "No");
                tb.ToolTip = "";
            }
        }
        // 
        // special case where no solution  is available since this is the users initial case
        aspHFSolution.Value = sSpace.PadRight(81, cPadding);    //all scalars are spaces
        aspHFCaseid.Value = "UserCase";     
    }
    private void NewRandomCase(ref string vCriteria)
    {
        XmlDocument Xml_IC = new XmlDocument();
        clsWebInterface myWebInterface = new clsWebInterface();
        string sCaseID = null;
        string sLevel = null;
        myWebInterface.GetRandomCase(vCriteria, ref Xml_IC, ref sCaseID, ref sLevel);
        PopulateSolnTable(Xml_IC, "NewCase");
    }
   
    private void SolvePuzzle()
    {
        //
        // pass number of iterations to calculate before returning
        //
        XmlDocument Xml_IC = new XmlDocument();
        //Input Case
        XmlDocument Xml_CS = new XmlDocument();
        //Case Solution
        clsSK mySK = new clsSK();
        bool bEmptyCase = false;
        bool bIsValid = false;
        //
        BuildXMLFromSolnTable(ref Xml_IC, ref bEmptyCase);
        mySK.LoadFromXML(ref Xml_IC);
        bIsValid = mySK.IsPuzzleValid;
        //
        mySK.IterationCount = mIterations;      //from the drop down
        mySK.SolveIT();
        // need routine to save attributes to hidden field and another to restore them to a xml header
        Xml_CS = mySK.GetXMLResults();      
        PopulateSolnTable(Xml_CS, "Solve");

        if (mIterations < MAXITERATIONS)
        {
            UpdateResultsPanel(Xml_CS,"SolveN");
        }
        else
        {
            UpdateResultsPanel(Xml_CS, "SolveAll");
        }

    }
    private string sGetPValuesFromTextBox(string sDisplay)
    {
        //
        //convert display form of cell contents to pure p numbers.
        //  eg from   123        Used in the frontend textbox's
        //            4 6
        //            7 9
        //  to        1234679    Used in the backend
        //
        string sResult = "";
        int pV = 0;
        string sV;
        for (pV = 1; pV <= 9; pV++)
        {
            sV = Convert.ToString(pV);
            if (sDisplay.IndexOf(sV) >= 0) sResult = sResult + sV;    //does string  contain the p value ?
        }       
        //empty cell default is 
        if (sResult.Length == 0) sResult = "123456789";

        return sResult;
    }
    private TextBox tbGetTextBoxControl(int r, int c)
    {
        string sControlID = null;
        sControlID = "tbASPr" + r + "c" + c;
        return (TextBox)tblsoln.FindControl(sControlID);   //vip cast any control to type textbox

    }
    private void BuildXMLFromSolnTable(ref XmlDocument Xml_IC, ref bool bEmptyCase)
    {
        //
        // create xml document, format InputCase.xml where from the soln table
        // each cell <cell>xx p<\cell>  where xx is 0 based index  and p is p value
        //
        XmlElement elem = null;
        string pDisplay = null;
        string pValues = null;

        int r = 0;
        int c = 0;
        TextBox tb = default(TextBox);
        bEmptyCase = true;
        //detect if any cells are not 123456789
        Xml_IC.LoadXml("<case caseid ='none' source='cdf' level='none' hardest='none' reason='none' casename='none' casedef='none' casesolution='none'> </case>"); 
        // 
        // ignore the header for this document is not valid but the header info is on the current page so we don't need use
        // the info from the xml doc.
        //
        XmlElement root;
        root = Xml_IC.DocumentElement;
        // 
        // create the <cell> elements
        // for each cell create an entry of the form r c 6789 where 6..9 are candidates
        //
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                tb = tbGetTextBoxControl(r, c);
                pDisplay = tb.Text;
                pValues = sGetPValuesFromTextBox(pDisplay);
                if (pValues.Length != 9)
                {
                    bEmptyCase = false;
                    //all cells 123456789 if empty case
                }
                //Create a new node.
                elem = Xml_IC.CreateElement("cell");
                elem.InnerText = Convert.ToString(r) + " " + Convert.ToString(c) + " " + pValues;
                //Add the node to the document.
                root.AppendChild(elem);
            }
        }

    }
    private void UpdateResultsPanel(XmlDocument myXML, string sSource)
    {
        // vip this sub returns with the control lblASPmess updated
        // type is solve once, solve all or invalid
        // input document can be either type xml_CS or xml_IC, but only type XML_CS is used
        //
        XmlNodeList nodeList = null;
        clsXML xmlLib = new clsXML();       // new xml library in V12
        int i = 0;
        string sXMLValue = null;
        string sStatus = null;
        string sValidFlag = null;
        string sReason = null;
        string sStatsCnt = null;
        string sXML = null;
        int iTotalChanges = 0;
        string sMess = "";
        //
        //By def'n invalid user input trapped now
        if (sSource == "Invalid")
        {
            //convert to xml to a base case definition without the 123456789 stuff
            //
            sMess = sAddReasonMessage(sMess, "INVALIDINPUT");
            lblASPmess.Text = sMess;
            sXML = myXML.InnerXml;
            //conains xml_IC format
            //means invalid user input in this context
            return;
        }
        //
        //only found in xml_cs documents
        //
        sStatus = xmlLib.getNodeValue(myXML, "status");
        //solved or unsolved at the moment
        sValidFlag = xmlLib.getNodeValue(myXML, "valid");
        //can be solved if valid
        sReason = xmlLib.getNodeValue(myXML, "reason");
        //reason it can't be solved.

        // techniques used 
        iTotalChanges = 0;
        nodeList = myXML.GetElementsByTagName("stats");
        for (i = 0; i <= nodeList.Count - 1; i++)
        {
            sXMLValue = nodeList[i].InnerXml;
            sStatsCnt = sXMLValue.Substring(3, 4);
            if (Convert.ToInt32(sStatsCnt) != 0)
            {
                iTotalChanges = iTotalChanges + Convert.ToInt32(sStatsCnt);
            }
        }
        //
        if (iTotalChanges > 0)
        {
            sMess = sBCM(sMess,HTML_BULLET, sGetTechniqueList(nodeList,"Go"));
        }
        if (sValidFlag == "No")
        {
            //
            sMess = sAddReasonMessage(sMess, sReason);
            lblASPmess.Text = sMess;
            //special case, may be possible that brute force is
            //quitting too soon so we want to log these too
            //
            sXML = myXML.InnerXml;
            //xml contains the reason codes etc.
            LogActivity(sMess,sReason);
            //more than 1 soln or not solvable by brute force
            return;
        }
        // puzzle is valid and 
        if (sStatus == "Yes")
        {
            lblASPmess.Text = sBCM(sMess,HTML_CHECK,"Puzzle has been solved.");
            return;
        }
        //
        //puzzle is valid but unsolved...
        //
        if (sSource == "SolveAll")
        {
            sXML = myXML.InnerXml;
            LogActivity("SolveAll returns", "unsolved");
            sMess = sBCM(sMess,HTML_YIKES, "Unable to solve this puzzle.");
        }
        else if (sSource == "SolveN" & iTotalChanges == 0)
        {
            sXML = myXML.InnerXml;
            LogActivity("SolveN returns ", "unsolved");
            sMess = sBCM(sMess,HTML_YIKES, "Unable to solve the next step.");
        }
        else
        {
            sMess = sBCM(sMess,HTML_CHECK, "Puzzle OK so far.");    // Version 9.2 enhancement Cool
        }
        lblASPmess.Text = sMess;    //note front end control updated.
    }

    private void FormatSolvedTable()
    {
        //
        // the puzzle has been solved, just format it correctly
        //
        int r = 0;
        int c = 0;
        TextBox tbCurr = default(TextBox);

        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                tbCurr = tbGetTextBoxControl(r, c);
                tbCurr.ToolTip = "";
                tbCurr.BackColor = System.Drawing.Color.White;
                if (tbCurr.Attributes["StartValue"] == "Yes")
                {
                    tbCurr.CssClass = "clASPStart clMyToolTip";
                }
                else
                {
                    tbCurr.CssClass = "clASPOneP clMyToolTip";
                }
            }
        }
    }

    private string sAddReasonMessage(string sCurrMess, string vSreason)
    {
        //rewritted version 10, new front end control
        string sMess = sCurrMess;       //just add to current message
        //qqq reason is Unable to solve.  where is the real reason
        switch (vSreason)
        {
            case "MAXTABLESIZE":
                sMess = sBCM(sMess, HTML_YIKES, "There may not be a solution to this puzzle.");
                break;
            case "2SOLUTIONS":
                sMess = sBCM(sMess, HTML_YIKES, "There is more than 1 solution so this is not a valid Sudoku puzzle.");
                break;
            case "0SOLUTIONS":
                sMess = sBCM(sMess, HTML_YIKES, "Even brute force could not find a solution to this puzzle.");
                break;
            case "1SOLUTION":
                sMess = sBCM(sMess, HTML_CHECK, "There is a single valid solution for this puzzle.");
                break;
            case "VALIDINPUT":
                sMess = sBCM(sMess, HTML_CHECK, "Current puzzle contains no conflicts.");
                break;
            case "INVALIDINPUT":
                // seldom, if ever executed as this test is now done in javascript 
                sMess = sBCM(sMess, HTML_YIKES, "There is a conflict in the puzzle.");
                sMess = sBCM(sMess, HTML_YIKES,"Check for more than one occurance of the same candidate in any row, column or box.");
                sMess = sBCM(sMess, HTML_YIKES, "All candidates must be numbers from 1 to 9");
                sMess = sBCM(sMess, HTML_YIKES, "At least 17 numbers are needed.");
                break;
        }
        return sMess;      //updated message with new info
    }
   
    private string sGetTechniqueList(XmlNodeList rNodeList,string rType)
    {
        // contstruct the messages displayed for 3 scenarios, Hints, CheckIt's and Go's
        string sXMLValue = null;
        int i = 0;
        string sStatsCode = null;
        string sStatsCnt = null;
        string sResult = null;
        string sStatsText = null;
        //
        sResult = "";
        switch (rType)
        {
            case "Hints":
                sResult = "Use technique: " + HTML_BREAK;
                break;
            case "CheckIt":
                sResult = "Techniques needed are: " + HTML_BREAK;
                break;
            case "Go":
                sResult = "Techniques used were: " + HTML_BREAK;
                break;
        }
        //
        for (i = 0; i <= rNodeList.Count - 1; i++)
        {
            sXMLValue = rNodeList[i].InnerXml;
            // new stats are of the form NS 1234 NAKED SINGLE  (THE DESCRIPTION IS INCLUDED IN XML)
            sStatsCode = sXMLValue.Substring(0, 2);
            sStatsCnt = sXMLValue.Substring(3, 4);
            sStatsText = sXMLValue.Substring(8);

            if (Convert.ToInt32(sStatsCnt) != 0)
            {
                switch (rType)
                {
                    case "Hints":
                        sResult = sResult + sGetTechnique(sStatsText, sStatsCnt, "NoCounts") + HTML_BREAK;
                        break;
                    case "Go":
                        sResult = sResult + sGetTechnique(sStatsText, sStatsCnt, "Counts") + HTML_BREAK;
                        break;
                    case "CheckIt":
                        sResult = sResult + sGetTechnique(sStatsText, sStatsCnt, "NoCounts") + HTML_BREAK;
                        break;
                }
            }
        }

        switch (rType)
        {
            case "Hints":
                sResult = sResult + " to solve the grey highlighted cells.";
                break;
            case "CheckIt":
                sResult = sResult + "";
                break;
            case "Go":
                sResult = sResult + HTML_BULLET + " Cells that have changed are show in yellow.";
                break;
        }
        return sResult;
    }
    private string sGetTechnique(string sStatsText , string sCnt, string sType)
    {
        string functionReturnValue = null;
        //   sStatsText is the description of the technique  eg Naked Single
        //   either add counts to the technnique or don't

        if (sType == "Counts")
        {
            char cZeros = '0';
            string sFmt = sCnt.TrimStart(cZeros);
            functionReturnValue = HTML_CHECK + sStatsText + "(" + sFmt + ")";       
        }
        else
        {
            functionReturnValue = HTML_CHECK + sStatsText;
        }
        return functionReturnValue;
    }

    private bool bNoInput()
    {
        bool functionReturnValue = false;
        //
        //check for initial state where user hasn't entered anything.
        //
        int r = 0;
        int c = 0;
        TextBox tb = default(TextBox);
        string sPValues = null;
        functionReturnValue = true;
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                tb = tbGetTextBoxControl(r, c);
                sPValues = sGetPValuesFromTextBox(tb.Text);
                //len 0 is empty, len 9 is all values 
                if (sPValues.Length > 0 | sPValues.Length < 9)
                {
                    functionReturnValue = false;
                    return functionReturnValue;
                    //found some input
                }
            }
        }
        return functionReturnValue;
    }
    private void TrialSolution(clsSK rmySK, ref XmlDocument xml_TS, ref string sStatus, ref string sValidFlag, ref string sReason)
    {
        //
        // Preform a complete trial solution to make sure the case is solveable
        //
        //
        clsXML xmlLib = new clsXML();       // new xml library in V12
        rmySK.IterationCount = MAXITERATIONS;
        rmySK.UseNishio = true;
        rmySK.UseBruteForce = true;
        rmySK.SolveIT();
        sValidFlag = xmlLib.getNodeValue(xml_TS, "valid");  //solved or unsolved at the moment
        xml_TS = rmySK.GetXMLResults(); 
        if (rmySK.sIsSolved() == "No")  
        {
            sStatus = "No";
            sReason = xmlLib.getNodeValue(xml_TS, "reason");   // bug found v10 testing.. was there awhile
            return;
        }
        sStatus = xmlLib.getNodeValue(xml_TS, "status");    //see if we solved the puzzle  
        //can be solved if valid
        sReason = xmlLib.getNodeValue(xml_TS, "reason");    //reason it can't be solved.
    }

    private string sCheckIt(XmlDocument Xml_TS, string sStatus, string sValidFlag, string sReason)
    {
        //
        // case has already been processed via the TrialSolution
        //
        int iCase = 0;
        string sMess = "";

        XmlNodeList nodeList = null;

        if (sStatus == "Yes")
        {
            iCase = 1;
            //solved
        }
        else if (sValidFlag == "Yes")
        {
            iCase = 2;
            //unsolved but still a valid case
        }
        else
        {
            iCase = 3;
            //unsolved and unsolveable display 'reason why'
        }
        //
        //this case is valid but no problem checking again. see command processorr
        //
        switch (iCase)
        {
            case 1:
                sMess = sBCM(sMess, HTML_CHECK,"Puzzle can be solved.");
                nodeList = Xml_TS.GetElementsByTagName("stats");
                //get stats results
                sMess = sBCM(sMess, HTML_BULLET,sGetTechniqueList(nodeList, "CheckIt"));
                break;
            case 2:
                sMess = sBCM(sMess, HTML_CHECK,"Puzzle is valid.");
                nodeList = Xml_TS.GetElementsByTagName("stats");
                //get stats results
                sMess = sBCM(sMess, HTML_BULLET,sGetTechniqueList(nodeList, "CheckIt"));
                break;
            case 3:
                sMess = sBCM(sMess, HTML_YIKES,"Puzzle is not solveable.");
                sMess = sAddReasonMessage(sMess, sReason);
                break;
        }
        return sMess;
    }
    private string  sGetHintsList(clsSK rmySK, int vIterCnt, ref XmlDocument rXML_IC)
    {
        // 
        //use update hints because wording is what will happend in the next step
        //not what has already happened.
        //xml_ic contains the current case on the screen compare it to the results in xml_cs after returning
        //from solvit.  Highlight those cells
        //
        XmlDocument Xml_CS = new XmlDocument();
        clsXML xmlLib = new clsXML();       // new xml library in V12
        //
        XmlNodeList nodeList = null;
        int i = 0;
        string sStatus = null;
        string sValidFlag = null;
        string sReason = null;
        int iCase = 0;
        string sMess = "";

        rmySK.IterationCount = 1;
        rmySK.SolveIT();
        Xml_CS = rmySK.GetXMLResults();
        sMess = "";
        //see if we solved the puzzle
        sStatus = xmlLib.getNodeValue(Xml_CS, "status");
        //solved or unsolved at the moment
        sValidFlag = xmlLib.getNodeValue(Xml_CS, "valid");
        //can be solved if valid
        sReason = xmlLib.getNodeValue(Xml_CS, "reason");
        //reason it can't be solved.
        //
        //this case is valid but no problem checking again. see command processorr
        //
        if (sStatus == "Yes")
        {
            iCase = 1;
            //solved
        }
        else if (sValidFlag == "Yes")
        {
            iCase = 2;
            //unsolved but still a valid case
        }
        else
        {
            iCase = 3;
            //unsolved and unsolveable display 'reason why'
        }

        switch (iCase)
        {
            case 1:
                //solved
               sMess = sBCM(sMess, HTML_CHECK,"Puzzle has been solved.");
                break;
            case 2:
                //unsolved and valid
                nodeList = Xml_CS.GetElementsByTagName("stats");
                //get stats results
               sMess = sBCM(sMess, HTML_BULLET,sGetTechniqueList(nodeList,"Hints"));
                break;
            case 3:
                //unsolved and unsolveable
               sMess = sBCM(sMess, HTML_YIKES,"Puzzle appears to be unsolved and unsolveable.");
               sMess=  sAddReasonMessage(sMess,sReason);
                break;
        }
        //============================= NEW SECTION TO FIND OUT WHAT CELLS ARE REFERRED TO IN THE HINTS LIST xxx
        string[] sOnScreen = new string[82];
        string[] sNextIteration = new string[82];
        string pValues = null;
        int iOnScreenCnt = 0;
        int iNextIterCnt = 0;
        int iListCnt = 0;
        int iDisplayCnt = 0;
        TextBox tbCurr = default(TextBox);
        int r = 0;
        int c = 0;
        nodeList = rXML_IC.GetElementsByTagName("cell");
        iOnScreenCnt = nodeList.Count;
        for (i = 0; i <= iOnScreenCnt - 1; i++)
        {
            sOnScreen[i] = nodeList[i].InnerXml;
        }
        nodeList = Xml_CS.GetElementsByTagName("cell");
        iNextIterCnt = nodeList.Count;
        for (i = 0; i <= iNextIterCnt - 1; i++)
        {
            sNextIteration[i] = nodeList[i].InnerXml;
        }
        iListCnt = Math.Max(iOnScreenCnt, iNextIterCnt);

        //almost impossible since both lists should have 81 cells, but be safe
        if (iListCnt <= 0)
        {
            return sMess;
        }
        //only show the first 6 cells in the list
        iDisplayCnt = 0;
        //Get rid of the change colors on the screen, too cluttered....
        PrepGridForHints();
        for (i = 0; i <= iListCnt - 1; i++)
        {
            if (sOnScreen[i] != sNextIteration[i])
            {
                iDisplayCnt = iDisplayCnt + 1;
                //no matter how many changes only show this many
                if (iDisplayCnt <= MAX_HINT_CELLS)
                {
                    r = Convert.ToInt32(sOnScreen[i].Substring(0, 1));
                    c = Convert.ToInt32(sOnScreen[i].Substring(2, 1));
                    pValues = sNextIteration[i].Substring(4);
                    //char pos 4 onwards eg 347
                    tbCurr = tbGetTextBoxControl(r, c);
                    tbCurr.BackColor = System.Drawing.Color.LightGray;
                }
            }
        }
        return sMess;
    }
    private void PrepGridForHints()
    {
        int r = 0;
        int c = 0;
        TextBox tbCurr = default(TextBox);
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                tbCurr = tbGetTextBoxControl(r, c);
                tbCurr.BackColor = System.Drawing.Color.White;
                tbCurr.ToolTip = "";
                //applies to the last iteration
            }
        }
    }

    private string sGetToolTipMess(string sOldTextBox, string sNewTextBox, string sAction)
    {
        string functionReturnValue = null;
        //
        //create tool tip message from old and new list of p values
        //
        string sOld = null;
        string sNew = null;
        short i = 0;
        string sOut = null;
        string sNoChange = "";
        //identify cells not changed
        //
        // convert textbox strings to list of p values (easier to work with)
        //
        functionReturnValue = "";
        sOld = sGetPValuesFromTextBox(sOldTextBox);
        //convert both to form 135...
        sNew = sGetPValuesFromTextBox(sNewTextBox);

        string sOldTest = sOld.Trim();
        string sNewtest = sNew.Trim();

        if (string.IsNullOrEmpty(sOldTest) & string.IsNullOrEmpty(sNewtest))
        {
            functionReturnValue = sNoChange;
            return functionReturnValue;
        }
        if (string.IsNullOrEmpty(sOldTest))
        {
            sOld = "123456789";
            //note passed by value
        }
        sOut = "";
        for (i = 0; i <= 9; i++)
        {
            //eg old = 123456789
            //   new = 12 456 89
            // diffs   fftffftff     need to show that 3 and 7 have been eliminated, therefore true for them
            string si = Convert.ToString(i);
            if (sOld.Contains(si) & !sNew.Contains(si))
            {
                sOut += i + " ";
                //generate 3 7
            }
        }
        if (string.IsNullOrEmpty(sOut))
        {
            sOut = sNoChange;
        }
        else
        {
            sOut += "Removed";
        }
        functionReturnValue = sOut;
        return functionReturnValue;
    }
    private void LogActivity(string sEvent, string sEventDesc)
    {
        // no logging in this app, I have used both SQL server and a text file for logging in the past
    }
    private string sBCM(string sCurrLine, string sDelim, string sNextLine) {
        // BCM is short for build client message
        // string to be sent to the control lblASPmess
        // note that html can be used.
        string sResult = null;
        sResult = sCurrLine + "<b>" + sDelim + "</b> " + sNextLine + HTML_BREAK;
        return sResult;
        }  
    private void SaveAttributes(XmlDocument myXML)
    {
        // input document must be of the form "<case caseid ='none' source='cdf' level='none' hardest='none' reason='none' casename='none' casedef='none' casesolution='none'> </case>");  
        // save the attributes from the xml header so they can be used to 
        // construct the new xml document
        // ie   XML --> TO WEB PAGE AND THE --->  XML 
        clsXML xmlLib = new clsXML();       // new xml library in V12
        // used same technique for other attributes if necessary 
        aspHFSolution.Value = xmlLib.getAttr(myXML, "casesolution");   // asp hidden field
        aspHFCaseid.Value = xmlLib.getAttr(myXML, "caseid");
    }
 //end of document
}