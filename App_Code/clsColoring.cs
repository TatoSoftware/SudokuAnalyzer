using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
// This technique is described in the book Mensa Guide to Solving Sudoku by Peter Gordon, 2006
public class clsColoring
{
	public clsColoring()
	{
	}
    
    //COJOChain Process list of cells that are co-joined and related to one another
    // if in a solution set a p value occurs exactly two times then the two cells are co-joined.
    //  Note - this object is called for each p value, it could be called for all p values
    //  and then one could determine the p value with the longest chain and solve for that
    //  p value 1st. That may make a big difference to performance. However to be simple
    //  implement the rule that the coloring techinque will be tried iff there are MIN_COJO_PAIRS
    //  co-joined pairs for a particular p-value. MIN_COJO_PAIRS could be from 2 to 11 (coloring 164 has
    //  a chain of 11 for p value 3. To check all possibilities for coloring, one must
    //  set MIN_COJO_PAIRS to 2.
    //
    //local variable(s) to hold property value(s)
    //performance tests a wash, so a min of 2 is ok
    const int MIN_COJO_PAIRS = 2;
    //lots of space for chains, 27 worked fine
    const int COJO_BUFFER = 100;
    //local copy
    private int mVarP;
    //same offset as main sk array,
    //cell values are 0 or 1, representing the presence of p in that cell
    //
    //input, not changed
    private int[,] mGridCopy = new int[9, 9];
    //output, used to update sk array
    private int[,] mResult = new int[9, 9];
    //results of trial solution A pair one starts with true, false
    private int[,] mTrialA = new int[9, 9];
    //results of trial solution B pair one starts with false, true
    private int[,] mTrialB = new int[9, 9];

    // csNote to fix this code use regular expressions \r  (return)  \n (line)  \t (tab)
    private int mChangeCount;
    int[,]mSSMap={{0,0,0,8},{1,0,1,8},{2,0,2,8},{3,0,3,8},{4,0,4,8},{5,0,5,8},{6,0,6,8},{7,0,7,8},{8,0,8,8},{0,0,8,0},{0,1,8,1},{0,2,8,2},{0,3,8,3},{0,4,8,4},
		{0,5,8,5},{0,6,8,6},{0,7,8,7},{0,8,8,8},{0,0,2,2},{0,3,2,5},{0,6,2,8},{3,0,5,2},{3,3,5,5},{3,6,5,8},{6,0,8,2},{6,3,8,5},{6,6,8,8}};
    public enum enSSMap
    {
        //col definitions for all solution sets (row, col and box)
        rFr = 0,
        //top left row,col
        cFr = 1,
        rTo = 2,
        //bottom right row col
        cTo = 3
    }

    const int SSCNT = 27;
    //used to lookup numbers of 3 solution sets associated with any cell
    private int[, ,] mSSLookup = new int[9, 9, 3];
    private enum enSSL
    {
        R = 0,
        C = 1,
        Box = 2
    }
    //ist of all co-joined pairs, may/may not be part of a chain
    private int[,] mCOJOPairs = new int[COJO_BUFFER + 1, 5];
    private int mCOJOPairsCnt;
    public enum enP
    {
        r1 = 0,
        //r,c of first pair in the solution set
        c1 = 1,
        r2 = 2,
        //r,c of 2nd pair in the solution set
        c2 = 3,
        Flag = 4
    }

    //flag values
    const int COJO_REMOVED = 0;

    const int COJO_AVAILABLE = -1;
    const int COJO_TRUE = 1;
    const int COJO_FALSE = 0;

    const int COJO_NOSTATE = -1;
    //ist of all co-joined cells in a chain
    private int[,] mCOJOChains = new int[COJO_BUFFER + 1, 7];
    private int mCOJOChainCnt;
    private int mSetID;
    //eg  a b 1      sample chain 
    //    b c 1
    //    r s 2
    //    s t 2
    //etc
    public enum enC
    {
        r1 = 0,
        //r,c of first cell in the chain  e
        c1 = 1,
        r2 = 2,
        //r,c of paired cell int he chain
        c2 = 3,
        SetNo = 4,
        //each grid can have multiple co-joined chains
        p1State = 5,
        //indicates if r1,c1 is to be set to true or false
        p2State = 6
        //indicates if r2,c2 is to be set to  true or false
    }
    public enum en
    {
        //convention for various arrays
        r = 0,
        c = 1
    }


    const int MAX_SETS = 10;
    //analyzed list of sets that we should work on
    int[] mSetsToTry = new int[MAX_SETS];
    //contains just the set numbers that have been o.k.'s to try
    int mSetsToTryCnt;

    //list of cells to update in final solution
    int[,] mSolnList = new int[81, 3];
    int mSolnCnt;
    public enum enS
    {
        r = 0,
        c = 1,
        Action = 2
    }
    const int COJO_REMOVE_CANDIDATE = 1;

    const int COJO_KEEP_CANDIDATE = 2;
    //local copy
    private string mvarMethod;
    const string COJO_METHOD_COLORING = "COLO";

    const string COJO_METHOD_COJOINS = "COJO";
    public int pValue
    {
        set { mVarP = value; }
    }

    public int ChangeCount
    {
        get { return mChangeCount; }
    }
    public string Method
    {
        get { return mvarMethod; }
    }

