using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
// This technique is descibed in the book Mensa Guide to Solving Sudoku by Peter Gordon, 2006
// Nishio is a fancy way of saying guess the answer. If there are only two candidates in a cell then
// it is a valid to technique to try 1 value and see if it works, if it doesn't then the solution is the other value.
//
public class clsNishio
{
    // For each pair pick the first cell containing p1 and p2,
    // choose p1 and solve with no brute force - record results
    // choose p2 and solve with no brute force - record results
    // if    p1  results in solved with no errors and p2 is unsolveable then p1 is it   ***
    //       p2                                       p2                     p2 is it   ***
    //       p1 and p2 are solved then there are two solutions to this puzzle ***
    //       neither p1 or p2 are solved then this puzzle is not solveable by this technique (possibly) try next pair!!
    //  exit and return to calling routine  the first time conditions *** is met

    // local copy of the SK array
    //9 elements addressed by 0 to 8
    int[, ,] mSK = new int[10, 10, 10];
    const string NEWLINE = "\n";
    private int mChangeCount;
    const int MAXITERATIONS = 9999999;
    // analyze the initial puzzle to help choosing the naked pairs to work with
    // 
    private int[,] mStats = new int[82, 10];
    private int mStatsCnt;
    private enum mS
    {
        p1 = 0,
        //lowest number pair eg 2
        p2 = 1,
        //highted number pair eg 4, naked pair is 24
        r = 2,
        //row of the first cell containing p1 p2
        c = 3,
        //col of the first cell containing p1 p2
        CellCnt = 4,
        //no of cells containing this pair
        pStats = 5,
        //number of cells where wither p1 or p2 is the only candidate (helps decide priority)
        Ranking = 6,
        //determines the order that the pairs will be attempted - highest ranked 1st.
        p1Result = 7,
        //result of solving when choosing p1
        p2Result = 8
        //result of solving when choosing p2
    }

    public int ChangeCount
    {
        get { return mChangeCount; }
    }

