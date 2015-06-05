using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
public class clsNineByNine
{
    // this class provides solutions for techniques that involve looking
    // at the entire grid as opposed to one soluton set (row, column or box).
    //  so far Swordfish and x-wing
    //  it solves for only one p value
    //
    private int mVarP;

    //same offset as main sk array,
    //cell values are 0 or 1, representing the presence of p in that cell
    //
    //input, not changed
    private int[,] mGridCopy = new int[9, 9];
    //output, used to update sk array
    private int[,] mResult = new int[9, 9];
    private int[] mRowTotals = new int[9];
    private int[] mColTotals = new int[9];
    private const int mXWSCnt = 4;
    //one entry for x-wing found
    private int mXWSPtr;
    //won't be 9 unless data is bad
    private int[,] mXWingSets = new int[10, 5];
    public enum enXWS
    {
        //
        r1 = 1,
        //row where the pair exist
        r2 = 2,
        c1 = 3,
        //intersecting columns
        c2 = 4
    }

    private int mChangeCount;

    //up to 9 possible p values can be defined
    private int[] mPList = new int[10];


    public int pValue
    {
        set { mVarP = value; }
    }

    public int ChangeCount
    {
        get { return mChangeCount; }
    }

    public void BuildMatrix(ref int[, ,] skArg)
    {
        //
        // build a copy of grid, we will be making intermediate changes
        //   values are either 1 or 0, not the p value
        //
        int r = 0;
        int c = 0;
        int p = 0;

        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                p = skArg[r, c, mVarP];
                mGridCopy[r, c] = p;
            }
        }

        CalcTotals();

    }

    public void Xwing(ref int[, ,] skArg)
    {
        int r = 0;
        int c = 0;


        mChangeCount = 0;
        XWingRow();
        XWingCol();
        //all changes are local until now, go and update master array
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                if (skArg[r, c, mVarP] != mGridCopy[r, c])
                {
                    skArg[r, c, mVarP] = mGridCopy[r, c];
                    //only 1's and 0's
                    mChangeCount = mChangeCount + 1;
                }
            }
        }

    }
    private void XWingRow()
    {
        //
        // need to detect multiple  x-wing patterns in the same grid
        // need two rows wiR1h two 1's in exactly 2 columns,
        //
        int iR1 = 0;
        //1st x wing row
        int iR2 = 0;
        //2nd x wing row
        int c1a = 0;
        int c1b = 0;
        int c2a = 0;
        int c2b = 0;
        //
        for (iR1 = 1; iR1 <= mXWSCnt; iR1++)
        {
            mXWingSets[iR1, (int)enXWS.r1] = 0;
            mXWingSets[iR1, (int)enXWS.r2] = 0;
            mXWingSets[iR1, (int)enXWS.c1] = 0;
            mXWingSets[iR1, (int)enXWS.c2] = 0;
        }
        mXWSPtr = 0;

        for (iR1 = 0; iR1 <= 7; iR1++)
        {
            //1st test
            if (mRowTotals[iR1] == 2)
            {
                // now check the remaining rows for another row with
                // exactly the same columns being occupied.
                //        -----------------
                //        c1a       c2a      ir1
                //        -----------------
                //        c1b       c2b      ir2
                GetXWCols(iR1, ref c1a, ref c2a);
                for (iR2 = iR1 + 1; iR2 <= 8; iR2++)
                {
                    if (mRowTotals[iR2] == 2)
                    {
                        GetXWCols(iR2, ref c1b, ref c2b);
                        if (c1a == c1b & c2a == c2b)
                        {
                            mXWSPtr = mXWSPtr + 1;
                            mXWingSets[mXWSPtr, (int)enXWS.r1] = iR1;
                            mXWingSets[mXWSPtr, (int)enXWS.r2] = iR2;
                            mXWingSets[mXWSPtr, (int)enXWS.c1] = c1a;
                            mXWingSets[mXWSPtr, (int)enXWS.c2] = c2a;
                        }
                    }
                }
            }

        }
        //
        //no xwings detected
        if (mXWSPtr == 0)
        {
            return;
        }

        //can be multiple x-wins sets, solve one by one in the temp array
        for (iR1 = 1; iR1 <= mXWSPtr; iR1++)
        {
            SolveXwing(ref mXWingSets[iR1, (int)enXWS.r1], ref mXWingSets[iR1, (int)enXWS.r2], ref mXWingSets[iR1, (int)enXWS.c1], ref mXWingSets[iR1, (int)enXWS.c2]);
        }

        //   mydebug "post solving xwing"
        //   Call PrintNineByNine
    }
    private void XWingCol()
    {
        //
        // need to detect multiple  x-wing patterns in the same grid
        // need two columns wiC1h two 1's in exactly 2 rows,
        //
        int iC1 = 0;
        //1st x wing col
        int iC2 = 0;
        //2nd x wing col
        int r1a = 0;
        int r1b = 0;
        int r2a = 0;
        int r2b = 0;
        //
        for (iC1 = 1; iC1 <= mXWSCnt; iC1++)
        {
            mXWingSets[iC1, (int)enXWS.r1] = 0;
            mXWingSets[iC1, (int)enXWS.r2] = 0;
            mXWingSets[iC1, (int)enXWS.c1] = 0;
            mXWingSets[iC1, (int)enXWS.c2] = 0;
        }
        mXWSPtr = 0;

        for (iC1 = 0; iC1 <= 7; iC1++)
        {
            //1st test
            if (mColTotals[iC1] == 2)
            {
                // now check the remaining rows for another row with
                // exactly the same columns being occupied.
                GetXWRows(iC1, ref r1a, ref r2a);
                for (iC2 = iC1 + 1; iC2 <= 8; iC2++)
                {
                    if (mColTotals[iC2] == 2)
                    {
                        GetXWRows(iC2, ref r1b, ref r2b);
                        if (r1a == r1b & r2a == r2b)
                        {
                            mXWSPtr = mXWSPtr + 1;
                            mXWingSets[mXWSPtr, (int)enXWS.r1] = r1a;
                            mXWingSets[mXWSPtr, (int)enXWS.r2] = r2a;
                            mXWingSets[mXWSPtr, (int)enXWS.c1] = iC1;
                            mXWingSets[mXWSPtr, (int)enXWS.c2] = iC2;
                        }
                    }
                }
            }

        }
        //
        //no xwings detected
        if (mXWSPtr == 0)
        {
            return;
        }

        //can by multiple x-wins sets, solve one by one in the temp array
        for (iC1 = 1; iC1 <= mXWSPtr; iC1++)
        {
            SolveXwing(ref mXWingSets[iC1, (int)enXWS.r1], ref mXWingSets[iC1, (int)enXWS.r2], ref mXWingSets[iC1, (int)enXWS.c1], ref mXWingSets[iC1, (int)enXWS.c2]);
        }

        //   mydebug "post solving xwing"
        //   Call PrintNineByNine
    }
    private void SolveXwing(ref int r1, ref int r2, ref int c1, ref int c2)
    {
        //
        //   in the two rows and two columns defined, keep the p value at
        //   the intersection (ie 4 of them) and remove the p value from everywher
        //   else in the 2 rows and 2 columns.  Don't touch the rest of the grid.
        //   should work for xwing rows and column situations
        //
        int r = 0;
        int c = 0;

        //row 1 & row 2
        for (c = 0; c <= 8; c++)
        {
            if (c == c1 | c == c2)
            {
                //leave it
            }
            else
            {
                UpdateGrid(r1, c, 0);
                UpdateGrid(r2, c, 0);
            }
        }

        //col 1 and col 2
        for (r = 0; r <= 8; r++)
        {
            if (r == r1 | r == r2)
            {
                //leave it
            }
            else
            {
                UpdateGrid(r, c1, 0);
                UpdateGrid(r, c2, 0);
            }
        }
        //
        CalcTotals();

    }
    public void SwordFish(ref int[, ,] skArg)
    {
        int r = 0;
        int c = 0;


        mChangeCount = 0;
        SwordFishCol();
        SwordFishRow();
        //all changes are local until now, go and update master array
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                if (skArg[r, c, mVarP] != mGridCopy[r, c])
                {
                    skArg[r, c, mVarP] = mGridCopy[r, c];
                    //only 1's and 0's
                    mChangeCount = mChangeCount + 1;
                }
            }
        }

    }
    private void SwordFishCol()
    {
        //
        // look for swordfish pattern for columns that have the same values
        // in 2 or 3 rows
        //
        int[] ColList = new int[10];
        //list of target cols
        uint[] uiRowMaps = new uint[10];
        //parallel list of row maps

        int clCnt = 0;
        int clPtr = 0;

        uint uiBitMap = 0;
        int c = 0;
        int r = 0;

        uint uiMapA = 0;
        uint uiMapB = 0;
        uint uiMapC = 0;

        int i = 0;
        int j = 0;
        int k = 0;
        bool bSwordFish = false;
        uint uiBitsHit = 0;
        //                                012345678  row or column number
        uint uiRowHitMask = 0;
        //100000001  means rows 0 and 8
        uint uiColHitMask = 0;
        //010000010  means cols 1 and 7

        clCnt = 0;
        //first test is sum of p's for the col must be at 2 or 3
        //build list of columns that contain the p value at l2 or 3 times
        //
        for (c = 0; c <= 8; c++)
        {
            if (mColTotals[c] == 2 | mColTotals[c] == 3)
            {
                clCnt = clCnt + 1;
                ColList[clCnt] = c;
                //list of target columns
            }
        }
        //need at least 3 columns or there is no swordfish
        if (clCnt < 3)
        {
            return;
        }
        //
        // for each target column build a bitmap of the rows occupied in the col
        //  eg in the specified column rows 5 and 8 contain the same p value
        //
        for (clPtr = 1; clPtr <= clCnt; clPtr++)
        {
            c = ColList[clPtr];
            uiBitMap = 0;
            //rows 0 and 9 map is 1000000001
            for (r = 0; r <= 8; r++)
            {
                //rows 5 and 8 bit map is 0000010010
                if (mGridCopy[r,c] == 1)
                {
                    uiBitMap = uiTurnBitOn(uiBitMap, r);
                }
            }
            uiRowMaps[clPtr] = uiBitMap;
        }
        //
        // now have a list of target columns and row maps for each col
        // magically, by Or's the bit maps we will find the 3 columns that contain
        // p values in the same 3 rows, thus creating the swordfish
        // build bit maps corresponding to the rows and columns where
        // the swordfish has been found
        //
        uiColHitMask = 0;
        uiRowHitMask = 0;

        bSwordFish = false;

        for (i = 1; i <= clCnt - 2; i++)
        {
            for (j = i + 1; j <= clCnt - 1; j++)
            {

                for (k = j + 1; k <= clCnt; k++)
                {
                    uiMapA = uiRowMaps[i];
                    uiMapB = uiRowMaps[j];
                    uiMapC = uiRowMaps[k];
                    //
                    uiBitsHit = uiMapA | uiMapB | uiMapC;
                    uiBitsHit = (uint)iBitCount(uiBitsHit);

                    //found 3 col's
                    if (uiBitsHit == 3)
                    {
                        uiRowHitMask = uiMapA | uiMapB | uiMapC;
                        //turn on bit collist(n) in uicolhitmask
                        uiColHitMask = uiTurnBitOn(uiColHitMask, ColList[i]);
                        uiColHitMask = uiTurnBitOn(uiColHitMask, ColList[j]);
                        uiColHitMask = uiTurnBitOn(uiColHitMask, ColList[k]);
                        bSwordFish = true;
                        //found one swordfish, get out and solve it, faster and clean
                        goto Found_First_one;
                    }
                }
            }
        }
    Found_First_one:
        //
        if (bSwordFish)
        {
            //
            // now remove p from all cells in the defined rows,
            //  NOT intersected by the specified columns
            //
            //the row and col masks indicate all cells to update on the grid
            for (r = 0; r <= 8; r++)
            {
                //if the bit is turned on for a row
                if (uiIsBitOn(uiRowHitMask, r))
                {
                    for (c = 0; c <= 8; c++)
                    {
                        //then update grid for every column with a 1
                        if (!uiIsBitOn(uiColHitMask, c))
                        {
                            UpdateGrid(r, c, 0);
                        }
                    }
                }
            }

        }


    } //end swordfilecol
    private void SwordFishRow()
    {
        //
        // look for sworkfish pattern in rows that thave the same values in
        // 2 or 3 columns
        //
        int[] RowList = new int[10];
        //list of target rows
        uint[] uiColMaps = new uint[10];
        //parallel list of col maps
        int rwCnt = 0;
        int rwPtr = 0;

        uint uiBitMap = 0;
        int c = 0;
        int r = 0;

        uint uiMapA = 0;
        uint uiMapB = 0;
        uint uiMapC = 0;


        int i = 0;
        int j = 0;
        int k = 0;
        bool bSwordFish = false;
        uint uiBitsHit = 0;

        uint uiRowHitMask = 0;
        //100000001  means rows 0 and 8
        uint uiColHitMask = 0;
        //010000010  means cols 1 and 7

        rwCnt = 0;
        //first test is sum of p's for the row must be 2 or 3
        for (r = 0; r <= 8; r++)
        {
            if (mRowTotals[r] == 2 | mRowTotals[r] == 3)
            {
                rwCnt = rwCnt + 1;
                RowList[rwCnt] = r;
                //list of target rows
            }
        }
        //need at least 3 or there is no swordfish
        if (rwCnt < 3)
        {
            return;
        }
        //
        // for each target row build a bitmap of the cols occupied in the row
        //  eg in the specified row cols 5 and 8 contain the same p value
        //
        for (rwPtr = 1; rwPtr <= rwCnt; rwPtr++)
        {
            uiBitMap = 0;
            r = RowList[rwPtr];
            //cols 0 and 9 map is 1000000001
            for (c = 0; c <= 8; c++)
            {
                if (mGridCopy[r,c] == 1)
                {
                    uiBitMap = uiTurnBitOn(uiBitMap, c);
                }
            }
            uiColMaps[rwPtr] = uiBitMap;
        }
        //
        // now have a list of target rows and col maps for each row
        //
        uiRowHitMask = 0;
        uiColHitMask = 0;
        bSwordFish = false;

        for (i = 1; i <= rwCnt - 2; i++)
        {
            for (j = i + 1; j <= rwCnt - 1; j++)
            {
                for (k = j + 1; k <= rwCnt; k++)
                {
                    uiMapA = uiColMaps[i];
                    uiMapB = uiColMaps[j];
                    uiMapC = uiColMaps[k];

                    uiBitMap = uiMapA | uiMapB | uiMapC;
                    uiBitsHit = (uint)iBitCount(uiBitMap);
                    //found 3 rows's
                    if (uiBitsHit == 3)
                    {
                        uiColHitMask = uiMapA | uiMapB | uiMapC;
                        uiRowHitMask = uiTurnBitOn(uiRowHitMask, RowList[i]);
                        uiRowHitMask = uiTurnBitOn(uiRowHitMask, RowList[j]);
                        uiRowHitMask = uiTurnBitOn(uiRowHitMask, RowList[k]);

                        bSwordFish = true;
                        goto Found_one;
                    }
                }
            }
        }
    Found_one:
        //

        if (bSwordFish)
        {
            //   now remove p from all cells in the defined rows,
            // NOT intersected by the specified columns
            //
            for (c = 0; c <= 8; c++)
            {
                //if the bit is turned on for a row
                if (uiIsBitOn(uiColHitMask, c))
                {
                    for (r = 0; r <= 8; r++)
                    {
                        //then update grid for every column with a 1
                        if (!uiIsBitOn(uiRowHitMask, r))
                        {
                            UpdateGrid(r, c, 0);
                        }
                    }
                }
            }
        }

    }  //end swordfishrow
    private void CalcTotals()
    {
        int r = 0;
        int c = 0;

        mColTotals.Initialize();
        mColTotals.Initialize();

        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                mRowTotals[r] = mRowTotals[r] + mGridCopy[r, c];
            }
        }

        for (c = 0; c <= 8; c++)
        {
            for (r = 0; r <= 8; r++)
            {
                mColTotals[c] = mColTotals[c] + mGridCopy[r,c];
            }
        }

    }
    private void UpdateGrid(int r, int c, int v)
    {
        if (mGridCopy[r,c] != v)
        {
            mGridCopy[r,c] = v;
        }
        //'
    }
    private void GetXWCols(int r, ref int c1, ref int c2)
    {
        int c = 0;
        //we know there are two 1's in the specified row
        //look from left to right for col 1
        for (c = 0; c <= 8; c++)
        {
            if (mGridCopy[r,c] == 1)
            {
                c1 = c;
                break; 
            }
        }
        //look from right to left for col 2
        for (c = 8; c >= 0; c += -1)
        {
            if (mGridCopy[r,c] == 1)
            {
                c2 = c;
                break; 
            }
        }
    }

    private void GetXWRows(int c, ref int r1, ref int r2)
    {
        int r = 0;
        //we know there are two 1's in the specified column
        //look from top to bottom for row 1
        for (r = 0; r <= 8; r++)
        {
            if (mGridCopy[r,c] == 1)
            {
                r1 = r;
                break; 
            }
        }
        //look from bottom to top for row 2
        for (r = 8; r >= 0; r += -1)
        {
            if (mGridCopy[r,c] == 1)
            {
                r2 = r;
                break; 
            }
        }
    }

    // =======================================================
    public void PrintXwingSets()
    {
        int iSet = 0;
        myDebug(mXWSPtr + " Xwing sets found: r1, r2,  c1, c2");
        for (iSet = 1; iSet <= mXWSPtr; iSet++)
        {
            myDebug(mXWingSets[iSet, 1] + " " + mXWingSets[iSet, 2] + " " + mXWingSets[iSet, 3] + " " + mXWingSets[iSet, 4] + " ");
        }
    }
    public void PrintNineByNine()
    {
        int r = 0;
        int c = 0;
        string sPrint = null;

        myDebug("NineByNine Matrix ");
        for (r = 0; r <= 8; r++)
        {
            sPrint = "r" + r + " ";
            for (c = 0; c <= 8; c++)
            {
                sPrint = sPrint + mGridCopy[r,c] + " ";
            }
            sPrint = sPrint + " " + mRowTotals[r];
            myDebug(sPrint);
        }
        sPrint = "   ";
        for (c = 0; c <= 8; c++)
        {
            sPrint = sPrint + mColTotals[c] + " ";
        }
        myDebug("");
        myDebug(sPrint);

    }
    private void myDebug(string sMess)
    {
        //Redirect all Output Window text to the Immediate Window" checked under the menu Tools > Options > Debugging > General.
        //System.Diagnostics.Debug.WriteLine(sMess);
    }

    private uint uiTurnBitOn(uint uiValue,int iBit)
    {
        //Convention used 0123456789
        //                0001000000  turn on bit(row or col) 3
        uint[] uiBitPos = {256, 128, 64, 32, 16, 8, 4, 2, 1};
        uint uResult;
        uResult = (uiValue | uiBitPos[iBit]);
        return uResult;
    }
    private bool  uiIsBitOn(uint uiValue, int iBit)
    {
        //Convention used 0123456789
        //                1000000000  return true if this bit (row or col) 0 is on 
        uint[] uiBitPos = {256, 128, 64, 32, 16, 8, 4, 2, 1};
        uint uResult;
        uResult = uiValue & uiBitPos[iBit];
        if (uResult == 0) return false; else return true;
    }
    private int iBitCount(uint uiBitMap)
    {
        //count the number of bits turned on the the supplied map
        //eg  uiBitMap  100000111   has 4 bits set
        uint[] uiBitPos = { 256, 128, 64, 32, 16, 8, 4, 2, 1 }; 
        int iCount =0 ;
        int i;
        uint uResult;
        for (i=0;i<=8;i++)
            {
            uResult = uiBitMap & uiBitPos[i];    
            if(uResult != 0) iCount=iCount+1;
            }
        return iCount;
    }

}  // end of class


