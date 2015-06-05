using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
internal class clsList
{
    //
    // this class maintains a list of cells, specified by their r and c in the grid
    // and a list of P(ossibilities) that will be applied to those cells.
    // methods are provided to act on the list of p values against the cells in the list
    //
    private int mvarPcnt;
    //local copy
    private int mvarCellCnt;
    //row and column
    private int[,] mCells = new int[82, 3];
    //up to 9 possible p values can be defined
    private int[] mPList = new int[10];


    private int mChangeCount;
    public int ChangeCount
    {
        get { return mChangeCount; }
    }


    public void RemovePFromCellList(ref int[, ,] skArg)
    {
        //
        // for every cell in the list, remove the value p values defined
        //
        int i = 0;
        int j = 0;
        int r = 0;
        int c = 0;

        for (i = 1; i <= mvarPcnt; i++)
        {
            for (j = 1; j <= mvarCellCnt; j++)
            {
                r = mCells[j, 1];
                c = mCells[j, 2];
                RemoveCandidate(r, c, mPList[i], ref skArg);
            }
        }

    }

    public void KeepOnlyPs(ref int[, ,] skArg)
    {
        //
        // for the list of cells, keep only the p values that are in the list
        //   p's are 4 , 5
        //   cell is   4, 5, 7, 8
        //    remove 7 and 8
        //
        //
        int j = 0;
        int r = 0;
        int c = 0;
        int ip = 0;

        for (ip = 1; ip <= 9; ip++)
        {
            if (!Pfound(ip))        //csnote rewritten
            {
                for (j = 1; j <= mvarCellCnt; j++)
                {
                    r = mCells[j, 1];
                    c = mCells[j, 2];
                    RemoveCandidate(r, c, ip, ref skArg);
                }
            }
        }

    }


    public void AddP(int p)
    {
        int i = 0;
        for (i = 1; i <= mvarPcnt; i++)
        {
            if (mPList[i] == p)
            {
                return;
            }
        }

        mvarPcnt = mvarPcnt + 1;
        //p value to be acted upon
        mPList[mvarPcnt] = p;
    }

    public void AddCell(int rArg, int cArg)
    {
        //
        // add specified cell to the list but do not add duplicates
        //
        int i = 0;
        for (i = 1; i <= mvarCellCnt; i++)
        {
            if (mCells[i, 1] == rArg & mCells[i, 2] == cArg)
            {
                return;
            }
        }
        //
        mvarCellCnt = mvarCellCnt + 1;
        mCells[mvarCellCnt, 1] = rArg;
        //cells to be updated
        mCells[mvarCellCnt, 2] = cArg;
    }
    private void RemoveCandidate(int rArg, int cArg, int pArg, ref int[, ,] skArg)
    {
        //
        // removes a single candidate from the specified cell
        // Warning - same code exists in other classes
        //
        //dont touch solved cells
        if (CandidateCount(rArg, cArg, ref skArg) > 1)
        {
            //currently a candidate
            if (skArg[rArg, cArg, pArg] == 1)
            {
                skArg[rArg, cArg, pArg] = 0;
                //no longer a candidate
                mChangeCount = mChangeCount + 1;
            }
        }
    }
    private int CandidateCount(int rArg, int cArg,ref int[, ,] skArg)
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
            functionReturnValue = functionReturnValue + skArg[rArg, cArg, p];
            //p is 1 one it is a possibility
        }
        return functionReturnValue;

    }
    public void PrintLists()
    {
        int i = 0;
        myDebug("Class List dump");
        for (i = 1; i <= mvarPcnt; i++)
        {
            myDebug("P value is " + mPList[i]);
        }
        for (i = 1; i <= mvarCellCnt; i++)
        {
            myDebug("Cell defined  is " + mCells[i, 1] + "," + mCells[i, 2]);
        }

    }
    public int CellCnt
    {
        get { return mvarCellCnt; }
    }

    public int pCnt
    {
        get { return mvarPcnt; }
    }
    private bool Pfound (int pArg)      //csNote rewritten
    {

            bool functionReturnValue = false;
            //
            // return true if p is found in the list
            //
            int i = 0;
            functionReturnValue = false;
            for (i = 1; i <= mvarPcnt; i++)
            {
                if (mPList[i] == pArg)
                {
                    functionReturnValue = true;
                    break;                          
                }
            }
            return functionReturnValue;
    }

    private bool bIsInTheList(int rArg, int cArg)
    {
        bool functionReturnValue = false;
        //
        //determines if the passed row and col is in the supplied row and col
        //
        int i = 0;
        functionReturnValue = false;
        for (i = 1; i <= mvarCellCnt; i++)
        {
            if (mCells[i, 1] == rArg & mCells[i, 2] == cArg)
            {
                functionReturnValue = true;
                break; 
            }
        }
        return functionReturnValue;
    }
    private void Class_Initialize_Renamed()
    {
        mvarPcnt = 0;
        mvarCellCnt = 0;
        mChangeCount = 0;
        mCells.Initialize();
        mPList.Initialize();
    }
    private void myDebug(string sMess)
    {
        //Redirect all Output Window text to the Immediate Window" checked under the menu Tools > Options > Debugging > General.
        //System.Diagnostics.Debug.WriteLine(sMess);
    }
    public clsList()
        : base()
    {
        Class_Initialize_Renamed();
    }
}
