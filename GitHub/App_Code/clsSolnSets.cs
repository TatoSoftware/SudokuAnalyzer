using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
internal class clsSolnSets
{
    //
    // solve all Sudoku Puzzles than can be solved by looking at individual 
    // solution sets (row, column or box)
    // msk array is cloned/copied from the calling object
    // ex  msk(1,3,4) = means row 5 column 6 (of the sudoku grid) has a candidate of 2
    // ex  msk(5,6,2) = 1 means row 5 column 6 (of the sudoku grid) has a candidate of 2
    //  dimension 1 rows 1 to 9, element 0 of array is ignored
    //  dimension 2 cols 1 to 9, element 0 of array is ignored
    //  dimension 3 contains p values 1 to 9, again element o is ignored.
    const int MAXITERATIONS = 9999999;
    int[, ,] mSK = new int[10, 10, 10];
    // ============================================================
    // scroll down to "CodeStart"
    //no of unique triplets
    int[,] miTRIPDefn ={
     {1, 2, 3}, {1, 2, 4}, {1, 2, 5}, {1, 2, 6}, {1, 2, 7}, {1, 2, 8}, {1, 2, 9}, {1, 3, 4}, {1, 3, 5}, {1, 3, 6}, {1, 3, 7}, {1, 3, 8}, {1, 3, 9}, {1, 4, 5}, {1, 4, 6}, {1, 4, 7}, {1, 4, 8}, {1, 4, 9}, {1, 5, 6}, {1, 5, 7}, {1, 5, 8}, {1, 5, 9}, {1, 6, 7}, {1, 6, 8}, {1, 6, 9}, {1, 7, 8}, {1, 7, 9}, {1, 8, 9},
     {2, 3, 4}, {2, 3, 5}, {2, 3, 6}, {2, 3, 7}, {2, 3, 8}, {2, 3, 9}, {2, 4, 5}, {2, 4, 6}, {2, 4, 7}, {2, 4, 8}, {2, 4, 9}, {2, 5, 6}, {2, 5, 7}, {2, 5, 8}, {2, 5, 9}, {2, 6, 7}, {2, 6, 8}, {2, 6, 9}, {2, 7, 8}, {2, 7, 9}, {2, 8, 9}, {3, 4, 5}, {3, 4, 6}, {3, 4, 7}, {3, 4, 8}, {3, 4, 9}, {3, 5, 6}, {3, 5, 7},
     {3, 5, 8}, {3, 5, 9}, {3, 6, 7}, {3, 6, 8}, {3, 6, 9}, {3, 7, 8}, {3, 7, 9}, {3, 8, 9}, {4, 5, 6}, {4, 5, 7}, {4, 5, 8}, {4, 5, 9}, {4, 6, 7}, {4, 6, 8}, {4, 6, 9}, {4, 7, 8}, {4, 7, 9}, {4, 8, 9}, {5, 6, 7}, {5, 6, 8}, {5, 6, 9}, {5, 7, 8}, {5, 7, 9}, {5, 8, 9}, {6, 7, 8}, {6, 7, 9}, {6, 8, 9}, {7, 8, 9}};
    //
    int[,] mQuadDefn ={
     {1, 2, 3, 4}, {1, 2, 3, 5}, {1, 2, 3, 6}, {1, 2, 3, 7}, {1, 2, 3, 8}, {1, 2, 3, 9}, {1, 2, 4, 5}, {1, 2, 4, 6}, {1, 2, 4, 7}, {1, 2, 4, 8}, {1, 2, 4, 9}, {1, 2, 5, 6}, {1, 2, 5, 7}, {1, 2, 5, 8}, {1, 2, 5, 9}, 
     {1, 3, 6, 7}, {1, 3, 6, 8}, {1, 3, 6, 9}, {1, 3, 7, 8}, {1, 3, 7, 9}, {1, 3, 8, 9}, {1, 4, 5, 6}, {1, 4, 5, 7}, {1, 4, 5, 8}, {1, 4, 5, 9}, {1, 4, 6, 7}, {1, 4, 6, 8}, {1, 4, 6, 9}, {1, 4, 7, 8}, {1, 4, 7, 9},
     {1, 4, 8, 9}, {1, 5, 6, 7}, {1, 5, 6, 8}, {1, 5, 6, 9}, {1, 5, 7, 8}, {1, 5, 7, 9}, {1, 5, 8, 9}, {1, 6, 7, 8}, {1, 6, 7, 9}, {1, 6, 8, 9}, {1, 7, 8, 9}, {2, 3, 4, 5}, {2, 3, 4, 6}, {2, 3, 4, 7}, {2, 3, 4, 8},
     {2, 3, 4, 9}, {2, 3, 5, 6}, {2, 3, 5, 7}, {2, 3, 5, 8}, {2, 3, 5, 9}, {2, 3, 6, 7}, {2, 3, 6, 8}, {2, 3, 6, 9}, {2, 3, 7, 8}, {2, 3, 7, 9}, {2, 3, 8, 9}, {2, 4, 5, 6}, {2, 4, 5, 7}, {2, 4, 5, 8}, {2, 4, 5, 9},
     {2, 4, 6, 7}, {2, 4, 6, 8}, {2, 4, 6, 9}, {2, 4, 7, 8}, {2, 4, 7, 9}, {2, 4, 8, 9}, {2, 5, 6, 7}, {2, 5, 6, 8}, {2, 5, 6, 9}, {2, 5, 7, 8}, {2, 5, 7, 9}, {2, 5, 8, 9}, {2, 6, 7, 8}, {2, 6, 7, 9}, {2, 6, 8, 9},
     {2, 7, 8, 9}, {3, 4, 5, 6}, {3, 4, 5, 7}, {3, 4, 5, 8}, {3, 4, 5, 9}, {3, 4, 6, 7}, {3, 4, 6, 8}, {3, 4, 6, 9}, {3, 4, 7, 8}, {3, 4, 7, 9}, {3, 4, 8, 9}, {3, 5, 6, 7}, {3, 5, 6, 8}, {3, 5, 6, 9}, {3, 5, 7, 8},
     {3, 5, 7, 9}, {3, 5, 8, 9}, {3, 6, 7, 8}, {3, 6, 7, 9}, {3, 6, 8, 9}, {3, 7, 8, 9}, {4, 5, 6, 7}, {4, 5, 6, 8}, {4, 5, 6, 9}, {4, 5, 7, 8}, {4, 5, 7, 9}, {4, 5, 8, 9}, {4, 6, 7, 8}, {4, 6, 7, 9}, {4, 6, 8, 9},
     {4, 7, 8, 9}, {5, 6, 7, 8}, {5, 6, 7, 9}, {5, 6, 8, 9}, {5, 7, 8, 9}, {6, 7, 8, 9}};
    //type of solution set
    const int mROWSS = 1;
    const int mCOLSS = 2;
    const int mBOXSS = 3;
    //36 unique pairs of p values, one row per pair, zero based definitions
    //no of unique pairs
    int[,] miPairsDefn = {{1, 2}, {1, 3}, {1, 4}, {1, 5}, {1, 6}, {1, 7}, {1, 8}, {1, 9}, {2, 3}, {2, 4}, {2, 5}, {2, 6}, {2, 7},
            {2, 8}, {2, 9}, {3, 4}, {3, 5}, {3, 6}, {3, 7}, {3, 8}, {3, 9}, {4, 5}, {4, 6}, {4, 7}, {4, 8}, {4, 9},
            {5, 6}, {5, 7}, {5, 8}, {5, 9}, {6, 7}, {6, 8}, {6, 9}, {7, 8}, {7, 9}, {8, 9}};
    //map of group ss's to row and col ss's
    int[,] mBoxSSMap = {
            {19, 1, 2, 3, 10, 11, 12}, {20, 1, 2, 3, 13, 14, 15}, {21, 1, 2, 3, 16, 17, 18}, {22, 4, 5, 6, 10, 11, 12},
            {23, 4, 5, 6, 13, 14, 15}, {24, 4, 5, 6, 16, 17, 18}, {25, 7, 8, 9, 10, 11, 12}, {26, 7, 8, 9, 13, 14, 15}, {27, 7, 8, 9, 16, 17, 18}};
    public enum enBoxSSMap
    {
        //col definitions for mBoxSSMap
        ss = 0,
        //box ss id
        R0 = 1,
        //contains ss no of intersecting row 0 of the group/box
        r1 = 2,
        r2 = 3,
        C0 = 4,
        //contains ss no of intersecting col 0 of the group/box
        c1 = 5,
        c2 = 6
    }
    int[,] mRowSSMap =  {
        {1, 19, 20, 21}, {2, 19, 20, 21}, {3, 19, 20, 21}, {4, 22, 23, 24}, {5, 22, 23, 24}, {6, 22, 23, 24}, {7, 25, 26, 27}, {8, 25, 26, 27}, {9, 25, 26, 27}};
    public enum enrSSM
    {
        //col definitions for mRowSSMap
        ss = 0,
        //row ss id
        b0 = 1,
        //contains ss no of intersecting box of the row
        b1 = 2,
        b2 = 3
    }
    int[,] mColSSMap = {
        {10, 19, 22, 25}, {11, 19, 22, 25}, {12, 19, 22, 25}, {13, 20, 23, 26}, {14, 20, 23, 26}, {15, 20, 23, 26}, {16, 21, 24, 27}, {17, 21, 24, 27}, {18, 21, 24, 27}};
    public enum encSSM
    {
        //col definitions for mColSSMap
        ss = 0,
        //col ss id
        b0 = 1,
        //contains ss no of intersecting box of the col
        b1 = 2,
        b2 = 3
    }

