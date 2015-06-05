using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
// This class is the workhorse of the Soduku solving engine and is the only interface to the various techniques
// that are used to solved a puzzle. Think of it as pure Sudoku solving and nothing to do with the user interface 
public class clsSK
{
    const int MAXITERATIONS = 9999999;
    // rows are 0 to 8, columns are 0 to 8
    //9 elements addressed by 0 to 8
    int[, ,] mSK = new int[10, 10, 10];
    clsStats myStats = new clsStats();

    int mNoOfChanges;
    public int gCurrentMethod;
    const string COJO_METHOD_COLORING = "COLO";
    const string COJO_METHOD_COJOINS = "COJO";
    //returned as property
    string mReasonCode;
    bool mbIsPuzzleValid;
    bool mUseBruteForce = true;
    bool mUseNishio = true;
    int mIterationCnt;
    bool mbIsPuzzleSolved;
    //

    public string ReasonCode
    {
        get { return mReasonCode; }
    }
    public bool IsPuzzleValid
    {
        get { return mbIsPuzzleValid; }
    }
    public bool IsPuzzleSolved
    {
        get { return mbIsPuzzleSolved; }
    }
    public int IterationCount
    {
        set { mIterationCnt = value; }
    }
    public bool UseBruteForce
    {
        set { mUseBruteForce = value; }
    }

    public bool UseNishio
    {
        set { mUseNishio = value; }
    }

    public void LoadFromXML(ref XmlDocument XML_IC)
    {
        // 
        clsSolnSets mySolnSets = new clsSolnSets();
        mSK.Initialize();
        ConvertXMLtoArray(ref XML_IC);
        mbIsPuzzleValid = mySolnSets.IsThisCaseValid(mSK);
        //this object only used for this purpose
        //tell front end case is invalid, ie something wrong with user input.
        mbIsPuzzleSolved = false;
        if (mbIsPuzzleValid)
        {
            mReasonCode = "VALIDINPUT";
            if (sIsSolved() == "Yes")
            {
                mbIsPuzzleSolved = true;
                //new feature to simplify front end
            }
        }
        else
        {
            mReasonCode = "INVALIDINPUT";
        }


    }

