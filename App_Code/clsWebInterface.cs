using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;        //was used for event logging on the server.
using System.Xml;
// see the SudokuAnalyzerMindMap to see the key role of this class
public class clsWebInterface : System.Web.UI.Page
{
    // DEFINE CASE DEFINITION FILE COLUMN NUMBERS
    const int CF_CASEID = 0;
    const int CF_LEVEL = 1;
    const int CF_HARDEST = 2;
    const int CF_REASON = 3;
    const int CF_CASENAME = 4;
    const int CF_CASEDEF = 5;
    const int CF_CASESOLN = 6;

    private string mCasePathName = "\\App_Data\\CaseDefinitionV12.txt";  // open with Server.MapPath(".") + path name
    private string mAboutPathName = "\\App_Code\\MyAbout.html";  // open with Server.MapPath(".") + path name
    private string mFreqAnalysisPathName = "\\App_Code\\MyFreqAnalysis.html";  // open with Server.MapPath(".") + path name
	private string[,] mSelected = new string[1000, 7];      //subset set of cases selected by user input criteria
    private int mSelectCount;

  
    public void GetRandomCase(string vCriteria, ref XmlDocument Xml_IC, ref string srCaseID, ref string srLevel)
    {
        // Use with the case definitions file
        // possible criteria, see list item values of drop down in main screen
        // do only 1* to 5* for now
        //
        string sPath = null;
        string[] sFields = null;
        char cDelim = ',';
        string sLevel;
        string sHardest;
        bool bStarSelect = false;
        bool bTechSelected = false;
        bool bSelectThis = false;

        string sLevelSelected = null;
        string sTechGroupSelected = null;

        Random rnd1 = new Random();

        sPath = Server.MapPath(".") + mCasePathName;
        mSelectCount = 0;           // new subset of cases
        // determine type of search and matchin criteria
        if (vCriteria.IndexOf('L') == 0)
            {
                bStarSelect = true;
                sLevelSelected = vCriteria.Substring(1, 1);     // must be of the formL1 L2 L3 ETC
            }
        if (vCriteria.IndexOf('T') == 0)
        {
            bTechSelected = true;
            sTechGroupSelected = vCriteria;     // must be of the form T1, T2, T3 ETC
        }   
        
        foreach (string sLineIn in File.ReadLines(sPath))
        {
            if (sLineIn.Length >= 100)              // ignore null or incomplete lines
            {
                sFields = sLineIn.Split(cDelim);   // new form of case definition file
                if (sFields[CF_REASON] == "OKINPROD")        // rudimentary check make sure the definitons is the correct length
                {
                    sLevel = sFields[CF_LEVEL];    // difficulty level 1-5
                    sHardest = sFields[CF_HARDEST];    // hardest technique
                    bSelectThis = false;

                    if (bStarSelect & sLevel == sLevelSelected) bSelectThis = true;
                    if (bTechSelected & bIsTechInGroup(sTechGroupSelected, sHardest)) bSelectThis = true;
                    if (bSelectThis)
                    {
                        int i;
                        for (i = 0; i <= 6; i++) mSelected[mSelectCount, i] = sFields[i];
                        mSelectCount = mSelectCount + 1;
                    }
                }
            }
        }
        // We have the correct subset of the case file in memory based on the selection criteria
        // now we need to select a random number between 0 and mSelectCount -1  and return that case to the front end
        int iRandom = 0;
        iRandom = rnd1.Next(mSelectCount);      //random number zero to no of entries in the array
        // return these values 
        srCaseID = mSelected[iRandom, 0];       // this is the case selected by random
        srLevel = mSelected[iRandom, 1];
        XmlElement root;
        root = Xml_IC.DocumentElement;
        root.SetAttribute("caseid", mSelected[iRandom,CF_CASEID]);
        root.SetAttribute("source", "random");
        root.SetAttribute("level", mSelected[iRandom,CF_LEVEL]);
        root.SetAttribute("hardest", mSelected[iRandom,CF_HARDEST]);
        root.SetAttribute("reason", mSelected[iRandom,CF_REASON]);
        root.SetAttribute("casename", mSelected[iRandom,CF_CASENAME]);
        root.SetAttribute("casedef", mSelected[iRandom,CF_CASEDEF]);
        root.SetAttribute("casesolution", mSelected[iRandom,CF_CASESOLN]);
        ProcessCase(ref Xml_IC);
    }
    public bool bIsTechInGroup(string sGroup, string sHardest)
    {   // is the hardest technique (HQ) in this puzzle in the group of techniques specified by the group eg T3
        // note must be same ranking as in DevOnly code
        string[] sCodeRanking  = { "NS", "HS", "NP", "HP", "NT", "HT", "LB", "LR", "LC", "NQ", "HQ", "CO", "XW", "SW", "CJ", "GR", "GO", "GP", "NI", "BF" };
        
      //<asp:ListItem Value="T1">Coloring</asp:ListItem>
      //<asp:ListItem Value="T2">X-Wing</asp:ListItem>
      //<asp:ListItem Value="T3">Swordfish</asp:ListItem>
      //<asp:ListItem Value="T4">Gordonian Rectangles</asp:ListItem> 

        if (sGroup == null) return false;
        if (sGroup == "T1" & sHardest == "CO") return true;
        if (sGroup == "T2" & sHardest == "XW") return true;
        if (sGroup == "T3" & sHardest == "SW") return true;
        if (sGroup == "T3" & sHardest == "CJ") return true;     //ADD CO-JOINED TO SW GROUP
        if (sGroup == "T4" & sHardest == "GR") return true;
        if (sGroup == "T4" & sHardest == "GO") return true;
        if (sGroup == "T4" & sHardest == "GP") return true;
        return false;
    }
    public void ProcessCase(ref XmlDocument Xml_IC)
    {
        // 9.1 input is case definiton of the form  000304100300290006700000300060003200000705000007100040002000005600019008005607000 exactly
        // process is solve all naked singles in the grid before returning, ie execute a AutoSolve
        // output is a standard XML_IC  document
        //
        XmlElement elem = null; 
        clsXML xmlLib = new clsXML();       // new xml library in V12
        int iCol, ip;
        int r, c, p;
        string sP;
        string sPlist;
        string sCellDefn;
        string sCaseDefn;
        int[, ,] sk = new int[10, 10, 10];

        sCaseDefn = xmlLib.getAttr(Xml_IC, "casedef");
        // extract the case definiton from the header

        // convert case definition to r,c,p format
        for (iCol = 0; iCol <= 80; iCol++)
        {
            r = iCol / 9;
            c = iCol - (r * 9);
            sP = sCaseDefn.Substring(iCol, 1);
            ip = Convert.ToInt16(sP);
            if (ip == 0)
            {
                for (p = 1; p <= 9; p++) sk[r, c, p] = 1;       //all candidates are possible
            } else
            {
                sk[r, c, ip] = 1;        //this cell already solved
            }
        }
        // do the auto solve here
        clsSolnSets mySolnSets = default(clsSolnSets);
        mySolnSets = new clsSolnSets();
        mySolnSets.QuickSolve(ref sk);
        // return the partially solved case

        // the default header is already set  eg
        // <case caseid ='none' source='cdf' level='none' hardest='none' reason='none' casename='none' casedef='none' casesolution='none'> </case>
        XmlNode root = Xml_IC.DocumentElement;
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                sPlist = "";
                for (p = 1; p <= 9; p++)
                {
                    if (sk[r, c, p] == 1)
                    {
                        sPlist = sPlist + p;
                        //build list of possibles
                    }
                }
                //new format for xml_cs is  'r c 12345'  not 'xx 12345'
                sCellDefn = Convert.ToString(r) + " " + Convert.ToString(c) + " " + sPlist;
                //Create a new node.
                elem = Xml_IC.CreateElement("cell");
                elem.InnerText = sCellDefn;
                //Add the node to the document.
                root.AppendChild(elem);
            }
        }
    }

    public void GetAllCases(ref string[,] sAllCases , ref int iCaseCnt)
    {
        // Volume testing interface, ONLY USED IN DEVELOPMENT
        // entire case file is not cached in production
        // new format from May 2012
        // the case definitons are now in a text file (and not the database)
        // see document for formats
        // get the entire list as pass it back for volume testing
        string sPath = null;
        string[] sFields = null;
        char cDelim = ',';
        sPath = Server.MapPath(".") + mCasePathName;
        iCaseCnt = 0;
        foreach (string sLineIn in File.ReadLines(sPath))
        {
            if (sLineIn.Length >= 100)              // ignore null or incomplete lines
            {
                sFields = sLineIn.Split(cDelim);   // new form of case definition file
                if (sFields[5].Length == 81)        // rudimentary check make sure the definitons is the correct length
                {
                    sAllCases[iCaseCnt, 0] = sFields[0];    //case id
                    sAllCases[iCaseCnt, 1] = sFields[1];    // difficulty level 1-5
                    sAllCases[iCaseCnt, 2] = sFields[2];    // hardest technique
                    sAllCases[iCaseCnt, 3] = sFields[3];    // case status
                    sAllCases[iCaseCnt, 4] = sFields[4];    // case name
                    sAllCases[iCaseCnt, 5] = sFields[5];    // case definitions
                    iCaseCnt = iCaseCnt + 1;
                }
            }
        }
    }
    public string sGetAboutHTML                 //this is a property
    {
        // Content for Help command :  
        // Convert html document to string for inclusing in asp control
        get {
            string sHTML = null;
            string sPath;
            
            sPath = Server.MapPath(".") + mAboutPathName;

            foreach (string sLineIn in File.ReadLines(sPath))
            {
                sHTML = sHTML + sLineIn;
            }
        return sHTML;}
    }
    public void GetFreqAnalysis(clsSK rmySK, ref string sFreqMess)       //this is a method
    {
        // Input is a clsSK object, output is the frequency html message with scalar counts updated
        // This method returns Content for Frequency Analysis in Get Hints command:
        // Convert html document to string for inclusion in asp control
        string sHTML = null;
        string sPath;
        sPath = Server.MapPath(".") + mFreqAnalysisPathName;

        foreach (string sLineIn in File.ReadLines(sPath))
        {
            sHTML = sHTML + sLineIn;
        }
      // replace the progress bar values with the p value counts
      // eg     progress bar 1    will be set to 8 if there are 8 solved 1's
     //                     3    will be set to 2 if there are 2 solved 3's
        sHTML = sHTML.Replace("111", rmySK.sGetPCount(1));
        sHTML = sHTML.Replace("222", rmySK.sGetPCount(2));
        sHTML = sHTML.Replace("333", rmySK.sGetPCount(3));
        sHTML = sHTML.Replace("444", rmySK.sGetPCount(4));
        sHTML = sHTML.Replace("555", rmySK.sGetPCount(5));
        sHTML = sHTML.Replace("666", rmySK.sGetPCount(6));
        sHTML = sHTML.Replace("777", rmySK.sGetPCount(7));
        sHTML = sHTML.Replace("888", rmySK.sGetPCount(8));
        sHTML = sHTML.Replace("999", rmySK.sGetPCount(9));
        sFreqMess = sHTML;
    }
        public void GetDailyCase(ref XmlDocument Xml_IC, ref string srCaseID, ref string srLevel)
    {
        // Dec 31, 2012
        // Use with the NewCaseDefinitions.txt file
        // Just get the record n for the nth day of the year.  There are over 500 OKINPROD case
        //  The same daily puzzle will be presented to all users on a given day
        //      it can be any level, or any technique
        //
        string sPath = null;
        string[] sFields = null;
        char cDelim = ',';
        string sCaseDefinition = "";
        bool bCaseFound = false;
        DateTime myDate;
        string[] mDaily = new string[6];     

        myDate = DateTime.Now;
        int iMonth = myDate.Month;
        int iDay = myDate.Day;
        //
        // algorythm produces a number from 0 to 371 based on the month and day of the year. Some numbers
        // in this range are never encountered since I assume all months have 31 days.  The number will be the
        // same number every year.  
        //
        int iDailyCaseID = (iMonth -1) * 31 + (iDay -1);
        if (iDailyCaseID < 0 | iDailyCaseID > 371) iDailyCaseID = 0;   //OR condition bullet proof algorythm 

        int iRecID = 0;     // 0 based record number in the file
        sPath = Server.MapPath(".") + mCasePathName; 

        foreach (string sLineIn in File.ReadLines(sPath))
        {
            if (sLineIn.Length >= 100)              // ignore null or incomplete lines
            {
                sFields = sLineIn.Split(cDelim);   // new form of case definition file
                if (sFields[CF_REASON] == "OKINPROD")        // rudimentary check make sure the definitons is the correct length
                {
                    // just get the nth record in the file, there must be at least 372 (0...371)  OKINPROD records in the case file
                    if (iRecID == iDailyCaseID)         
                    {
                        srCaseID = sFields[CF_CASEID];
                        srLevel = sFields[CF_LEVEL];       // bug fixed was hardest
                        sCaseDefinition = sFields[CF_CASEDEF];
                        bCaseFound = true;
                    }
                    iRecID++;
                }
            }
            if (bCaseFound) break;      // stop reading the case file
        }
        // We have the correct subset of the case file in memory based on the value of the current day of the year
        // 9.1 the current case definiton is 2 dimensional, position (0-80 and, p value 0 or p)
        // 9.1 create an 3 dimensional array like mSK(r,c,p)
        // 9.1 then create the xml to be returned auto solved.
        // v12 - attributes added to root definition of input case
        XmlElement root;
        root = Xml_IC.DocumentElement;
        root.SetAttribute("caseid", sFields[CF_CASEID]);
        root.SetAttribute("source", "daily");
        root.SetAttribute("level", sFields[CF_LEVEL]);
        root.SetAttribute("hardest", sFields[CF_HARDEST]);
        root.SetAttribute("reason", sFields[CF_REASON]);
        root.SetAttribute("casename", sFields[CF_CASENAME]);
        root.SetAttribute("casedef", sFields[CF_CASEDEF]);
        root.SetAttribute("casesolution",sFields[CF_CASESOLN]);
        ProcessCase(ref Xml_IC);
    }
    private void myDebug(string sMess)
    {
        //Redirect all Output Window text to the Immediate Window" checked under the menu Tools > Options > Debugging > General.
        //System.Diagnostics.Debug.WriteLine(sMess);
    }
}