    public void TryNishio(ref int[, ,] skArg)
    {
        // nishio says if all else fails and we have a cell with a naked pair, then pick one and try it!
        //1 get statistics for current puzzle
        //2 build a table, one row per naked pair and determine a ranking to help decide what order to try (could be 10 or 20 rows)
        //3 sort the table by ranking
        //4 process each pair, solving the puzzle with p1 and then p2 and then comparing the results
        //5  quit when the 1st one used
        //note this is invoked by clsSK and this object also invokes more clsSK objects!!!
        //
        int iStatPtr = 0;
        int NewPValue = 0;
        bool bFoundSolution = false;
        int r = 0;
        int c = 0;

        mStats.Initialize();
        mStatsCnt = 0;
        mChangeCount = 0;
        SaveCopyofSK(skArg);
        //use a local copy only
        CalcPStats();
        //build local table describing the pairs found
        if (mStatsCnt == 0)
        {
            return;
            //no pairs found at all
        }

        RankCandidates();
        //try the pairs most likely to result in a solution first

        //    Call DumpStats()
        //process each row in the stats table, looking for a solution, quit as soon as one solution is found
        bFoundSolution = false;
        for (iStatPtr = 0; iStatPtr <= mStatsCnt - 1; iStatPtr++)
        {
            if (bProcessNakedPair(ref iStatPtr, ref NewPValue))
            {
                bFoundSolution = true;
                break; 
            }
        }
        if (bFoundSolution)
        {
            r = mStats[iStatPtr, (int)mS.r];
            c = mStats[iStatPtr, (int)mS.c];
            myDebug("Solution found set " + r + "," + c + " TO " + NewPValue);
            KeepCandidate(ref skArg, ref r, ref c, ref NewPValue);
            //update calling object. this change is not a trial
        }
        else
        {
            mChangeCount = 0;
            //tell front end no changes
        }
    }
    private bool bProcessNakedPair(ref int vStatPtr, ref int vNewPvalue)
    {
        bool functionReturnValue = false;
        // process the pair identified by the pointer into the statistics table 
        int[, ,] pCase = new int[10, 10, 10];
        XmlDocument Xml_CS = new XmlDocument();
        //Case Solution
        string s1Status = null;
        string s1ValidFlag = null;
        string s1Reason = null;
        string s2Status = null;
        string s2ValidFlag = null;
        string s2Reason = null;
        int p1 = 0;
        int p2 = 0;
        int r = 0;
        int c = 0;
        string sMess = null;
        functionReturnValue = false;
        //extract the case to try from the stats table
        p1 = mStats[vStatPtr, (int)mS.p1];
        r = mStats[vStatPtr, (int)mS.r];
        c = mStats[vStatPtr, (int)mS.c];
        // p1 Try the first choice in the pair ===================================
        CopyGrid(ref pCase, r, ref c, p1);
        RunTrialSK(ref pCase, ref Xml_CS);
        GetResults(ref Xml_CS, ref s1Status, ref s1ValidFlag, ref s1Reason);
        sMess = "p1 Set " + r + "," + c + " to " + p1 + " status=" + s1Status + " Valid=" + s1ValidFlag + " Reason=" + s1Reason;
        myDebug(sMess);
        // p2 Try the first choice in the pair ===================================
        p2 = mStats[vStatPtr, (int)mS.p2];
        CopyGrid(ref pCase, r, ref c, p2);
        RunTrialSK(ref pCase, ref Xml_CS);
        GetResults(ref Xml_CS, ref s2Status, ref s2ValidFlag, ref s2Reason);
        sMess = "p2 Set " + r + "," + c + " to " + p2 + " status=" + s2Status + " Valid=" + s2ValidFlag + " Reason=" + s2Reason;
        myDebug(sMess);
        // RESULTS =====================
        if (s1ValidFlag == "Yes" & s2ValidFlag == "Yes")
        {
            //case must work for the one p value and not the other
            if (s1Status == "Yes" & s2Status == "No")
            {
                //we know p1 is the solution                     'because we may have a case where there are two solutions
                functionReturnValue = true;
                vNewPvalue = p1;
            }

            if (s2Status == "Yes" & s1Status == "No")
            {
                //we know p2 is the solution
                functionReturnValue = true;
                vNewPvalue = p2;
            }
        }
        return functionReturnValue;
    }

    private void RunTrialSK(ref int[, ,] vpCase, ref XmlDocument XML_CS)
    {
        clsSK mySKCase = new clsSK();

        mySKCase.LoadFromArray(vpCase);
        mySKCase.IterationCount = MAXITERATIONS;
        mySKCase.UseBruteForce = false;
        //NEED A SOLUTION THAT DOES NOT NEED BRUTE FORCE
        mySKCase.UseNishio = false;
        //DONT WANT TO GET INTO A RECURSIVE LOOP
        mySKCase.SolveIT();
        XML_CS = mySKCase.GetXMLResults();
        mySKCase = null;
    }
    private void GetResults(ref XmlDocument myXML, ref string vStatus, ref string vValid, ref string vReason)
    {
        //
        //       type is solve once, solve all or invalid
        // input document can be either type xml_CS or xml_IC, but only type XML_CS is used
        //
        //only found in xml_cs documents
        //status = Yes solved, No  = not solved
        //valid flag  Yes puzzle is valid, No not valid for reason in reason code
        //reasons are
        //   "MAXTABLESIZE", "2SOLUTIONS","0SOLUTIONS","1SOLUTION","VALIDINPUT", "INVALIDINPUT"
        clsXML xmlLib = new clsXML();       // new xml library in V12

        vStatus = xmlLib.getNodeValue(myXML, "status");
        //solved or unsolved at the moment
        vValid = xmlLib.getNodeValue(myXML, "valid");
        //can be solved if valid
        vReason = xmlLib.getNodeValue(myXML, "reason");
        //reason it can't be solved.

    }