    public void LoadFromArray(int[, ,] skArg)
    {
        // nisho has already prepared the array, just copy it to this object
        short r = 0;
        short c = 0;
        short p = 0;
        clsSolnSets mySolnSets = new clsSolnSets();
        mSK.Initialize();
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                for (p = 1; p <= 9; p++)
                {
                    mSK[r, c, p] = skArg[r, c, p];
                }
            }
        }
        mbIsPuzzleValid = mySolnSets.IsThisCaseValid(mSK);
        //this object only used for this purpose
        //tell front end case is invalid, ie something wrong with user input.
        if (mbIsPuzzleValid)
        {
            mReasonCode = "VALIDINPUT";
        }
        else
        {
            mReasonCode = "INVALIDINPUT";
        }
    }
    public void SolveIT()
    {
        //
        //based on the number of iterations to perform, count set via a property
        //
        do
        {
            mNoOfChanges = 0;
            NewIteration();
            mIterationCnt = mIterationCnt - 1;
        } while (!(mNoOfChanges == 0 | mIterationCnt == 0));
    }

    public XmlDocument GetXMLResults()
    {
        //
        //gather all info and return to front end via an xml document/string
        //
        //<cell>3 4 157<\cell>     2 values for r 3 col 4   
        //<cell>8 8 9<\cell>       1 value for cell on bottom right
        //<stats>xx nnnn<\stats>  eg xw 0024  24 cells changed via x-wing method
        //<solved>yes or no<\solved>
        //<changes>0000<\changes>     total changes made this iteration
        //<reason>Invalid User Input<\reason>          reason why puzzle invalid, (user input error or >1 solution
        //                           or reason why brute force analysis stopped
        //<valid>yes or no<\valid>
        XmlDocument XML_CS = new XmlDocument();
        XmlElement elem = null;
        XmlNode root = null;
        string sPlist = null;
        string sValue = null;
        int r = 0;
        int c = 0;
        int p = 0;

        XML_CS.LoadXml("<case caseid ='none' source='cdf' level='none' hardest='none' reason='none' casename='none' casedef='none' casesolution='none'> </case>"); 
        root = XML_CS.DocumentElement;
        //clone the text boxes to a integer array
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                sPlist = "";
                for (p = 1; p <= 9; p++)
                {
                    if (mSK[r, c, p] == 1)
                    {
                        sPlist = sPlist + p;
                        //build list of possibles
                    }
                }
                //new format for xml_cs is  'r c 12345'  not 'xx 12345'
                sValue = Convert.ToString(r) + " " + Convert.ToString(c) + " " + sPlist;   //csNote
                //Create a new node.
                elem = XML_CS.CreateElement("cell");
                elem.InnerText = sValue;
                //Add the node to the document.
                root.AppendChild(elem);
            }
        }
        myStats.GetXMLStats(ref XML_CS);        //append statistics
        //
        //total changes, create node and append to document
        //
        sValue = String.Format("{0:0000}", mNoOfChanges);
        elem = XML_CS.CreateElement("changes");
        elem.InnerText = sValue;
        root.AppendChild(elem);
        //
        // solved or not flag, create node and append to document
        //
        sValue = sIsSolved();
        elem = XML_CS.CreateElement("status");
        elem.InnerText = sValue;
        root.AppendChild(elem);
        //
        // case valid or not flag, create node and append to document
        //
        //tranform to text
        if (mbIsPuzzleValid)
        {
            sValue = "Yes";
        }
        else
        {
            sValue = "No";
        }
        elem = XML_CS.CreateElement("valid");
        elem.InnerText = sValue;
        root.AppendChild(elem);
        //
        // reason why puzzle is not valid create node and append to document
        //
        sValue = mReasonCode;
        elem = XML_CS.CreateElement("reason");
        elem.InnerText = sValue;
        root.AppendChild(elem);

        //all done return the document
        return XML_CS;
    }

    private void ConvertXMLtoArray(ref XmlDocument XML_IC)
    {
        int r = 0;
        int c = 0;
        int p = 0;

        string sValue = null;
        string sCandidates = null;
        string sP = null;
        int[,] intCells = new int[9,9];
        intCells.Initialize();
        //
       
        XmlNodeList elemList = XML_IC.GetElementsByTagName("cell");
        int i = 0;
        //
        //each cell is represented by the form "r c 12357"  set r,c p values to 12357
        //
        for (i = 0; i <= elemList.Count - 1; i++)
        {
            sValue = elemList[i].InnerXml;
            r = Convert.ToInt16(sValue.Substring(0, 1));     //csNote
            c = Convert.ToInt16(sValue.Substring(2, 1));
            sCandidates = sValue.Substring(4);
            //to end of string
            for (p = 1; p <= 9; p++)
            {
                sP = Convert.ToString(p);
                if (sCandidates.Contains(sP))
                {
                    mSK[r, c, p] = 1;
                    intCells[r, c] = 1;         //csNote new feature, if no cell specified default to all p values
                }
            }

        }
        // if in xml the cell is not specified or blank default it to 123456789
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                if (intCells[r, c] == 0)
                {
                    for (p = 1; p <= 9; p++)
                    {
                        mSK[r, c, p] = 1;
                    }  //next p
                }
            } //next col
        } //next row
    }
    public void NewIteration()
    {
        // Methods to convert/test
        // Solution Sets - work in progress
        // solve one iteration of the puzzle, solve once really means just one iteration, not 1,2,4,8 as before   
        //

        clsSolnSets mySolnSets = default(clsSolnSets);
        clsNineByNine myNineByNine = default(clsNineByNine);
        clsColoring myColoring = default(clsColoring);
        clsGordonian myGordonian = default(clsGordonian);
        clsNishio myNishio = default(clsNishio);
        clsBrute myBrute = default(clsBrute);
        int iOldChangeCnt = 0;
        int p = 0;
        //
        //solve all row, column or box's (solution sets) independently
        //
        mySolnSets = new clsSolnSets();

        iOldChangeCnt = mNoOfChanges;
        mySolnSets.NewIteration(ref mSK, mIterationCnt);
        mNoOfChanges = mNoOfChanges + mySolnSets.ChangeCount;
        if (mySolnSets.ChangeCount > 0)
        {
            myStats.UpdateStats("NS", mySolnSets.NakedSingles);
            myStats.UpdateStats("HS", mySolnSets.HiddenSingles);
            myStats.UpdateStats("NP", mySolnSets.NakedPairs);
            myStats.UpdateStats("NT", mySolnSets.NakedTriples);
            myStats.UpdateStats("NQ", mySolnSets.NakedQuads);
            myStats.UpdateStats("HP", mySolnSets.HiddenPairs);
            myStats.UpdateStats("LB", mySolnSets.LockedBoxes);
            myStats.UpdateStats("LR", mySolnSets.LockedRows);
            myStats.UpdateStats("LC", mySolnSets.LockedCols);
            myStats.UpdateStats("HT", mySolnSets.HiddenTriples);
            myStats.UpdateStats("HQ", mySolnSets.HiddenQuads); 
        }
        mySolnSets = null;
        //for clarity in single stepping, don't change too much at once
        if (mNoOfChanges > iOldChangeCnt) return;
        //
        // look for Gordonian Rectangles
        //
        iOldChangeCnt = mNoOfChanges;
        myGordonian = new clsGordonian();
        myGordonian.Gordonian(ref mSK);
        switch (myGordonian.Method)
        {
            case "GRNP":
                myStats.UpdateStats("GR", myGordonian.ChangeCount);
                break;
            case "GROS":
                myStats.UpdateStats("GO", myGordonian.ChangeCount);
                break;
            case "GRPP":
                myStats.UpdateStats("GP", myGordonian.ChangeCount);
                break;
        }
        mNoOfChanges = mNoOfChanges + myGordonian.ChangeCount;
        myGordonian = null;
        if (mNoOfChanges > iOldChangeCnt) return;  //for clarity in single stepping, don't change too much at once
        //
        // X-wing Look at the entire matrix
        //
        iOldChangeCnt = mNoOfChanges;
        for (p = 1; p <= 9; p++)
        {
            myNineByNine = new clsNineByNine();
            var _with3 = myNineByNine;
            _with3.pValue = p;
            _with3.BuildMatrix(ref mSK);
            _with3.Xwing(ref mSK);
            myStats.UpdateStats("XW", _with3.ChangeCount);
            mNoOfChanges = mNoOfChanges + _with3.ChangeCount;
            myNineByNine = null;
        }
        //for clarity in single stepping, don't change too much at once
        if (mNoOfChanges > iOldChangeCnt) return;
        //
        // SwordFish - look at entire matrix
        //
        iOldChangeCnt = mNoOfChanges;
        for (p = 1; p <= 9; p++)
        {
            myNineByNine = new clsNineByNine();
            var _with4 = myNineByNine;
                _with4.pValue = p;
                _with4.BuildMatrix(ref mSK);
                _with4.SwordFish(ref mSK);
                myStats.UpdateStats("SW", _with4.ChangeCount);
            mNoOfChanges = mNoOfChanges + _with4.ChangeCount;
            myNineByNine = null;
        }
        if (mNoOfChanges > iOldChangeCnt) return; //for clarity in single stepping, don't change too much at once
        //
        // Coloring, look at entire matrix, one p value only look for a chain of co-jointed pairs
        //
        iOldChangeCnt = mNoOfChanges;
        for (p = 1; p <= 9; p++)
        {
            myColoring = new clsColoring();
            var _with5 = myColoring;
                _with5.pValue = p;
                _with5.BuildMatrix(ref mSK);
                _with5.COJOChains(ref mSK);
            if (myColoring.Method == COJO_METHOD_COJOINS)
            {
                myStats.UpdateStats("CJ", myColoring.ChangeCount);
                //will be one or the other
            }
            else if (myColoring.Method == COJO_METHOD_COLORING)
            {
                myStats.UpdateStats("CO", myColoring.ChangeCount);
                //but not both
            }
            mNoOfChanges = mNoOfChanges + _with5.ChangeCount;
            //only solve for 1 p value for clarity in single stepping
            if (myColoring.ChangeCount != 0) break; 
            myColoring = null;
        }
        if (mNoOfChanges > iOldChangeCnt) return; //for clarity in single stepping, don't change too much at once
        //
        // Now try Nishio, looking for naked pairs and guessing as to which one is correct
        //
        if (mUseNishio)
        {
            myNishio = new clsNishio();
            myNishio.TryNishio(ref mSK);
            mNoOfChanges = mNoOfChanges + myNishio.ChangeCount;
            myStats.UpdateStats("NI", myNishio.ChangeCount);
            myNishio = null;
        }

        if (mNoOfChanges > iOldChangeCnt) return;  //for clarity in single stepping, don't change too much at once
        //
        //now try brute force (if requested) if all known methods have failed
        //note Nishio will create another instance of clsSK.. very cool
        //
        if (mUseBruteForce)
        {
            if (mNoOfChanges == 0 & sIsSolved() == "No")
            {
                myBrute = new clsBrute();
                myBrute.Analyze(ref mSK);
                if (myBrute.IsSolveable)
                {
                    mbIsPuzzleValid = true;
                    mReasonCode = myBrute.ReasonCode;
                    //one solution, found by brute force
                    myStats.UpdateStats("BF", myBrute.NoOfChanges);
                    mNoOfChanges = mNoOfChanges + myBrute.NoOfChanges;
                }
                else
                {
                    mbIsPuzzleValid = false;
                    mReasonCode = myBrute.ReasonCode;
                    //probably more than 1 solution
                }
            }
        }
    }