    // for performance reasons, define a list of soln set pointers
    // that point to the mSolnSet array, showing where the list of r,c's
    // start and end for each solution set
    // indexed by ss number whiich is 1-27, first entry is not used.
    int[,] mSSPtr = {
        {0, 0}, {1, 9}, {10, 18}, {19, 27}, {28, 36}, {37, 45}, {46, 54}, {55, 63}, {64, 72}, {73, 81}, {82, 90}, {91, 99}, {100, 108}, {109, 117}, {118, 126}, {127, 135}, {136, 144}, {145, 153}, {154, 162}, {163, 171},
        {172, 180}, {181, 189}, {190, 198}, {199, 207}, {208, 216}, {217, 225}, {226, 234}, {235, 243}};
    public enum enmSSP
    {
        FromPtr = 0,
        //ptr to start of list in mSolnSet array
        ToPtr = 1
        //ptr to end of list
    }

    //define the types of solution sets
    //fixed no of soln sets
    const int mSSCNT = 27;
    //indexed by solution set number which is 1-27 (first element is ignored)
    int[] mSolnSetTypes = { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
    //eg solution set no 1, contains the cells r0,c0 to r0c8
    int[,] mSolnSet = {{0, 0, 0},
        {1, 0, 0}, {1, 0, 1}, {1, 0, 2}, {1, 0, 3}, {1, 0, 4}, {1, 0, 5}, {1, 0, 6}, {1, 0, 7}, {1, 0, 8}, {2, 1, 0}, {2, 1, 1}, {2, 1, 2}, {2, 1, 3}, {2, 1, 4}, {2, 1, 5}, {2, 1, 6}, {2, 1, 7}, {2, 1, 8}, {3, 2, 0}, {3, 2, 1}, {3, 2, 2},
        {3, 2, 3}, {3, 2, 4}, {3, 2, 5}, {3, 2, 6}, {3, 2, 7}, {3, 2, 8}, {4, 3, 0}, {4, 3, 1}, {4, 3, 2}, {4, 3, 3}, {4, 3, 4}, {4, 3, 5}, {4, 3, 6}, {4, 3, 7}, {4, 3, 8}, {5, 4, 0}, {5, 4, 1}, {5, 4, 2}, {5, 4, 3}, {5, 4, 4}, {5, 4, 5},
        {5, 4, 6}, {5, 4, 7}, {5, 4, 8}, {6, 5, 0}, {6, 5, 1}, {6, 5, 2}, {6, 5, 3}, {6, 5, 4}, {6, 5, 5}, {6, 5, 6}, {6, 5, 7}, {6, 5, 8}, {7, 6, 0}, {7, 6, 1}, {7, 6, 2}, {7, 6, 3}, {7, 6, 4}, {7, 6, 5}, {7, 6, 6}, {7, 6, 7}, {7, 6, 8},
        {8, 7, 0}, {8, 7, 1}, {8, 7, 2}, {8, 7, 3}, {8, 7, 4}, {8, 7, 5}, {8, 7, 6}, {8, 7, 7}, {8, 7, 8}, {9, 8, 0}, {9, 8, 1}, {9, 8, 2}, {9, 8, 3}, {9, 8, 4}, {9, 8, 5}, {9, 8, 6}, {9, 8, 7}, {9, 8, 8}, {10, 0, 0}, {10, 1, 0},
        {10, 2, 0}, {10, 3, 0}, {10, 4, 0}, {10, 5, 0}, {10, 6, 0}, {10, 7, 0}, {10, 8, 0}, {11, 0, 1}, {11, 1, 1}, {11, 2, 1}, {11, 3, 1}, {11, 4, 1}, {11, 5, 1}, {11, 6, 1}, {11, 7, 1}, {11, 8, 1}, {12, 0, 2}, {12, 1, 2},
        {12, 2, 2}, {12, 3, 2}, {12, 4, 2}, {12, 5, 2}, {12, 6, 2}, {12, 7, 2}, {12, 8, 2}, {13, 0, 3}, {13, 1, 3}, {13, 2, 3}, {13, 3, 3}, {13, 4, 3}, {13, 5, 3}, {13, 6, 3}, {13, 7, 3}, {13, 8, 3}, {14, 0, 4}, {14, 1, 4},
        {14, 2, 4}, {14, 3, 4}, {14, 4, 4}, {14, 5, 4}, {14, 6, 4}, {14, 7, 4}, {14, 8, 4}, {15, 0, 5}, {15, 1, 5}, {15, 2, 5}, {15, 3, 5}, {15, 4, 5}, {15, 5, 5}, {15, 6, 5}, {15, 7, 5}, {15, 8, 5}, {16, 0, 6}, {16, 1, 6},
        {16, 2, 6}, {16, 3, 6}, {16, 4, 6}, {16, 5, 6}, {16, 6, 6}, {16, 7, 6}, {16, 8, 6}, {17, 0, 7}, {17, 1, 7}, {17, 2, 7}, {17, 3, 7}, {17, 4, 7}, {17, 5, 7}, {17, 6, 7}, {17, 7, 7}, {17, 8, 7}, {18, 0, 8}, {18, 1, 8},
        {18, 2, 8}, {18, 3, 8}, {18, 4, 8}, {18, 5, 8}, {18, 6, 8}, {18, 7, 8}, {18, 8, 8}, {19, 0, 0}, {19, 0, 1}, {19, 0, 2}, {19, 1, 0}, {19, 1, 1}, {19, 1, 2}, {19, 2, 0}, {19, 2, 1}, {19, 2, 2}, {20, 0, 3}, {20, 0, 4},
        {20, 0, 5}, {20, 1, 3}, {20, 1, 4}, {20, 1, 5}, {20, 2, 3}, {20, 2, 4}, {20, 2, 5}, {21, 0, 6}, {21, 0, 7}, {21, 0, 8}, {21, 1, 6}, {21, 1, 7}, {21, 1, 8}, {21, 2, 6}, {21, 2, 7}, {21, 2, 8}, {22, 3, 0}, {22, 3, 1},
        {22, 3, 2}, {22, 4, 0}, {22, 4, 1}, {22, 4, 2}, {22, 5, 0}, {22, 5, 1}, {22, 5, 2}, {23, 3, 3}, {23, 3, 4}, {23, 3, 5}, {23, 4, 3}, {23, 4, 4}, {23, 4, 5}, {23, 5, 3}, {23, 5, 4}, {23, 5, 5}, {24, 3, 6}, {24, 3, 7},
        {24, 3, 8}, {24, 4, 6}, {24, 4, 7}, {24, 4, 8}, {24, 5, 6}, {24, 5, 7}, {24, 5, 8}, {25, 6, 0}, {25, 6, 1}, {25, 6, 2}, {25, 7, 0}, {25, 7, 1}, {25, 7, 2}, {25, 8, 0}, {25, 8, 1}, {25, 8, 2}, {26, 6, 3}, {26, 6, 4},
        {26, 6, 5}, {26, 7, 3}, {26, 7, 4}, {26, 7, 5}, {26, 8, 3}, {26, 8, 4}, {26, 8, 5}, {27, 6, 6}, {27, 6, 7}, {27, 6, 8}, {27, 7, 6}, {27, 7, 7}, {27, 7, 8}, {27, 8, 6}, {27, 8, 7}, {27, 8, 8}};
    // CodeStart
    public enum enSS 
    {
        //column definitions for mSolnSet
        ss = 0,
        //soln set
        r = 1,
        //cell r,c address
        c = 2
        //
    }

    //count all changes of any kind
    int mNoOfChanges;
    //counts naked singles
    int mNakedSingles;
    //counts hidden singles
    int mHiddenSingles;
    //counts naked pairs
    int mNakedPairs;
    //counts naked triples
    int mNakedTriples;
    //counts naked quads
    int mNakedQuads;
    //counts hidden pairs
    int mHiddenPairs;
    //counts locked boxes
    int mLockedBoxes;
    //counts locked rows
    int mLockedRows;
    //counts locked columns
    int mLockedCols;
    //counts hidden triples
    int mHiddenTriples;
    //counts hidden quads
    int mHiddenQuads;
    public int ChangeCount
    {
        get { return mNoOfChanges; }
    }
    public int NakedSingles
    {
        get { return mNakedSingles; }
    }
    public int HiddenSingles
    {
        get { return mHiddenSingles; }
    }
    public int NakedPairs
    {
        get { return mNakedPairs; }
    }
    public int NakedTriples
    {
        get { return mNakedTriples; }
    }
    public int NakedQuads
    {
        get { return mNakedQuads; }
    }
    public int HiddenPairs
    {
        get { return mHiddenPairs; }
    }
    public int LockedBoxes
    {
        get { return mLockedBoxes; }
    }
    public int LockedRows
    {
        get { return mLockedRows; }
    }
    public int LockedCols
    {
        get { return mLockedCols; }
    }
    public int HiddenTriples
    {
        get { return mHiddenTriples; }
    }
    public int HiddenQuads
    {
        get { return mHiddenQuads; }
    }

    //9.1 quick solve.  pass in 3 dimensional array  mSK(r,c,p)
    //9.1 cycle through all solution sets,
    //9.1 solve each one for naked singles only
    public void QuickSolve(ref int[, ,] skArg)
    {
        int ss = 0;
        int NSChanges = -1;
        int iterationsNeeded = 0;
        //
        // QuickSolved means removing all candidates in the every solution set set where there is a scalar already solved
        // in that solution set.  eg in a row 5 there is a solved cell containing a 7. Remove all candidates with the value 7
        // from all the other cells in that row.  
        //
        CopySK(skArg, ref mSK);
        while (NSChanges != 0)
        {
            for (ss = 1; ss <= mSSCNT; ss++)
            {
                if (UnSolved(ref ss)) NakedSingle(ref ss, ref NSChanges);
            }
            iterationsNeeded++;
        }   //iterate again if any changes made
        // myDebug("quicksolve iterations needed " + iterationsNeeded);  usually 1-2 iterations
        CopySK(mSK, ref skArg);
        //
    }
    //9.1 end of new feature
    public void NewIteration(ref int[, ,] skArg, int vIterations)
    {
        //
        // can be called n times to solve the puzzle
        //  for each solution set, try all techniques, just once
        //
        int ss = 0;
        int NSChanges = 0;
        int HSChanges = 0;
        int DSChanges = 0;
        int TSChanges = 0;
        int QSChanges = 0;
        int LBChanges = 0;
        int LRChanges = 0;
        int LCChanges = 0;
        int iBefore = 0;

        //
        //mSK = skArg     'get a copy of the current case
        CopySK(skArg, ref mSK);
        //
        //Solve puzzle one solution set at a time.  A sol'n set is a row, col or box
        //a bit slow but very good for the user experience...
        //
        for (ss = 1; ss <= mSSCNT; ss++)
        {
            iBefore = mNoOfChanges;
            if (UnSolved(ref ss))
            {
                NakedSingle(ref ss, ref NSChanges);
                HiddenSingle(ref ss, ref HSChanges);
                DoubleSets(ref ss, ref DSChanges);
                TripleSets(ref ss, ref TSChanges);
                QuadSets(ref ss, ref QSChanges);
                LockedBox(ref ss, ref LBChanges);
                LockedRow(ref ss, ref LRChanges);
                LockedCol(ref ss, ref LCChanges);
            }
            //if any changes made, return to user, except if we are doing a solveall
            if (vIterations < MAXITERATIONS & iBefore != mNoOfChanges)
            {
                break; 
            }
        }
        //
        //return the updated case back to calling object
        //
        CopySK(mSK, ref skArg);

    }
    private void NakedSingle(ref int ssArg, ref int ChangeCount)
    {
        //
        // eg, solved cell contains a 5, but there are other cells with 5 as a candidate
        // in the same solution set. Remove the 5 as a candidate in the other cells in the group
        //
        int ss = 0;
        int r = 0;
        int c = 0;
        int p = 0;
        int lNoOfChanges = 0;

        lNoOfChanges = mNoOfChanges;

        for (ss = mSSPtr[ssArg, (int)enmSSP.FromPtr]; ss <= mSSPtr[ssArg, (int)enmSSP.ToPtr]; ss++)
        {
            r = mSolnSet[ss, (int)enSS.r];
            c = mSolnSet[ss, (int)enSS.c];
            //is there one solution
            if (CandidateCount(ref r, ref c) == 1)
            {
                p = iGetSolvedP(ref r, ref c);
                //get the solved value
                RemoveCandidates(ref ssArg, ref p);
            }
        }

        ChangeCount = mNoOfChanges - lNoOfChanges;
        mNakedSingles = mNakedSingles + ChangeCount;

    }
    private void HiddenSingle(ref int ssArg, ref int ChangeCount)
    {
        //
        //   cell contains 5 6 7 as candidates. However the 5 only appears once in the soln set.
        //   thus remove the 6 and 7 from the list of candidates for this cell.
        //
        int ss = 0;
        int r = 0;
        int c = 0;
        int p = 0;
        int lNoOfChanges = 0;

        lNoOfChanges = mNoOfChanges;
        int[] pArray = new int[10];

        CountCandidates(ref ssArg, ref pArray);
        //
        for (p = 1; p <= 9; p++)
        {
            //we have a hidden single p,  somewhere
            if (pArray[p] == 1)
            {
                // p is set to the candidate to KEEP
                for (ss = mSSPtr[ssArg, (int)enmSSP.FromPtr]; ss <= mSSPtr[ssArg, (int)enmSSP.ToPtr]; ss++)
                {
                    r = mSolnSet[ss, (int)enSS.r];
                    c = mSolnSet[ss, (int)enSS.c];
                    if (mSK[r, c, p] == 1)
                    {
                        KeepCandidate(ref r, ref c, ref p);
                        break; 
                        //only act on one cell
                    }
                }
            }
            // p hit
        }

        ChangeCount = mNoOfChanges - lNoOfChanges;
        mHiddenSingles = mHiddenSingles + ChangeCount;

    }


    private void DoubleSets(ref int ssArg, ref int ChangeCount)
    {
        //
        //   find either naked or hidden pairs
        //
        int iPair = 0;
        int lNoOfChanges = 0;
        clsList mylist = default(clsList);
        int[] pValues = new int[5];
        string sType = null;
        int iPairsUB = miPairsDefn.GetUpperBound(0);
        lNoOfChanges = mNoOfChanges;
        //first scan look at all possible pairs
        //now zero based
        for (iPair = 0; iPair <= iPairsUB ; iPair++)
        {
            mylist = new clsList();
            pValues[1] = miPairsDefn[iPair, 0];
            //now zero based
            pValues[2] = miPairsDefn[iPair, 1];
            pValues[3] = 0;
            pValues[4] = 0;
            //end of list
            //check the ss for this pair
            sType = "";
            MatchSet(ref ssArg, ref sType, ref pValues, ref mylist);
            switch (sType)
            {
                case "Hidden":
                    mylist.KeepOnlyPs(ref mSK);
                    //keep only the pairs in each cell
                    mNoOfChanges = mNoOfChanges + mylist.ChangeCount;
                    mHiddenPairs = mHiddenPairs + mylist.ChangeCount;
                    break;
                case "Naked":
                    mylist.RemovePFromCellList(ref mSK);
                    mNoOfChanges = mNoOfChanges + mylist.ChangeCount;
                    mNakedPairs = mNakedPairs + mylist.ChangeCount;
                    break;
            }

            mylist = null;

        }

        mylist = null;
        ChangeCount = mNoOfChanges - lNoOfChanges;

    }
    private void TripleSets(ref int ssArg, ref int ChangeCount)
    {
        //
        //   find either naked or hidden triples
        //
        int iTrip = 0;
        int lNoOfChanges = 0;
        clsList mylist = default(clsList);
        int[] pValues = new int[5];
        string sType = null;
        int iTripUB = miTRIPDefn.GetUpperBound(0);
        lNoOfChanges = mNoOfChanges;
        //first scan look at all possible triplets
        for (iTrip = 0; iTrip <= iTripUB; iTrip++)
        {
            mylist = new clsList();
            pValues[1] = miTRIPDefn[iTrip, 0];
            pValues[2] = miTRIPDefn[iTrip, 1];
            pValues[3] = miTRIPDefn[iTrip, 2];
            pValues[4] = 0;
            //end of list
            //check the ss for this triplet
            sType = "";
            MatchSet(ref ssArg, ref sType, ref pValues, ref mylist);
            switch (sType)
            {
                case "Hidden":
                    mylist.KeepOnlyPs(ref mSK);
                    //keep only the triple in each cell
                    mNoOfChanges = mNoOfChanges + mylist.ChangeCount;
                    mHiddenTriples = mHiddenTriples + mylist.ChangeCount;
                    break;
                case "Naked":
                    mylist.RemovePFromCellList(ref mSK);
                    mNoOfChanges = mNoOfChanges + mylist.ChangeCount;
                    mNakedTriples = mNakedTriples + mylist.ChangeCount;
                    break;
            }
            mylist = null;

        }
        mylist = null;
        ChangeCount = mNoOfChanges - lNoOfChanges;

    }
    private void QuadSets(ref int ssArg, ref int ChangeCount)
    {
        //
        //   find either naked or hidden quad sets
        //
        int iTrip = 0;
        int lNoOfChanges = 0;
        clsList mylist = default(clsList);
        int[] pValues = new int[5];
        int iQuadUB = 0;
        string sType = null;
        lNoOfChanges = mNoOfChanges;
        //first scan look at all possible quads
        iQuadUB = mQuadDefn.GetUpperBound(0);

        for (iTrip = 0; iTrip <= iQuadUB; iTrip++)     
        {
            mylist = new clsList();
            pValues[1] = mQuadDefn[iTrip, 0];
            pValues[2] = mQuadDefn[iTrip, 1];
            pValues[3] = mQuadDefn[iTrip, 2];
            pValues[4] = mQuadDefn[iTrip, 3];
            //check the ss for this quad set
            sType = "";
            MatchSet(ref ssArg, ref sType, ref pValues, ref mylist);
            switch (sType)
            {
                case "Hidden":
                    mylist.KeepOnlyPs(ref mSK);
                    //keep only the quads in each cell
                    mNoOfChanges = mNoOfChanges + mylist.ChangeCount;
                    mHiddenQuads = mHiddenQuads + mylist.ChangeCount;
                    break;
                case "Naked":
                    mylist.RemovePFromCellList(ref mSK);
                    mNoOfChanges = mNoOfChanges + mylist.ChangeCount;
                    mNakedQuads = mNakedQuads + mylist.ChangeCount;
                    break;
            }

            mylist = null;

        }

        mylist = null;
        ChangeCount = mNoOfChanges - lNoOfChanges;

    }


    private void LockedBox(ref int ssArg, ref int ChangeCount)
    {
        //
        //

        int lNoOfChanges = 0;
        clsList mylist = default(clsList);
        string sIntersect = null;
        bool bLBFound = false;
        int p = 0;
        int iSS = 0;
        //intersecting solution set.

        lNoOfChanges = mNoOfChanges;
        //only applies to box type solution sets
        if (mSolnSetTypes[ssArg] != mBOXSS)
        {
            goto exit_sub;
        }

        for (p = 1; p <= 9; p++)
        {
            sIntersect = "";
            bLBFound = bLockedBox(ref ssArg, ref sIntersect, ref p);
            //is p locked in this box ?
            if (bLBFound)
            {
                mylist = new clsList();
                iSS = GetIntersectingVector(ref ssArg, ref sIntersect);
                CreateListNonIntersecting(ref ssArg, ref iSS, ref mylist);
                mylist.AddP(p);
                //   myList.PrintLists 'list look correct, puzzle 49
                mylist.RemovePFromCellList(ref mSK);
                mNoOfChanges = mNoOfChanges + mylist.ChangeCount;
                mLockedBoxes = mLockedBoxes + mylist.ChangeCount;
                mylist = null;
            }
        }
    exit_sub:
        ChangeCount = mNoOfChanges - lNoOfChanges;

    }
    private void LockedRow(ref int ssArg, ref int ChangeCount)
    {
        //
        // if a row contains either 2 or 3 p values and those numbers are found in the same row
        //   in the same box then the p values can be excluded from the other 2 rows in the box
        //
        int lNoOfChanges = 0;
        clsList mylist = default(clsList);
        string sIntersect = null;
        bool bLRFound = false;
        int p = 0;
        int iSS = 0;
        //intersecting solution set.

        lNoOfChanges = mNoOfChanges;
        //only applies to row type solution sets
        if (mSolnSetTypes[ssArg] != mROWSS)
        {
            goto exit_sub;
        }

        for (p = 1; p <= 9; p++)
        {
            sIntersect = "";
            bLRFound = bLockedVector(ref ssArg, ref sIntersect, ref p);
            //is p locked in this row ?
            if (bLRFound)
            {
                mylist = new clsList();
                iSS = GetIntersectingBox(ref ssArg, ref sIntersect);
                CreateListNonIntersecting(ref ssArg, ref iSS, ref mylist);
                mylist.AddP(p);
                //   myList.PrintLists 'list look correct, puzzle 49
                mylist.RemovePFromCellList(ref mSK);
                mNoOfChanges = mNoOfChanges + mylist.ChangeCount;
                mLockedRows = mLockedRows + mylist.ChangeCount;
                mylist = null;
            }
        }
    exit_sub:
        ChangeCount = mNoOfChanges - lNoOfChanges;

    }
    private void LockedCol(ref int ssArg, ref int ChangeCount)
    {
        //
        // if a column contains either 2 or 3 p values and those numbers are found in the same column
        //   in the same box then the p values can be excluded from the other 2 columns in the box
        //
        int lNoOfChanges = 0;
        clsList mylist = default(clsList);
        string sIntersect = null;
        bool bLCFound = false;
        int p = 0;
        int iSS = 0;
        //intersecting solution set.

        lNoOfChanges = mNoOfChanges;
        //only applies to column type solution sets
        if (mSolnSetTypes[ssArg] != mCOLSS)
        {
            goto exit_sub;
        }

        for (p = 1; p <= 9; p++)
        {
            sIntersect = "";
            bLCFound = bLockedVector(ref ssArg, ref sIntersect, ref p);
            //is p locked in this row ?
            if (bLCFound)
            {
                mylist = new clsList();
                iSS = GetIntersectingBox(ref ssArg, ref sIntersect);
                CreateListNonIntersecting(ref ssArg, ref iSS, ref mylist);
                mylist.AddP(p);
                //   myList.PrintLists 'list look correct, puzzle 49
                mylist.RemovePFromCellList(ref mSK);
                mNoOfChanges = mNoOfChanges + mylist.ChangeCount;
                mLockedCols = mLockedCols + mylist.ChangeCount;
                mylist = null;
            }
        }
    exit_sub:
        ChangeCount = mNoOfChanges - lNoOfChanges;

    }
    // =================================== support routines ===================
    private void MatchSet(ref int ssArg, ref string SetType, ref int[] pValues, ref clsList mylist)
    {
        //
        // pValues = list of pairs, tripletes or quads to analyze
        //   cCellPtrs (1..9) ptr to cell that we are counting the p's for
        //   InSetCnt (1..9) count of p's found that match the list of p's provided
        //   OutSetCnt (1..9) count of p's found that are not in the list of p's provided
        //   for ex if the pair we are looking for is 4 7
        //   the first cell contained p values  13457
        //   then InSetCnt(1) would return 2     (ie 4 & 7)
        //        OutSetCnt(1) would return 3    (ie 1,3 and 5)
        //
        int pCnt = 0;
        //either 2 (pairs), 3(triplets), 4 quads
        int i = 0;
        int cellPtr = 0;
        int iCell = 0;
        int iHidden = 0;
        int iNaked = 0;
        int r = 0;
        int c = 0;
        int p = 0;
        int ip = 0;
        int jp = 0;
        bool InSet = false;
        int[] InSetCnts = new int[10];
        int[] OutSetCnts = new int[10];
        int[] CellPtrs = new int[10];

        SetType = "none";
        pCnt = 0;
        //just a 0 terminated list
        for (i = 1; i <= 4; i++)
        {
            //grab list of p values to test
            if (pValues[i] != 0)
            {
                pCnt = pCnt + 1;
                mylist.AddP(pValues[i]);
                //keep dup copy of p list in the object
            }
        }
        //if (pCnt == 4)  csNote
        //{
        //    pCnt = pCnt;
        //}

        // if any p value has already been solved then the set is not a valid set
        for (p = 1; p <= pCnt; p++)
        {
            if (CountOccurances(ref ssArg, ref pValues[p]) == 1)
            {
                return;
            }
        }
        //
        CellPtrs.Initialize();
        InSetCnts.Initialize();
        OutSetCnts.Initialize();

        iCell = 0;
        //first scan we have at least 2 cells for every p value passed
        for (cellPtr = mSSPtr[ssArg, (int)enmSSP.FromPtr]; cellPtr <= mSSPtr[ssArg, (int)enmSSP.ToPtr]; cellPtr++)
        {
            r = mSolnSet[cellPtr, (int)enSS.r];
            c = mSolnSet[cellPtr, (int)enSS.c];
            iCell = iCell + 1;
            CellPtrs[iCell] = cellPtr;
            //look at all p values
            for (jp = 1; jp <= 9; jp++)
            {
                InSet = false;
                for (ip = 1; ip <= pCnt; ip++)
                {
                    p = pValues[ip];
                    if (p == jp)
                    {
                        InSet = true;
                    }
                }
                if (InSet)
                {
                    InSetCnts[iCell] = InSetCnts[iCell] + mSK[r, c, jp];
                }
                else
                {
                    OutSetCnts[iCell] = OutSetCnts[iCell] + mSK[r, c, jp];
                }
            }

        }
        // now determine what kind of match we found
        //
        iHidden = 0;
        iNaked = 0;
        for (i = 1; i <= 9; i++)
        {
            if (InSetCnts[i] > 0)
            {
                iHidden = iHidden + 1;
                //if 3 p values only in 3 cells they are hidden
            }
            if (OutSetCnts[i] == 0)
            {
                iNaked = iNaked + 1;
                //if no other 's are in
            }
        }

        if (iHidden == pCnt & iNaked == pCnt)
        {
            SetType = "Solved";
            //don't provide a cell list
            return;
        }

        if (iHidden == pCnt)
        {
            SetType = "Hidden";
            //cell list is in the InSet
            for (i = 1; i <= 9; i++)
            {
                if (InSetCnts[i] > 0)
                {
                    r = mSolnSet[CellPtrs[i], (int)enSS.r];
                    c = mSolnSet[CellPtrs[i], (int)enSS.c];
                    mylist.AddCell(r, c);
                }
            }
        }
        if (iNaked == pCnt)
        {
            SetType = "Naked";
            //cell list is in the OutSet
            for (i = 1; i <= 9; i++)
            {
                if (OutSetCnts[i] > 0)
                {
                    r = mSolnSet[CellPtrs[i], (int)enSS.r];
                    c = mSolnSet[CellPtrs[i], (int)enSS.c];
                    mylist.AddCell(r, c);
                }
            }
        }

    }
    private void CreateListNonIntersecting(ref int ssArg, ref int iSSArg, ref clsList mylist)
    {
        //
        //from the two solution sets generate a list of cells that are in in the intersecting
        //solution set but not in the original solution set. Used to zap all cells that are
        //in locked in a row or column
        //
        int ss = 0;
        int r = 0;
        int c = 0;
        //
        for (ss = mSSPtr[iSSArg, (int)enmSSP.FromPtr]; ss <= mSSPtr[iSSArg, (int)enmSSP.ToPtr]; ss++)
        {
            r = mSolnSet[ss, (int)enSS.r];
            c = mSolnSet[ss, (int)enSS.c];
            //now does this r,c exist in the original ss
            if (!bIsInTheSS(ref ssArg, ref r, ref c))
            {
                mylist.AddCell(r, c);
            }
        }
    }
    private bool bIsInTheSS(ref int ssArg, ref int rArg, ref int cArg)
    {
        bool functionReturnValue = false;
        // return true if the supplied r,c is in the specified solution set
        int ss = 0;
        int r = 0;
        int c = 0;
        //
        functionReturnValue = false;
        for (ss = mSSPtr[ssArg, (int)enmSSP.FromPtr]; ss <= mSSPtr[ssArg, (int)enmSSP.ToPtr]; ss++)
        {
            r = mSolnSet[ss, (int)enSS.r];
            c = mSolnSet[ss, (int)enSS.c];
            if (r == rArg & c == cArg)
            {
                functionReturnValue = true;
                return functionReturnValue;
            }
        }
        return functionReturnValue;
    }


    private void KeepCandidate(ref int rArg, ref int cArg, ref int pArg)
    {
        //
        // removes all candidates except the specified one from the cell
        //
        int p = 0;

        //dont touch solved cells
        if (CandidateCount(ref rArg, ref cArg) == 1)
        {
            return;
        }
        for (p = 1; p <= 9; p++)
        {
            //keep p only
            if (p != pArg)
            {
                //currently a candidate
                if (mSK[rArg, cArg, p] == 1)
                {
                    mSK[rArg, cArg, p] = 0;
                    //no longer a candidate
                    mNoOfChanges = mNoOfChanges + 1;
                }
            }
        }

    }
    private void CountCandidates(ref int ssArg, ref int[] pArray)
    {
        //
        // for all not solved cells counts the number of times each candidate occurs
        //
        int p = 0;
        int ss = 0;
        int r = 0;
        int c = 0;

        //
        for (p = 1; p <= 9; p++)
        {
            pArray[p] = 0;
            //zero counters
        }

        for (ss = mSSPtr[ssArg, (int)enmSSP.FromPtr]; ss <= mSSPtr[ssArg, (int)enmSSP.ToPtr]; ss++)
        {
            r = mSolnSet[ss, (int)enSS.r];
            c = mSolnSet[ss, (int)enSS.c];
            for (p = 1; p <= 9; p++)
            {
                pArray[p] = pArray[p] + mSK[r, c, p];
            }
        }
    }

    private int iGetSolvedP(ref int rArg, ref int cArg)
    {
        int functionReturnValue = 0;
        //
        //call iff current cell has one and only one solution,  return it
        //
        int i = 0;
        functionReturnValue = -1;
        for (i = 1; i <= 9; i++)
        {
            if (mSK[rArg, cArg, i] == 1)
            {
                functionReturnValue = i;
                break; 
            }
        }

        if (functionReturnValue == -1)
        {
            // cause a hard crash, logic error should not return here.
            myDebug("In iGetSolvedP Serious logic error, should not occur");
        }
        return functionReturnValue;

    }



    private bool UnSolved(ref int ssArg)
    {
        bool functionReturnValue = false;
        //for speed only do not try to solve a ss already completed
        int r = 0;
        int c = 0;
        int ss = 0;

        functionReturnValue = false;
        for (ss = mSSPtr[ssArg, (int)enmSSP.FromPtr]; ss <= mSSPtr[ssArg, (int)enmSSP.ToPtr]; ss++)
        {
            r = mSolnSet[ss, (int)enSS.r];
            c = mSolnSet[ss, (int)enSS.c];
            if (CandidateCount(ref r, ref c) != 1)
            {
                functionReturnValue = true;
                return functionReturnValue;
            }
        }
        return functionReturnValue;

    }

    private bool bLockedBox(ref int ssArg, ref string sArgIntersect, ref int pArg)
    {
        bool functionReturnValue = false;
        //
        //   now p is passed, look to see if p is locked in this group
        //
        // find any row or col in the passed group/box that contains the p value passed
        // in any other row or col in the group/box.
        //  Return the relative r or c of where the p values were found
        // eg  - - 35
        //     - - 45
        //     - - -   The 5's are locked in c3, return c3 and 5
        // tested using puzzle 49
        //
        string ssMap = null;
        int ss = 0;
        int r = 0;
        int c = 0;
        int pCnt = 0;
        const int ssLEN = 9;
        string[] BlockMask = new string[7];
        string[] sIntersect = new string[7];
        int i = 0;
        int iRslt = 0;
        //
        functionReturnValue = false;
        sArgIntersect = "";

        BlockMask[1] = "111000000";
        // row 1 in the 3*3 block
        BlockMask[2] = "000111000";
        // row 2
        BlockMask[3] = "000000111";
        // row 3
        BlockMask[4] = "100100100";
        // col 1
        BlockMask[5] = "010010010";
        // col 2
        BlockMask[6] = "001001001";
        // col 3

        sIntersect[1] = "r0";
        sIntersect[2] = "r1";
        sIntersect[3] = "r2";
        sIntersect[4] = "c0";
        sIntersect[5] = "c1";
        sIntersect[6] = "c2";

        pCnt = 0;
        ssMap = "";
        for (ss = mSSPtr[ssArg, (int)enmSSP.FromPtr]; ss <= mSSPtr[ssArg, (int)enmSSP.ToPtr]; ss++)
        {
            r = mSolnSet[ss, (int)enSS.r];
            c = mSolnSet[ss, (int)enSS.c];
            ssMap = ssMap + mSK[r, c, pArg];
            //create bit map for this p
            pCnt = pCnt + mSK[r, c, pArg];
            //how many p's have we found
        }
        //not locked if only one
        if (pCnt > 1)
        {
            //
            //must check against all 6 possibilities
            for (i = 1; i <= 6; i++)
            {
                iRslt = MaskOR(ssMap, BlockMask[i], ssLEN);
                if (iRslt == 3)
                {
                    sArgIntersect = sIntersect[i];
                    functionReturnValue = true;
                    return functionReturnValue;
                    //Parg has been found to be locked in the box
                }
            }
            //try another mask
        }
        return functionReturnValue;

    }
    private bool bLockedVector(ref int ssArg, ref string sArgIntersect, ref int pArg)
    {
        bool functionReturnValue = false;
        //
        //   a vector is a row or column ss. determine if any p values only exist
        //   in one intersecting box.
        //
        string ssMap = null;
        int ss = 0;
        int r = 0;
        int c = 0;
        int pCnt = 0;
        const int ssLEN = 9;
        string[] BlockMask = new string[4];
        string[] sIntersect = new string[4];
        int i = 0;
        int iRslt = 0;
        //
        functionReturnValue = false;
        sArgIntersect = "";

        BlockMask[1] = "111000000";
        // mask left box
        BlockMask[2] = "000111000";
        // mask middle box
        BlockMask[3] = "000000111";
        // mask right box

        sIntersect[1] = "b0";
        sIntersect[2] = "b1";
        sIntersect[3] = "b2";

        pCnt = 0;
        ssMap = "";
        for (ss = mSSPtr[ssArg, (int)enmSSP.FromPtr]; ss <= mSSPtr[ssArg, (int)enmSSP.ToPtr]; ss++)
        {
            r = mSolnSet[ss, (int)enSS.r];
            c = mSolnSet[ss, (int)enSS.c];
            ssMap = ssMap + mSK[r, c, pArg];
            //create bit map for this p
            pCnt = pCnt + mSK[r, c, pArg];
            //how many p's have we found
        }
        //not locked if only one
        if (pCnt > 1)
        {
            //
            //must check against all 3 possibilities
            for (i = 1; i <= 3; i++)
            {
                iRslt = MaskOR(ssMap, BlockMask[i], ssLEN);
                if (iRslt == 3)
                {
                    sArgIntersect = sIntersect[i];
                    functionReturnValue = true;
                    return functionReturnValue;
                    //Parg has been found to be locked in the row
                }
            }
            //try another mask
        }
        return functionReturnValue;

    }

    private int MaskOR(string Map, string Mask, int iLen)
    {
        int functionReturnValue = 0;
        //simple ORing of two same length strings, whose values must be O's or 1's
        int i = 0;
        int OrResult = 0;
        functionReturnValue = 0;
        //old style
        //For i = 1 To iLen
        //    OrResult = Mid(Map, i, 1) Or Mid(Mask, i, 1)
        //    MaskOR = MaskOR + OrResult
        //Next i
        for (i = 0; i <= iLen - 1; i++)
        {
            //note string objects must be byVal
            OrResult = Convert.ToInt16(Map.Substring(i, 1)) | Convert.ToInt16(Mask.Substring(i, 1));
            functionReturnValue = functionReturnValue + OrResult;
        }
        return functionReturnValue;

    }
    private int GetIntersectingBox(ref int ssArg, ref string sArgIntersect)
    {
        //
        // returns box ss that intersects the specified row or col vector
        //
        int iSS = 0;
        int i = 0;
        int iRowSSUB = mRowSSMap.GetUpperBound(0);
        int iColSSUB = mColSSMap.GetUpperBound(0);
        iSS = 0;
        //process box's that intersect rows

        if (mSolnSetTypes[ssArg] == mROWSS)
        {
            for (i = 0; i <= iColSSUB ; i++)
            {
                if (mRowSSMap[i, (int)enrSSM.ss] == ssArg)
                {
                    switch (sArgIntersect)
                    {
                        case "b0":
                            iSS = mRowSSMap[i, (int)enrSSM.b0];
                            break;
                        case "b1":
                            iSS = mRowSSMap[i, (int)enrSSM.b1];
                            break;
                        case "b2":
                            iSS = mRowSSMap[i, (int)enrSSM.b2];
                            break;
                    }
                }
            }
        }
        //process box's that intersect columns

        if (mSolnSetTypes[ssArg] == mCOLSS)
        {
            for (i = 0; i <= 8; i++)
            {
                if (mColSSMap[i, (int)encSSM.ss] == ssArg)
                {
                    switch (sArgIntersect)
                    {
                        case "b0":
                            iSS = mColSSMap[i, (int)encSSM.b0];
                            break;
                        case "b1":
                            iSS = mColSSMap[i, (int)encSSM.b1];
                            break;
                        case "b2":
                            iSS = mColSSMap[i, (int)encSSM.b2];
                            break;
                    }
                }
            }
        }

        //must be a box type
        if (iSS < 19 | iSS > 27)
        {
            myDebug("bad intersect ss returned " + iSS + " " + ssArg + " " + sArgIntersect);
        }
        return iSS;

    }
    private int GetIntersectingVector(ref int ssArg, ref string sArgIntersect)
    {
        //
        // returns the row or column that intersects with the specified box
        //
        int iSS = 0;
        int i = 0;
        int iBoxSSUB = mBoxSSMap.GetUpperBound(0);

        iSS = 0;
        for (i = 0; i <= iBoxSSUB ; i++)
        {
            if (mBoxSSMap[i, (int)enBoxSSMap.ss] == ssArg)
            {
                switch (sArgIntersect)
                {
                    case "c0":
                        iSS = mBoxSSMap[i, (int)enBoxSSMap.C0];
                        break;
                    case "c1":
                        iSS = mBoxSSMap[i, (int)enBoxSSMap.c1];
                        break;
                    case "c2":
                        iSS = mBoxSSMap[i, (int)enBoxSSMap.c2];
                        break;
                    case "r0":
                        iSS = mBoxSSMap[i, (int)enBoxSSMap.R0];
                        break;
                    case "r1":
                        iSS = mBoxSSMap[i, (int)enBoxSSMap.r1];
                        break;
                    case "r2":
                        iSS = mBoxSSMap[i, (int)enBoxSSMap.r2];
                        break;
                }
            }
        }
        if (iSS < 1 | iSS > 18)
        {
            myDebug("bad intersect ss returned " + iSS + " " + ssArg + " " + sArgIntersect);
        }

        return iSS;

    }
    private void RemoveCandidate(ref int rArg, ref int cArg, ref int pArg)
    {
        //
        // removes a single candidate from the specified cell
        // Warning - same code exists in other classes
        //
        //dont touch solved cells
        if (CandidateCount(ref rArg, ref cArg) > 1)
        {
            //currently a candidate
            if (mSK[rArg, cArg, pArg] == 1)
            {
                mSK[rArg, cArg, pArg] = 0;
                //no longer a candidate
                mNoOfChanges = mNoOfChanges + 1;
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
    private void RemoveCandidates(ref int ssArg, ref int pArg)
    {
        //
        // from the solution set remove pArg as possible candidates from all cells,
        // except of course from the single cell containing only pArg
        //
        int ss = 0;
        int r = 0;
        int c = 0;

        for (ss = mSSPtr[ssArg, (int)enmSSP.FromPtr]; ss <= mSSPtr[ssArg, (int)enmSSP.ToPtr]; ss++)
        {
            r = mSolnSet[ss, (int)enSS.r];
            c = mSolnSet[ss, (int)enSS.c];
            RemoveCandidate(ref r, ref c, ref pArg);
        }

    }

    private int CountOccurances(ref int ssArg, ref int pArg)
    {
        //
        //count how many times p is a candidate in the entire solution set
        //
        int ss = 0;
        int r = 0;
        int c = 0;
        int pCnt = 0;
        pCnt = 0;
        for (ss = mSSPtr[ssArg, (int)enmSSP.FromPtr]; ss <= mSSPtr[ssArg, (int)enmSSP.ToPtr]; ss++)
        {
            r = mSolnSet[ss, (int)enSS.r];
            c = mSolnSet[ss, (int)enSS.c];
            pCnt = pCnt + mSK[r, c, pArg];
            //count the p's that are candidates
        }
        return pCnt;
    }
    private void CopySK(int[, ,] skFrom, ref int[, ,] skTo)
    {
        //make a clone of the provided sk array
        int r = 0;
        int c = 0;
        int p = 0;
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                for (p = 1; p <= 9; p++)
                {
                    skTo[r, c, p] = skFrom[r, c, p];
                }
            }
        }
    }
    public bool IsThisCaseValid(int[, ,] skArg)     //csNote possibly a ref ? was a property get
    {
            //
            //validate case ss by ss, return true if all ok
            //need at least 17 scalars in entire puzzle.
            //each soln set must not have the same scalar more than once.
            //
            bool bOK = false;
            bOK = false;
            int ss = 0;
            int r = 0;
            int c = 0;
            int p = 0;
            int pTotal = 0;
            int pScalar = 0;
            pScalar = 0;
            for (r = 0; r <= 8; r++)
            {
                for (c = 0; c <= 8; c++)
                {
                    pTotal = 0;
                    for (p = 1; p <= 9; p++)
                    {
                        pTotal = pTotal + skArg[r, c, p];
                    }
                    if (pTotal == 1)
                    {
                        pScalar = pScalar + 1;
                    }
                }
            }
            if (pScalar >= 17)
            {
                //
                for (ss = 1; ss <= mSSCNT; ss++)
                {
                    bOK = bValidateSS(skArg, ss);
                    if (!bOK)
                    {
                        break; 
                    }
                }
            }
            return bOK;
    }
    private bool bValidateSS(int[, ,] skArg, int ssArg)
    {
        bool functionReturnValue = false;
        // check for more than one p of the same value in the soln set
        int r = 0;
        int c = 0;
        int pi = 0;
        //p index
        int p = 0;
        int ss = 0;
        int[] PCnts = new int[10];
        int iCandidateCnt = 0;
        int iCandidate = 0;

        //
        functionReturnValue = true;
        PCnts.Initialize();
        for (ss = mSSPtr[ssArg, (int)enmSSP.FromPtr]; ss <= mSSPtr[ssArg, (int)enmSSP.ToPtr]; ss++)
        {
            r = mSolnSet[ss, (int)enSS.r];
            c = mSolnSet[ss, (int)enSS.c];
            //
            iCandidateCnt = 0;
            for (p = 1; p <= 9; p++)
            {
                iCandidateCnt = iCandidateCnt + skArg[r, c, p];
                //p is 1 one it is a possibility
                if (skArg[r, c, p] == 1)
                {
                    iCandidate = p;
                    //remember which p value
                }
            }
            //only look at cells with only one candidate
            //is there 1 p value
            if (iCandidateCnt == 1)
            {
                PCnts[iCandidate] = PCnts[iCandidate] + 1;
            }
        }
        //
        // there should never be more that one cell in a ss containing the same
        // single p value.
        //
        for (pi = 1; pi <= 9; pi++)
        {
            if (PCnts[pi] > 1)
            {
                functionReturnValue = false;
            }
        }
        return functionReturnValue;
    }
    private void myDebug(string sMess)
    {
        //Redirect all Output Window text to the Immediate Window" checked under the menu Tools > Options > Debugging > General.
        //System.Diagnostics.Debug.WriteLine(sMess); 
    }
}