    private void CopyGrid(ref int[, ,] vToSK, int vR, ref int vC, int vP)
    {
        //copy the original sk grid to the new array, and force r,c to be the value vP
        int r = 0;
        int c = 0;
        int p = 0;
        vToSK.Initialize();
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                if (r == vR & c == vC)
                {
                    vToSK[vR, vC, vP] = 1;
                }
                else
                {
                    for (p = 1; p <= 9; p++)
                    {
                        vToSK[r, c, p] = mSK[r, c, p];
                    }
                }
            }
        }

    }
    private void RankCandidates()
    {
        //
        //set the ranking such that the pairs with highest probility of being solved are atempted first
        //
        //try this algorythm  2 times the number of pairs of the same candidates plus the number
        //of solved cells containing either candidate.
        //   eg 5 cells   5      6        46         46   PAIR 46 RANK IS (2 * 2 ) + 1 = 5
        //a typical high ranking number is over 9 and usually around 12 or 13
        //
        int iStatPtr = 0;
        int j = 0;
        int iTemp = 0;
        for (iStatPtr = 0; iStatPtr <= mStatsCnt - 1; iStatPtr++)
        {
            mStats[iStatPtr, (int)mS.Ranking] = mStats[iStatPtr, (int)mS.CellCnt] * 2 + mStats[iStatPtr, (int)mS.pStats];
        }
        //now bubble sort the array based on the ranking, highest one firstang
        bool bChanges = false;
        if (mStatsCnt == 1)
        {
            return;
            //only one entry therefore sorted
        }
        do
        {
            bChanges = false;
            //0,1,2   valid are 0,1,2,3 
            for (iStatPtr = 0; iStatPtr <= mStatsCnt - 2; iStatPtr++)
            {
                if (mStats[iStatPtr, (int)mS.Ranking] < mStats[iStatPtr + 1, (int)mS.Ranking])
                {
                    //swap the two rows, the 2nd one needs to come first
                    for (j = 0; j <= 6; j++)
                    {
                        iTemp = mStats[iStatPtr, j];
                        mStats[iStatPtr, j] = mStats[iStatPtr + 1, j];
                        mStats[iStatPtr + 1, j] = iTemp;
                        bChanges = true;
                    }
                }
            }

        } while (!(bChanges == false));

    }
    private void CountPairs(ref int rFound, ref int cFound)
    {
        //this cell (identified in the arguments) contains a naked pair
        //extract p1 and p2 before we start finding how many cells contain this pair
        int p = 0;
        int p1 = 0;
        int p2 = 0;
        p1 = -1;
        for (p = 1; p <= 9; p++)
        {
            //we know there are only 2 p values
            if (mSK[rFound, cFound, p] == 1)
            {
                if (p1 == -1)
                {
                    p1 = p;
                }
                else
                {
                    p2 = p;
                }
            }
        }
        //now add this pair to the stats table, no dup's allowed
        int iStatPtr = 0;
        bool bAlreadyThere = false;
        for (iStatPtr = 0; iStatPtr <= mStatsCnt; iStatPtr++)
        {
            if (mStats[iStatPtr, (int)mS.p1] == p1 & mStats[iStatPtr, (int)mS.p2] == p2)
            {
                mStats[iStatPtr, (int)mS.CellCnt] = mStats[iStatPtr, (int)mS.CellCnt] + 1;
                //no of cells containing this pair
                bAlreadyThere = true;
            }
        }
        if (!bAlreadyThere)
        {
            mStats[mStatsCnt, (int)mS.p1] = p1;
            mStats[mStatsCnt, (int)mS.p2] = p2;
            mStats[mStatsCnt, (int)mS.CellCnt] = 1;
            //no of cells containing this pair
            mStats[mStatsCnt, (int)mS.r] = rFound;
            //location of first cell containing this pair
            mStats[mStatsCnt, (int)mS.c] = cFound;
            mStatsCnt = mStatsCnt + 1;
        }
    }
    private void CalcPStats()
    {
        // build table of statics to help decide what naked pairs to try Nishio on
        int iStatPtr = 0;
        int p1 = 0;
        int p2 = 0;
        int r = 0;
        int c = 0;
        int pCnt = 0;
        int p = 0;
        // interogate the source case to see how many pairs we have
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                pCnt = 0;
                for (p = 1; p <= 9; p++)
                {
                    pCnt = pCnt + mSK[r, c, p];
                }
                if (pCnt == 2)
                {
                    CountPairs(ref r, ref c);
                    //know we have a pair but not which ones
                }
            }
        }
        //for every pair scan the entire grid to determine which pair to start solving first
        for (iStatPtr = 0; iStatPtr <= mStatsCnt - 1; iStatPtr++)
        {
            p1 = mStats[iStatPtr, (int)mS.p1];
            p2 = mStats[iStatPtr, (int)mS.p2];
            for (r = 0; r <= 8; r++)
            {
                for (c = 0; c <= 8; c++)
                {
                    //ie the cell has been solved
                    if (CandidateCount(ref r, ref c) == 1)
                    {
                        //either p1 or p2 has been solved
                        if (mSK[r, c, p1] == 1 | mSK[r, c, p2] == 1)
                        {
                            mStats[iStatPtr, (int)mS.pStats] = mStats[iStatPtr, (int)mS.pStats] + 1;
                        }
                    }
                }
            }
        }
    }
    private int CandidateCount(ref int rArg, ref int cArg)
    {
        int functionReturnValue = 0;
        //
        //returns the number of possible solutions for the specified cell
        //warning - same code exists in other classes
        //
        functionReturnValue = 0;
        int p = 0;
        for (p = 1; p <= 9; p++)
        {
            functionReturnValue = functionReturnValue + mSK[rArg, cArg, p];
            //p is 1 one it is a possibility
        }
        return functionReturnValue;

    }
    private void DumpStats()
    {
        int iStatPtr = 0;
        string sMess = null;
        sMess = "";
        if (mStatsCnt == 0)
        {
            sMess = "Nishio No pairs found";
            myDebug(sMess);
        }
        else
        {
            for (iStatPtr = 0; iStatPtr <= mStatsCnt - 1; iStatPtr++)
            {
                sMess = "Nishio Pair p1,p2 " + mStats[iStatPtr, (int)mS.p1] + "," + mStats[iStatPtr, (int)mS.p2] + " r,c " + mStats[iStatPtr, (int)mS.r] + "," 
                    + mStats[iStatPtr, (int)mS.c] + " cells " + mStats[iStatPtr, (int)mS.CellCnt] + " pstats " + mStats[iStatPtr, (int)mS.pStats] 
                    + " ranking " + mStats[iStatPtr, (int)mS.Ranking] + NEWLINE;
                myDebug(sMess);
            }
        }
    }
    private void SaveCopyofSK(int[, ,] skArg)
    {
        //save original array at the module level
        int r = 0;
        int c = 0;
        int p = 0;
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
    }
    private void KeepCandidate(ref int[, ,] skArg, ref int rArg, ref int cArg, ref int pArg)
    {
        //
        // removes all candidates except the specified one from the cell
        //
        int p = 0;

        for (p = 1; p <= 9; p++)
        {
            //keep p only
            if (p != pArg)
            {
                //currently a candidate
                if (skArg[rArg, cArg, p] == 1)
                {
                    skArg[rArg, cArg, p] = 0;
                    //no longer a candidate
                    mChangeCount = mChangeCount + 1;
                }
            }
        }

    }
    private void myDebug(string sMess)
    {
        //Redirect all Output Window text to the Immediate Window" checked under the menu Tools > Options > Debugging > General.
        //System.Diagnostics.Debug.WriteLine(sMess);
    }

}