//xxxx
    public string sIsSolved()
    {
        string functionReturnValue = null;
        //
        // Need to check all solution sets
        // each and every one  must contain the numbers 1..9
        //
        int r = 0;
        int c = 0;
        int p = 0;
        int pTotals = 0;
        int[] pCnts = new int[10];
        //make sure we have 9 occurances of every number 1..9
        int[,] miniSK = new int[9, 9];
        //copy of grid looking at only scalars
        int iNoOfPValues = 0;
        int iPValue = 0;
        //define the  top left corner of each box in the mini grid
        int[,] iBoxMap = { { 0, 0 }, { 0, 3 }, { 0, 6 }, { 3, 0 }, { 3, 3 }, { 3, 6 }, { 6, 0 }, { 6, 3 }, { 6, 6 } };
        int iBox = 0;
        pCnts.Initialize();
        functionReturnValue = "Yes";
        pTotals = 0;
        //make a copy of the grid with only solved p values
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                iNoOfPValues = 0;
                for (p = 1; p <= 9; p++)
                {
                    if (mSK[r, c, p] == 1)
                    {
                        iNoOfPValues = iNoOfPValues + 1;
                        iPValue = p;
                        pTotals = pTotals + p;
                    }
                }
                if (iNoOfPValues == 1)
                {
                    miniSK[r, c] = iPValue;
                    //clone grid to with only p values
                }
                else
                {
                    functionReturnValue = "No";
                    return functionReturnValue;
                    //still choices to make therefore unsolved
                }
            }
        }
        //simple (original) test only proves puzzle is not valid, not the inverse.
        if (pTotals != 405)
        {
            functionReturnValue = "No";
            return functionReturnValue;
        }
        //
        //
        //make sure 1 2 3 4 5 6 7 8 9 in every row 
        //
        for (r = 0; r <= 8; r++)
        {
            for (p = 1; p <= 9; p++)
            {
                pCnts[p] = 0;
                //array.initialize did not work at run time
            }
            for (c = 0; c <= 8; c++)
            {
                pCnts[miniSK[r, c]] = pCnts[miniSK[r, c]] + 1;
            }
            for (p = 1; p <= 9; p++)
            {
                if (pCnts[p] != 1)
                {
                    functionReturnValue = "No";
                    return functionReturnValue;
                }
            }
        }
        //make sure 1 2 3 4 5 6 7 8 9 in every column
        for (c = 0; c <= 8; c++)
        {
            for (p = 1; p <= 9; p++)
            {
                pCnts[p] = 0;
                //array.initialize did not work at run time
            }
            for (r = 0; r <= 8; r++)
            {
                pCnts[miniSK[r, c]] = pCnts[miniSK[r, c]] + 1;
            }
            for (p = 1; p <= 9; p++)
            {
                if (pCnts[p] != 1)
                {
                    functionReturnValue = "No";
                    return functionReturnValue;
                }
            }
        }
        //
        //still not good enough, need to check every box
        //
        for (iBox = 0; iBox <= 8; iBox++)
        {
            for (p = 1; p <= 9; p++)
            {
                pCnts[p] = 0;
            }
            for (r = iBoxMap[iBox, 0]; r <= iBoxMap[iBox, 0] + 2; r++)
            {
                for (c = iBoxMap[iBox, 1]; c <= iBoxMap[iBox, 1] + 2; c++)
                {
                    pCnts[miniSK[r, c]] = pCnts[miniSK[r, c]] + 1;
                }
            }
            for (p = 1; p <= 9; p++)
            {
                if (pCnts[p] != 1)
                {
                    functionReturnValue = "No";
                    return functionReturnValue;
                }
            }
        }
        return functionReturnValue;
    }