    public void BuildMatrix(ref int[, ,] skArg)
    {
        //
        // build a copy of grid with only the the p value we are interested in
        //   values are either 0 or the p Value
        //
        int r = 0;
        int c = 0;
        int p = 0;
        int iSS = 0;

        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                p = skArg[r, c, mVarP];
                if (p == 1)
                {
                    mGridCopy[r, c] = mVarP;
                }
                else
                {
                    mGridCopy[r, c] = 0;
                }
            }
        }
        //   
        // at the same time build the map of cell numbers to the soln sets that they are in
        // 
        //row ss's
        for (iSS = 0; iSS <= 8; iSS++)
        {
            for (r = mSSMap[iSS, (int)enSSMap.rFr]; r <= mSSMap[iSS, (int)enSSMap.rTo]; r++)
            {
                for (c = mSSMap[iSS, (int)enSSMap.cFr]; c <= mSSMap[iSS, (int)enSSMap.cTo]; c++)
                {
                    mSSLookup[r, c, (int)enSSL.R] = iSS;
                    //row ss
                }
            }
        }
        //col ss's
        for (iSS = 9; iSS <= 17; iSS++)
        {
            for (r = mSSMap[iSS, (int)enSSMap.rFr]; r <= mSSMap[iSS, (int)enSSMap.rTo]; r++)
            {
                for (c = mSSMap[iSS, (int)enSSMap.cFr]; c <= mSSMap[iSS, (int)enSSMap.cTo]; c++)
                {
                    mSSLookup[r, c, (int)enSSL.C] = iSS;
                    //colss
                }
            }
        }
        //box ss's
        for (iSS = 18; iSS <= 26; iSS++)
        {
            for (r = mSSMap[iSS, (int)enSSMap.rFr]; r <= mSSMap[iSS, (int)enSSMap.rTo]; r++)
            {
                for (c = mSSMap[iSS, (int)enSSMap.cFr]; c <= mSSMap[iSS, (int)enSSMap.cTo]; c++)
                {
                    mSSLookup[r, c, (int)enSSL.Box] = iSS;
                    //colss
                }
            }
        }
        //'
    }

    public void COJOChains(ref int[, ,] skArg)
    {
        int iPairID = 0;
        string sMethodOneResult = null;
        string sMethodTwoResult = null;
        mChangeCount = 0;
        mCOJOPairsCnt = 0;
        mSolnCnt = 0;
        mvarMethod = "";
        // look for chaines of co-joined cells and process
        BuildCOJOPairs();
        if (mCOJOPairsCnt < 2)
        {
            mChangeCount = 0;
            return;
            //need at least 2 pairs to care
        }
        //Process each pair that is available add to output list
        for (iPairID = 0; iPairID <= mCOJOPairsCnt - 1; iPairID++)
        {
            if (mCOJOPairs[iPairID, (int)enP.Flag] == COJO_AVAILABLE)
            {
                COJO_AddNew(iPairID);
            }
        }
        //Call DumpCOJOStats()
        // attempt to solve iff we have a minimum number of co-joined pairs
        GetListOfSetsToTry();

        sMethodOneResult = "";
        MethodOneDriver(ref sMethodOneResult);
        // only try method two if method one faills
        if (!string.IsNullOrEmpty(sMethodOneResult))
        {
            mvarMethod = COJO_METHOD_COJOINS;
        }
        //
        sMethodTwoResult = "";
        if (string.IsNullOrEmpty(sMethodOneResult))
        {
            MethodTwoDriver(ref sMethodTwoResult);
            //only do if method 1 does not work
            if (!string.IsNullOrEmpty(sMethodTwoResult))
            {
                mvarMethod = COJO_METHOD_COLORING;
            }
        }
        if (mSolnCnt > 0)
        {
            UpdateResult(ref skArg);
            //update complete puzzle, do stats and return
            //Call DumpSolnList()
        }
        else
        {
            mChangeCount = 0;
            //tell front end no changes
        }
    }
    private void MethodOneDriver(ref string sResult)
    {
        //method one, find a chain and stuff into an empty grid, try both true and false
        //scenarios, if one contains a conflict the other must be the only soln.
        int iSetID = 0;
        bool bFoundConflictsA = false;
        bool bFoundConflictsB = false;
        sResult = "";
        if (mSetsToTryCnt > 0)
        {
            for (iSetID = 0; iSetID <= mSetsToTryCnt - 1; iSetID++)
            {
                sResult = "";
                MethodOneSolnA(mSetsToTry[iSetID], ref bFoundConflictsA);
                MethodOneSolnB(mSetsToTry[iSetID], ref bFoundConflictsB);
                //Call IntegrityCheck(mSetsToTry(iSetID))     'testing only
                //
                if (bFoundConflictsA == true & bFoundConflictsB == false)
                {
                    //B is the one and only solution
                    sResult = "B is the only valid solution for pValue " + mVarP;
                }
                else if (bFoundConflictsB == true & bFoundConflictsA == false)
                {
                    //A is the only solution
                    sResult = "A is the only valid solution for pValue " + mVarP;
                }
                //RETURN WITH FIRST SOLUTION
                if (!string.IsNullOrEmpty(sResult))
                {
                    return;
                }
            }
        }

        if (string.IsNullOrEmpty(sResult))
        {
            mSolnCnt = 0;
            //a solution may have been captured that we want to ignore
        }

    }


    private void MethodOneSolnA(int vSetID, ref bool bConflictDetected)
    {
        int iPairID = 0;
        //need to propogate the true / false values in the cojo table and then analyze the result
        InitializeCOJOState(vSetID);
        iPairID = iGetFirstPairID(vSetID);
        //pair id is set to first pair in the list
        mCOJOChains[iPairID, (int)enC.p1State] = COJO_TRUE;
        mCOJOChains[iPairID, (int)enC.p2State] = COJO_FALSE;
        PropogateTrueFalse(vSetID, iPairID);
        InitializeGrid(ref mTrialA, "Zeros");
        MethodOneBuildTrial(vSetID, ref mTrialA);
        //Call DumpGrid("Trial A", mTrialA)
        if (bTestTrial(mTrialA))
        {
            bConflictDetected = false;
            UpdateSolnList(vSetID);
            //capture solution right now
        }
        else
        {
            bConflictDetected = true;
        }
    }
    private void MethodOneSolnB(int vSetID, ref bool bConflictDetected)
    {
        //same code as a, except first pair starting point is false, true  and destination grid
        int iPairID = 0;
        InitializeCOJOState(vSetID);
        iPairID = iGetFirstPairID(vSetID);
        //pair id is set to first pair in the list
        mCOJOChains[iPairID, (int)enC.p1State] = COJO_FALSE;
        mCOJOChains[iPairID, (int)enC.p2State] = COJO_TRUE;
        PropogateTrueFalse(vSetID, iPairID);
        InitializeGrid(ref mTrialB, "Zeros");
        MethodOneBuildTrial(vSetID, ref mTrialB);
        //    Call DumpGrid("Trial B", mTrialA)
        if (bTestTrial(mTrialB))
        {
            bConflictDetected = false;
            UpdateSolnList(vSetID);
            //capture solution right now
        }
        else
        {
            bConflictDetected = true;
        }
    }
    private void UpdateSolnList(int vSetID)
    {
        //just transfer list of cells to update to soln array for processing lates
        int r = 0;
        int c = 0;
        int[,] iGrid = new int[9, 9];
        mSolnCnt = 0;
        //
        InitializeGrid(ref iGrid, "BaseCase");
        MethodOneBuildTrial(vSetID, ref iGrid);
        //from the cojoin action list
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                mSolnList[mSolnCnt, (int)enS.r] = r;
                mSolnList[mSolnCnt, (int)enS.c] = c;
                if (iGrid[r, c] == 0)
                {
                    mSolnList[mSolnCnt, (int)enS.Action] = COJO_REMOVE_CANDIDATE;
                }
                else
                {
                    mSolnList[mSolnCnt, (int)enS.Action] = COJO_KEEP_CANDIDATE;
                }
                mSolnCnt = mSolnCnt + 1;
            }
        }
    }
    private int iGetFirstPairID(int vSetID)
    {
        int functionReturnValue = 0;
        //get pair id of the first member in the list
        int iPairID = 0;
        functionReturnValue = -1;
        //should always be set, the list is valid
        for (iPairID = 0; iPairID <= mCOJOPairsCnt - 1; iPairID++)
        {
            if (mCOJOChains[iPairID, (int)enC.SetNo] == vSetID)
            {
                functionReturnValue = iPairID;
                break; 
            }
        }
        return functionReturnValue;
    }

    private void InitializeCOJOState(int vSetID)
    {
        int iPairID = 0;
        //initialize the true/false flags in this set
        for (iPairID = 0; iPairID <= mCOJOPairsCnt - 1; iPairID++)
        {
            if (mCOJOChains[iPairID, (int)enC.SetNo] == vSetID)
            {
                mCOJOChains[iPairID, (int)enC.p1State] = COJO_NOSTATE;
                mCOJOChains[iPairID, (int)enC.p2State] = COJO_NOSTATE;
            }
        }
    }
    private bool bTestTrial(int[,] vGrid)
    {
        bool functionReturnValue = false;
        //
        //we have this set's T/F values in the grid, make sure
        //than no solution set contains the same value more than once
        //if so this set can never be valid
        //
        int r = 0;
        int c = 0;
        int iCnt = 0;
        int iSS = 0;
        //every row, col and box needs at least one value
        functionReturnValue = true;
        for (iSS = 0; iSS <= SSCNT - 1; iSS++)
        {
            iCnt = 0;
            for (r = mSSMap[iSS, (int)enSSMap.rFr]; r <= mSSMap[iSS, (int)enSSMap.rTo]; r++)
            {
                for (c = mSSMap[iSS, (int)enSSMap.cFr]; c <= mSSMap[iSS, (int)enSSMap.cTo]; c++)
                {
                    if (vGrid[r, c] != 0)
                    {
                        iCnt = iCnt + 1;
                    }
                }
            }
            //conflict detected
            if (iCnt > 1)
            {
                functionReturnValue = false;
                return functionReturnValue;
            }
        }
        return functionReturnValue;

    }
    private void InitializeGrid(ref int[,] rGrid, string sAction)
    {
        //
        int r = 0;
        int c = 0;
        switch (sAction)
        {
            case "Zeros":
                for (r = 0; r <= 8; r++)
                {
                    for (c = 0; c <= 8; c++)
                    {
                        rGrid[r, c] = 0;
                    }
                    //
                }

                break;
            case "BaseCase":
                for (r = 0; r <= 8; r++)
                {
                    for (c = 0; c <= 8; c++)
                    {
                        rGrid[r, c] = mGridCopy[r, c];
                    }
                }

                break;
        }

    }
    private void MethodOneBuildTrial(int vSetID, ref int[,] rGrid)
    {
        //populate the trial grid with the case defined in the COJO chain table
        int r = 0;
        int c = 0;
        int iPairID = 0;

        for (iPairID = 0; iPairID <= mCOJOChainCnt - 1; iPairID++)
        {

            if (mCOJOChains[iPairID, (int)enC.SetNo] == vSetID)
            {
                if (mCOJOChains[iPairID, (int)enC.p1State] == COJO_TRUE)
                {
                    //keep candidate
                    r = mCOJOChains[iPairID, (int)enC.r1];
                    c = mCOJOChains[iPairID, (int)enC.c1];
                    rGrid[r, c] = mVarP;
                    //keep candidate
                }
                else
                {
                    //wipes out previous candidate
                    r = mCOJOChains[iPairID, (int)enC.r1];
                    c = mCOJOChains[iPairID, (int)enC.c1];
                    rGrid[r, c] = 0;
                    //no longer a candidate
                }
                //
                if (mCOJOChains[iPairID, (int)enC.p2State] == COJO_TRUE)
                {
                    //keep candidate
                    r = mCOJOChains[iPairID, (int)enC.r2];
                    c = mCOJOChains[iPairID, (int)enC.c2];
                    rGrid[r, c] = mVarP;
                    //keep candidate
                }
                else
                {
                    //wipes out previous candidate
                    r = mCOJOChains[iPairID, (int)enC.r2];
                    c = mCOJOChains[iPairID, (int)enC.c2];
                    rGrid[r, c] = 0;
                    //no longer a candidate
                }
            }
        }
    }
    private void PropogateTrueFalse(int vSetID, int vPairID)
    {
        //recursive sub to take the true false values at the pairid specified and apply
        //these values to all other  matching pairs  in the set
        int iPairID = 0;
        int r1ToFind = 0;
        int c1ToFind = 0;
        int p1State = 0;
        int r2ToFind = 0;
        int c2ToFind = 0;
        int p2State = 0;
        bool bRowChanged = false;

        if (!bMoreTrueFalse(vSetID))
        {
            return;
        }
        // row, col to find
        r1ToFind = mCOJOChains[vPairID, (int)enC.r1];
        c1ToFind = mCOJOChains[vPairID, (int)enC.c1];
        p1State = mCOJOChains[vPairID, (int)enC.p1State];
        r2ToFind = mCOJOChains[vPairID, (int)enC.r2];
        c2ToFind = mCOJOChains[vPairID, (int)enC.c2];
        p2State = mCOJOChains[vPairID, (int)enC.p2State];
        //note could test if state is not set if so we have a bug
        //only find the first pair that needs to be updated
        for (iPairID = 0; iPairID <= mCOJOPairsCnt - 1; iPairID++)
        {
            //for clarity passed 
            bRowChanged = false;
            if (mCOJOChains[iPairID, (int)enC.SetNo] == vSetID)
            {
                //compare first pair to first pair
                if (mCOJOChains[iPairID, (int)enC.p1State] == COJO_NOSTATE & mCOJOChains[iPairID, (int)enC.r1] == r1ToFind & mCOJOChains[iPairID, (int)enC.c1] == c1ToFind)
                {
                    mCOJOChains[iPairID, (int)enC.p1State] = p1State;
                    mCOJOChains[iPairID, (int)enC.p2State] = p2State;
                    bRowChanged = true;
                }
                //compare first pair to second pair
                if (mCOJOChains[iPairID, (int)enC.p1State] == COJO_NOSTATE & mCOJOChains[iPairID, (int)enC.r2] == r1ToFind & mCOJOChains[iPairID, (int)enC.c2] == c1ToFind)
                {
                    mCOJOChains[iPairID, (int)enC.p2State] = p1State;
                    mCOJOChains[iPairID, (int)enC.p1State] = p2State;
                    bRowChanged = true;
                }
                //compare second pair to first pair
                if (mCOJOChains[iPairID, (int)enC.p1State] == COJO_NOSTATE & mCOJOChains[iPairID, (int)enC.r1] == r2ToFind & mCOJOChains[iPairID, (int)enC.c1] == c2ToFind)
                {
                    mCOJOChains[iPairID, (int)enC.p1State] = p2State;
                    mCOJOChains[iPairID, (int)enC.p2State] = p1State;
                    bRowChanged = true;
                }
                //compare second pair to second pair
                if (mCOJOChains[iPairID, (int)enC.p1State] == COJO_NOSTATE & mCOJOChains[iPairID, (int)enC.r2] == r2ToFind & mCOJOChains[iPairID, (int)enC.c2] == c2ToFind)
                {
                    mCOJOChains[iPairID, (int)enC.p2State] = p2State;
                    mCOJOChains[iPairID, (int)enC.p1State] = p1State;
                    bRowChanged = true;
                }
                if (bRowChanged)
                {
                    PropogateTrueFalse(vSetID, iPairID);
                }
            }
        }

    }

    private bool bMoreTrueFalse(int vSetID)
    {
        bool functionReturnValue = false;
        //see if any more T/F values need to be set for this set
        int iPairID = 0;
        functionReturnValue = false;
        for (iPairID = 0; iPairID <= mCOJOPairsCnt - 1; iPairID++)
        {
            if (mCOJOChains[iPairID, (int)enC.SetNo] == vSetID)
            {
                if (mCOJOChains[iPairID, (int)enC.p1State] == COJO_NOSTATE | mCOJOChains[iPairID, (int)enC.p2State] == COJO_NOSTATE)
                {
                    functionReturnValue = true;
                    break; 
                }
            }
        }
        return functionReturnValue;
    }

    private void COJO_AddNew(int vPairID)
    {
        //
        //call with the of a new pair which starts a new chain/set
        //
        int r1 = 0;
        int c1 = 0;
        int r2 = 0;
        int c2 = 0;
        mSetID = mSetID + 1;
        //add entry to list of co-joins, but need all the rest that are joined
        r1 = mCOJOPairs[vPairID, (int)enP.r1];
        c1 = mCOJOPairs[vPairID, (int)enP.c1];
        mCOJOChains[mCOJOChainCnt, (int)enC.r1] = r1;
        //a of ab pair
        mCOJOChains[mCOJOChainCnt, (int)enC.c1] = c1;
        //
        r2 = mCOJOPairs[vPairID, (int)enP.r2];
        c2 = mCOJOPairs[vPairID, (int)enP.c2];
        mCOJOChains[mCOJOChainCnt, (int)enC.r2] = r2;
        //b of ab pair
        mCOJOChains[mCOJOChainCnt, (int)enC.c2] = c2;
        //
        mCOJOChains[mCOJOChainCnt, (int)enC.SetNo] = mSetID;

        mCOJOChainCnt = mCOJOChainCnt + 1;
        //new entry in list table

        mCOJOPairs[vPairID, (int)enP.Flag] = COJO_REMOVED;
        //no longer available 

        // are there any available pairs that contain either p1 or p2
        //Debug.Print("AddNew p1,p2,setid " & p1 & "," & p2 & " " & mSetID)
        //
        BuildList(mSetID, r1, c1, r2, c2);
        //set and staring pair

    }
    private void BuildList(int vSetID, int vr1, int vc1, int vr2, int vc2)
    {
        //
        //recursive routine to wallk the chain of all pairs that are co-joined
        //adding each one to the set/chain and removing the pair from the source pair table
        //
        int iPairId = 0;
        int iPairCnt = 0;
        int iPairIndx = 0;
        int[] iPairList = new int[101];
        int iNewR1 = 0;
        int iNewC1 = 0;
        int iNewR2 = 0;
        int iNewC2 = 0;

        //belt and braces recursive routine
        if (!bAvailablePairs())
        {
            return;
        }

        Get_MatchingPairs(ref iPairList, ref iPairCnt, vr1, vc1, vr2, vc2);
        //if a pair was found
        if (iPairCnt > 0)
        {
            for (iPairIndx = 0; iPairIndx <= iPairCnt - 1; iPairIndx++)
            {
                iPairId = iPairList[iPairIndx];
                //add the found pair to the list in the chain
                if (mCOJOPairs[iPairId, (int)enP.Flag] == COJO_AVAILABLE)
                {
                    mCOJOChains[mCOJOChainCnt, (int)enC.SetNo] = mSetID;
                    iNewR1 = mCOJOPairs[iPairId, (int)enP.r1];
                    iNewC1 = mCOJOPairs[iPairId, (int)enP.c1];
                    iNewR2 = mCOJOPairs[iPairId, (int)enP.r2];
                    iNewC2 = mCOJOPairs[iPairId, (int)enP.c2];
                    mCOJOChains[mCOJOChainCnt, (int)enC.r1] = iNewR1;
                    //a of ab pair
                    mCOJOChains[mCOJOChainCnt, (int)enC.c1] = iNewC1;

                    mCOJOChains[mCOJOChainCnt, (int)enC.r2] = iNewR2;
                    //b of ab pair
                    mCOJOChains[mCOJOChainCnt, (int)enC.c2] = iNewC2;

                    mCOJOChainCnt = mCOJOChainCnt + 1;

                    mCOJOPairs[iPairId, (int)enP.Flag] = COJO_REMOVED;
                    //take off available list
                    //Call myDebug("Set " & mSetID & " " & "pair found at " & iPairId & " " & vr1 & "," & vc1 & "  " & vr2 & "," & vc2)
                    if (bAvailablePairs())
                    {
                        BuildList(vSetID, iNewR1, iNewC1, iNewR2, iNewC2);
                        //recursive call
                    }
                    else
                    {
                        return;
                    }
                }
            }
            //pair
        }
    }
    public bool bAvailablePairs()
    {
        bool functionReturnValue = false;
        //any pairs remaining the pairs list to process
        int iPairID = 0;

        functionReturnValue = false;
        for (iPairID = 0; iPairID <= mCOJOPairsCnt - 1; iPairID++)
        {
            if (mCOJOPairs[iPairID, (int)enP.Flag] == COJO_AVAILABLE)
            {
                functionReturnValue = true;
                return functionReturnValue;
            }
        }
        return functionReturnValue;

    }
    private void Get_MatchingPairs(ref int[] rPList, ref int rPlistCnt, int vR1, int vC1, int vR2, int vC2)
    {
        //
        //return list of available pairs that are jointed to the cell passed.
        // check  a,b against the list  c,d 
        // compare a,c  a,d  b,c  b,d
        //
        int iPairID = 0;
        rPlistCnt = 0;
        for (iPairID = 0; iPairID <= mCOJOPairsCnt - 1; iPairID++)
        {
            if (mCOJOPairs[iPairID, (int)enP.Flag] == COJO_AVAILABLE)
            {
                //check both possibilities independently
                if ((mCOJOPairs[iPairID, (int)enP.r1] == vR1 & mCOJOPairs[iPairID, (int)enP.c1] == vC1 | mCOJOPairs[iPairID, (int)enP.r2] == vR1 & mCOJOPairs[iPairID, (int)enP.c2] == vC1))
                {
                    rPList[rPlistCnt] = iPairID;
                    rPlistCnt = rPlistCnt + 1;
                }
                if ((mCOJOPairs[iPairID, (int)enP.r1] == vR2 & mCOJOPairs[iPairID, (int)enP.c1] == vC2 | mCOJOPairs[iPairID, (int)enP.r2] == vR2 & mCOJOPairs[iPairID, (int)enP.c2] == vC2))
                {
                    rPList[rPlistCnt] = iPairID;
                    rPlistCnt = rPlistCnt + 1;
                }
            }
        }
    }

    private void BuildCOJOPairs()
    {
        int[,] iTest = new int[9, 2];
        //temp buffer for 9 candidates, row and col
        int iSS = 0;
        int r = 0;
        int c = 0;
        int iTestCnt = 0;
        mCOJOPairsCnt = 0;
        iTestCnt = 0;

        for (iSS = 0; iSS <= SSCNT - 1; iSS++)
        {
            for (r = mSSMap[iSS, (int)enSSMap.rFr]; r <= mSSMap[iSS, (int)enSSMap.rTo]; r++)
            {
                for (c = mSSMap[iSS, (int)enSSMap.cFr]; c <= mSSMap[iSS, (int)enSSMap.cTo]; c++)
                {
                    if (mGridCopy[r, c] != 0)
                    {
                        iTest[iTestCnt, (int)en.r] = r;
                        //just save them there may be 1,9 p values  in this ss
                        iTest[iTestCnt, (int)en.c] = c;
                        iTestCnt = iTestCnt + 1;
                        //   myDebug("colist ss,r,c " & iSS & " " & r & " " & c & " Value is " & mGridCopy(r, c))
                    }
                }
            }
            //end of this solution set, do we have 2 and only 2 cells that contain this p value
            //as a candidate ?
            if (iTestCnt == 2)
            {
                //
                //add to main list of cojoint cells, ie 2 cells in this ss are co-joined.
                //
                AddCOJOPair(iTest[0, (int)en.r], iTest[0, (int)en.c], iTest[1, (int)en.r], iTest[1, (int)en.c]);
            }
            iTestCnt = 0;
            //start again
        }
    }
    private void AddCOJOPair(int vr1, int vc1, int vr2, int vc2)
    {
        //
        //add new pair, but don't add duplicates,
        //not that a,b = b,a
        //
        if (iFindPair(vr1, vc1, vr2, vc2) < 0)
        {
            mCOJOPairs[mCOJOPairsCnt, (int)enP.r1] = vr1;
            //first one row, col
            mCOJOPairs[mCOJOPairsCnt, (int)enP.c1] = vc1;
            mCOJOPairs[mCOJOPairsCnt, (int)enP.r2] = vr2;
            //second one row, col
            mCOJOPairs[mCOJOPairsCnt, (int)enP.c2] = vc2;
            mCOJOPairs[mCOJOPairsCnt, (int)enP.Flag] = COJO_AVAILABLE;
            mCOJOPairsCnt = mCOJOPairsCnt + 1;
        }

    }
    private int iFindPair(int vr1, int vc1, int vr2, int vc2)
    {
        int functionReturnValue = 0;
        //Process each pair that is available add to output list
        int iPairId = 0;
        functionReturnValue = -1;
        for (iPairId = 0; iPairId <= mCOJOPairsCnt - 1; iPairId++)
        {
            if (mCOJOPairs[iPairId, (int)enP.Flag] == COJO_AVAILABLE)
            {
                //
                //look for exact pairs a,b or b,a 
                //
                if ((mCOJOPairs[iPairId, (int)enP.r1] == vr1 & mCOJOPairs[iPairId, (int)enP.c1] == vc1) & (mCOJOPairs[iPairId, (int)enP.r2] == vr2 & mCOJOPairs[iPairId, (int)enP.c2] == vc2) | (mCOJOPairs[iPairId, (int)enP.r2] == vr1 & mCOJOPairs[iPairId, (int)enP.c2] == vc1) & (mCOJOPairs[iPairId, (int)enP.r1] == vr2 & mCOJOPairs[iPairId, (int)enP.c1] == vc2))
                {
                    functionReturnValue = iPairId;
                }
            }
        }
        return functionReturnValue;

    }

    private void GetListOfSetsToTry()
    {
        int i = 0;
        int[] nSetCnts = new int[MAX_SETS];
        int iSetNo = 0;
        for (i = 0; i <= mCOJOChainCnt - 1; i++)
        {
            iSetNo = mCOJOChains[i, (int)enC.SetNo];
            if (iSetNo < MAX_SETS)
            {
                nSetCnts[iSetNo] = nSetCnts[iSetNo] + 1;
            }
        }
        //
        // now know how may pairs we have in each set.
        // if the no of pairs pass the threshold add it to the list of sets to try
        //
        for (iSetNo = 0; iSetNo <= MAX_SETS - 1; iSetNo++)
        {
            if (nSetCnts[iSetNo] >= MIN_COJO_PAIRS)
            {
                mSetsToTry[mSetsToTryCnt] = iSetNo;
                mSetsToTryCnt = mSetsToTryCnt + 1;
            }
        }
    }


    //---------------------------------------------------------------------------
    private void MethodTwoDriver(ref string rResult)
    {
        //
        // method two is ...
        //
        //Call myDebug("Trying Method Two, one didn't work p value is " & mVarP)

        int iSetID = 0;
        rResult = "";
        if (mSetsToTryCnt > 0)
        {
            for (iSetID = 0; iSetID <= mSetsToTryCnt - 1; iSetID++)
            {
                rResult = "";
                MethodTwoSolnA(mSetsToTry[iSetID]);
                MethodTwoSolnB(mSetsToTry[iSetID]);
                CompareMethodTwoSolns();
                if (mSolnCnt > 0)
                {
                    rResult = "Method 2 Success";
                    //return first solution
                    return;
                }
            }
        }

    }
    private void MethodTwoSolnA(int vSetID)
    {
        int iPairID = 0;
        //need to propogate the true / false values in the cojo table and then analyze the result
        InitializeCOJOState(vSetID);
        iPairID = iGetFirstPairID(vSetID);
        //pair id is set to first pair in the list
        mCOJOChains[iPairID, (int)enC.p1State] = COJO_TRUE;
        mCOJOChains[iPairID, (int)enC.p2State] = COJO_FALSE;
        PropogateTrueFalse(vSetID, iPairID);
        InitializeGrid(ref mTrialA, "BaseCase");
        MethodTwoBuildTrial(vSetID, ref mTrialA);
        MethodTwoNakedSingles(ref mTrialA);
        //Call DumpGrid("MethodTwo A", mTrialA)
    }
    private void MethodTwoSolnB(int vSetID)
    {
        int iPairID = 0;
        //need to propogate the true / false values in the cojo table and then analyze the result
        InitializeCOJOState(vSetID);
        iPairID = iGetFirstPairID(vSetID);
        //pair id is set to first pair in the list
        mCOJOChains[iPairID, (int)enC.p1State] = COJO_FALSE;
        mCOJOChains[iPairID, (int)enC.p2State] = COJO_TRUE;
        PropogateTrueFalse(vSetID, iPairID);
        InitializeGrid(ref mTrialB, "BaseCase");
        MethodTwoBuildTrial(vSetID, ref mTrialB);
        MethodTwoNakedSingles(ref mTrialB);
        //Call DumpGrid("MethodTwo B", mTrialB)
    }
    private void MethodTwoBuildTrial(int vSetID, ref int[,] rGrid)
    {
        //
        //method two brings in the original values from the saved grid, we may need to remove naked singles
        //
        int r = 0;
        int c = 0;
        int iPairID = 0;

        for (iPairID = 0; iPairID <= mCOJOChainCnt - 1; iPairID++)
        {

            if (mCOJOChains[iPairID, (int)enC.SetNo] == vSetID)
            {
                if (mCOJOChains[iPairID, (int)enC.p1State] == COJO_TRUE)
                {
                    //keep candidate
                    r = mCOJOChains[iPairID, (int)enC.r1];
                    c = mCOJOChains[iPairID, (int)enC.c1];
                    SetTrialValue(ref rGrid, r, c, mVarP);
                    //lock in candidate, remove naked singles
                }
                else
                {
                    //wipes out previous candidate
                    r = mCOJOChains[iPairID, (int)enC.r1];
                    c = mCOJOChains[iPairID, (int)enC.c1];
                    rGrid[r, c] = 0;
                    //no longer a candidate
                }
                //
                if (mCOJOChains[iPairID, (int)enC.p2State] == COJO_TRUE)
                {
                    //keep candidate
                    r = mCOJOChains[iPairID, (int)enC.r2];
                    c = mCOJOChains[iPairID, (int)enC.c2];
                    SetTrialValue(ref rGrid, r, c, mVarP);
                    //lock in candidate, remove naked singles
                }
                else
                {
                    //wipes out previous candidate
                    r = mCOJOChains[iPairID, (int)enC.r2];
                    c = mCOJOChains[iPairID, (int)enC.c2];
                    rGrid[r, c] = 0;
                    //no longer a candidate
                }
            }
        }
    }
    private void MethodTwoNakedSingles(ref int[,] rGrid)
    {
        //
        //there still may be some naked singles laying around if so then process them
        // not sure if one pass through all naked singles will do the trick
        //
        int r = 0;
        int c = 0;
        int iCnt = 0;
        int iSS = 0;
        int rFound = 0;
        int cFound = 0;

        //every row, col and box needs at least one value
        for (iSS = 0; iSS <= SSCNT - 1; iSS++)
        {
            iCnt = 0;
            for (r = mSSMap[iSS, (int)enSSMap.rFr]; r <= mSSMap[iSS, (int)enSSMap.rTo]; r++)
            {
                for (c = mSSMap[iSS, (int)enSSMap.cFr]; c <= mSSMap[iSS, (int)enSSMap.cTo]; c++)
                {
                    if (rGrid[r, c] != 0)
                    {
                        rFound = r;
                        cFound = c;
                        iCnt = iCnt + 1;
                    }
                }
            }
            //naked single detected.
            if (iCnt == 1)
            {
                SetTrialValue(ref rGrid, rFound, cFound, mVarP);
                //will set this value to only one on in the ss
            }
        }
    }

    private void SetTrialValue(ref int[,] rGrid, int vR, int vC, int vPValue)
    {
        //we have a solution for a certain cell.
        //thus all other candidates in the row, col or box containing that cell must be removed
        int iSS = 0;
        int r = 0;
        int c = 0;
        int iSSPtr = 0;
        int rF = 0;
        int cF = 0;
        int rT = 0;
        int cT = 0;
        //
        if (vPValue == 0)
        {
            rGrid[vR, vC] = 0;
            return;
        }
        //no longer a candidate, this is the value for this cell
        //zap all candiates in the 3 solution sets that this cell is in
        //
        for (iSSPtr = 0; iSSPtr <= 2; iSSPtr++)
        {
            iSS = mSSLookup[vR, vC, iSSPtr];
            rF = mSSMap[iSS, (int)enSSMap.rFr];
            rT = mSSMap[iSS, (int)enSSMap.rTo];
            cF = mSSMap[iSS, (int)enSSMap.cFr];
            cT = mSSMap[iSS, (int)enSSMap.cTo];
            for (r = rF; r <= rT; r++)
            {
                for (c = cF; c <= cT; c++)
                {
                    rGrid[r, c] = 0;
                }
            }
        }
        //all ss's are cleared, stuff in the one and only candidate
        rGrid[vR, vC] = vPValue;
    }

    private void CompareMethodTwoSolns()
    {
        //
        //if any cells are the same in both cases, then we can set them to these values
        //as no matter what this is what they are
        //need to exclude 
        int r = 0;
        int c = 0;
        bool bSuccess = false;
        int[,] iResultGrid = new int[9, 9];
        //populate grid initially wiht starting case
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                iResultGrid[r, c] = mGridCopy[r, c];
            }
        }
        //
        bSuccess = false;
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                if (mGridCopy[r, c] != 0)
                {
                    //If mTrialA(r, c) = mTrialB(r, c) Then
                    //    Call SetTrialValue(iResultGrid, r, c, mTrialA(r, c))
                    //    bSuccess = True
                    //End If
                    if (mTrialA[r, c] == mTrialB[r, c] & mTrialA[r, c] == 0)
                    {
                        iResultGrid[r, c] = 0;
                        //removes the candidate
                        bSuccess = true;
                    }
                }
            }
        }
        if (bSuccess)
        {
            mvarMethod = COJO_METHOD_COLORING;
            //Call DumpGrid("MethodTwo Combined Success , Generating action list ", iResultGrid)
            mSolnCnt = 0;
            for (r = 0; r <= 8; r++)
            {
                for (c = 0; c <= 8; c++)
                {
                    mSolnList[mSolnCnt, (int)enS.r] = r;
                    mSolnList[mSolnCnt, (int)enS.c] = c;
                    if (iResultGrid[r, c] == 0)
                    {
                        mSolnList[mSolnCnt, (int)enS.Action] = COJO_REMOVE_CANDIDATE;
                    }
                    else
                    {
                        mSolnList[mSolnCnt, (int)enS.Action] = COJO_KEEP_CANDIDATE;
                    }
                    mSolnCnt = mSolnCnt + 1;
                }
            }
        }
    }
    private void DumpSolnList()
    {
        int i = 0;
        int[,] rTest = new int[9, 9];

        for (i = 0; i <= mSolnCnt - 1; i++)
        {
            switch (mSolnList[i, (int)enS.Action])
            {
                case COJO_KEEP_CANDIDATE:
                    rTest[mSolnList[i, (int)enS.r], mSolnList[i, (int)enS.c]] = mVarP;
                    break;
                case COJO_REMOVE_CANDIDATE:
                    rTest[mSolnList[i, (int)enS.r], mSolnList[i, (int)enS.c]] = 0;
                    break;
            }
            if (mSolnList[i, (int)enS.Action] == COJO_KEEP_CANDIDATE)
            {
                myDebug("Method 2 Keep -  " + mSolnList[i, (int)enS.r] + "," + mSolnList[i, (int)enS.c] + "  " + mVarP);
            }
            //If mSolnList(i, (int)enS.Action) = COJO_ONLY_CANDIDATE Then
            //    Call myDebug("Method 2 Only -  " & mSolnList(i, (int)enS.r) & "," & mSolnList(i, (int)enS.c) & "  " & mVarP)
            //End If
            if (mSolnList[i, (int)enS.Action] == COJO_REMOVE_CANDIDATE)
            {
                myDebug("Method 2 Remove -  " + mSolnList[i, (int)enS.r] + "," + mSolnList[i, (int)enS.c] + "  " + mVarP);
            }
        }

        //Call DumpGrid("Final Solution using " & mvarMethod, rTest)
    }
    public void UpdateResult(ref int[, ,] skArg)
    {
        //
        //update original grid with the changes for this particular p value only
        //update is driven by a list of cells (task list) to process.
        //
        int iCell = 0;
        mChangeCount = 0;
        int iNewValue = 0;
        int iOldValue = 0;
        for (iCell = 0; iCell <= mSolnCnt - 1; iCell++)
        {
            iOldValue = skArg[mSolnList[iCell, (int)enS.r], mSolnList[iCell, (int)enS.c], mVarP];
            //
            switch (mSolnList[iCell, (int)enS.Action])
            {
                case COJO_KEEP_CANDIDATE:
                    skArg[mSolnList[iCell, (int)enS.r], mSolnList[iCell, (int)enS.c], mVarP] = 1;
                    break;
                case COJO_REMOVE_CANDIDATE:
                    skArg[mSolnList[iCell, (int)enS.r], mSolnList[iCell, (int)enS.c], mVarP] = 0;
                    break;
            }
            //
            iNewValue = skArg[mSolnList[iCell, (int)enS.r], mSolnList[iCell, (int)enS.c], mVarP];
            if (iNewValue != iOldValue)
            {
                mChangeCount = mChangeCount + 1;
            }
        }
    }
    private void myDebug(string sMess)
    {
        //Redirect all Output Window text to the Immediate Window" checked under the menu Tools > Options > Debugging > General.
        //System.Diagnostics.Debug.WriteLine(sMess);
    }
}