//xxxx
//=====
    public string sGetPCount(int viP)
        //
        // returns the number of times the passed p value occurs in the grid
        //  eg   a=sGetPCount("8") returns 3 if there are 3 8's in the grid which have been solved
        //       a=sGetPCount("2") returns 0 if there are no 2's solved in the grid
        //
    {
        string sfunctionReturnValue = null;
        //
        // Need to check all solution sets
        // each and every one  must contain the numbers 1..9
        //
        int r = 0;
        int c = 0;
        int p = 0;
        int ipCnts = 0;
        bool bIsPaCandidate = false;
        int iSolvedPCnt = 0;
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                bIsPaCandidate = false;
                ipCnts = 0;
                for (p = 1; p <= 9; p++)        //check all the candidates
                {
                    if (mSK[r, c, p] == 1) {ipCnts = ipCnts + 1;}
                    if (mSK[r, c, viP] == 1) { bIsPaCandidate = true; }
                }
                if (ipCnts == 1 && bIsPaCandidate) {iSolvedPCnt = iSolvedPCnt + 1;}
            }
        }
        sfunctionReturnValue = Convert.ToString(iSolvedPCnt);
        return sfunctionReturnValue;
    }


    //=====
    private void myDebug(string sMess)
    {
        //Redirect all Output Window text to the Immediate Window" checked under the menu Tools > Options > Debugging > General.
        //System.Diagnostics.Debug.WriteLine(sMess);
    }
}
