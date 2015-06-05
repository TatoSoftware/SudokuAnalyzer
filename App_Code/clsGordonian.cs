using System;
using System.Collections.Generic;

public class clsGordonian
{
    // Refer to Chapter 8, Gordonian Logic
    // In the book Mensa Guide to Solving Sudoku by Peter Gordon, 2006
    private int mChangeCount;
    int[,] mSSMap = { { 0, 0, 0, 8 }, { 1, 0, 1, 8 }, { 2, 0, 2, 8 }, { 3, 0, 3, 8 }, { 4, 0, 4, 8 }, { 5, 0, 5, 8 }, { 6, 0, 6, 8 }, { 7, 0, 7, 8 }, { 8, 0, 8, 8 }, { 0, 0, 8, 0 }, { 0, 1, 8, 1 }, { 0, 2, 8, 2 }, { 0, 3, 8, 3 }, { 0, 4, 8, 4 }, { 0, 5, 8, 5 }, { 0, 6, 8, 6 }, { 0, 7, 8, 7 }, { 0, 8, 8, 8 }, { 0, 0, 2, 2 }, { 0, 3, 2, 5 }, { 0, 6, 2, 8 }, { 3, 0, 5, 2 }, { 3, 3, 5, 5 }, { 3, 6, 5, 8 }, { 6, 0, 8, 2 }, { 6, 3, 8, 5 }, { 6, 6, 8, 8 } };
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
    //new term - Zone . There are 6 zones, 3 rows of box's and 3 columns of box's
    //
    //ie if each 9*9 box is an X then we need the 5 naked pairs to be in boxes specified
    //XXX    ...     ...     X..     .X.     ..X
    //...    XXX     ...     X..     .X.     ..X
    //...    ...     XXX     X..     .X.     ..X  
    // each group of 3 x's represents a zone
    private int[,] mZoneMap = { { 0, 0, 2, 8 }, { 3, 0, 5, 8 }, { 6, 0, 8, 8 }, { 0, 0, 8, 2 }, { 0, 3, 8, 5 }, { 0, 6, 8, 8 } };
    private int mZoneMapCnt = 6;
    public enum ENz
    {
        rFr = 0,
        cFr = 1,
        rTo = 2,
        cTo = 3
    }
    //index into mssmap array for box solution sets
    const int BOX_SS_Fr = 18;

    const int BOX_SS_To = 26;
    private enum enNP
    {
        p1 = 0,
        p2 = 1,
        Cnt = 2
    }

    private int[,] mNakedPairList = new int[81, 3];

    private int mNakedPairListCnt;
    //local copy
    private string mvarMethod;

    public int ChangeCount
    {
        get { return mChangeCount; }
    }
    public string Method
    {
        get { return mvarMethod; }
    }
    public void Gordonian(ref int[, ,] skArg)
    {
        //need to use one of three quite different techniques
        mChangeCount = 0;
        NewRectangle(ref skArg);
        //first type of Gordonian Rectangle
        if (mChangeCount == 0)
        {
            OneSidedRectangle(ref skArg);
            //one-sided gordonian rectangles
        }
        if (mChangeCount == 0)
        {
            PolygonPlus(ref skArg);
            //gordonian polygon plus
        }
    }

    private void OneSidedRectangle(ref int[, ,] skArg)
    {
        // look for the following pattern
        // 23      23x
        // 23      23x  in two boxes
        // call 23x the OSPattern
        //  if found candidate x must be in either cell above and thus a x can be elimitated from other columns in the grid
        // variable names for targets
        // r1,c1    r1,c2
        // r2,c1    r2,c2
        //
        int r = 0;
        int c = 0;
        int nP1 = 0;
        int nP2 = 0;
        int cScan = 0;
        int r1 = 0;
        int c1 = 0;
        int r2 = 0;
        //coordinates of one sided rectangle 
        int npX = 0;
        //the p value for x in 23x
        bool bHit = false;
        npX = 0;
        bHit = false;
        myDebug("Enter One-Sided Gordonian analysis");
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 7; c++)
            {
                if (bFindNakedPair(skArg, r, c, ref nP1, ref nP2))
                {
                    r1 = r;
                    c1 = c;
                    for (cScan = 0; cScan <= 8; cScan++)
                    {
                        if (bFindTriplet(skArg, r1, cScan, nP1, nP2, ref npX))
                        {
                            bHit = true;
                            goto Exit2Fors;
                        }
                    }
                }

            }
        }
    Exit2Fors:
        if (!bHit)
        {
            return;
        }
        //
        //found one row with 23   in one column and 23x in another
        //r1 = row number
        //c1 = col number of 23
        //cScan = col number of 23x
        //npX = x  eg 8
        //nP1 = 2
        //nP2 = 3
        //
        // Find another row with 23 in the same column, to form a corner to the box
        //
        bHit = false;
        for (r = 0; r <= 8; r++)
        {
            //exclude the 23 we just found
            if (r != r1)
            {
                if (bFindSpecificPair(skArg, r, c1, nP1, nP2))
                {
                    bHit = true;
                    r2 = r;
                    break; 
                }
            }
        }
        if (!bHit)
        {
            return;
        }
        //now have r2, need to find corresponding 23x
        //Call myDebug("second pair(23) found at " & r2 & "," & c1)
        if (!bFindSpecificTiplet(skArg, r2, cScan, nP1, nP2, npX))
        {
            return;
        }
        //
        //we have the 4 corners of a box with the form
        //   23     23x
        //   23     23x     and this works for test case 80
        //
        //Call myDebug("One sided Gordonian detected " & r1 & "," & c1 & "     " & r1 & "," & cScan)
        //Call myDebug("                             " & r2 & "," & c1 & "     " & r2 & "," & cScan)
        //now do the four corners of the box reside in two and only two boxes
        if (bValidGordonianRectangle(r1, c1, r1, cScan, r2, c1, r2, cScan))
        {
            myDebug("One sided gordonian all test passed.");
            //
            //now in our example the x must be in either the first 23x or the second 23x, if the
            //pair of 23x's are in column 6, then removed candidate x from all other cells in that column
            //eg  23  23x
            //    17   4x         <--- remove the x, leaving just the candidate 4
            //    23  23x
            for (r = 0; r <= 8; r++)
            {
                if (r != r1 & r != r2)
                {
                    if (skArg[r, cScan, npX] == 1)
                    {
                        mChangeCount = mChangeCount + 1;
                        skArg[r, cScan, npX] = 0;
                    }
                    //zap the candidate
                }
            }
            mvarMethod = "GROS";
            //gordonian one-sided rectangle
        }
    }
    private bool bValidGordonianRectangle(int rA, int cA, int rB, int cB, int rC, int cC, int rD, int cD)
    {
        bool functionReturnValue = false;
        //
        //all 4 cells must be within 2 exactly 2 solution sets of the box type
        //eg
        //r c  ss    see there are only 2 solution sets
        //1 4  19  
        //1 6  20
        //2 6  20
        //2 4  19
        int[] iSSCnt = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        int iSSSum = 0;
        int i = 0;

        functionReturnValue = false;
        iSSCnt[nLookupBoxSS(rA, cA)] = 1;
        //turn on the bit for the box this r,c falls into
        iSSCnt[nLookupBoxSS(rB, cB)] = 1;
        iSSCnt[nLookupBoxSS(rC, cC)] = 1;
        iSSCnt[nLookupBoxSS(rD, cD)] = 1;

        //check all the box solution sets only
        for (i = BOX_SS_Fr; i <= BOX_SS_To; i++)
        {
            //make sure we have two and only 2
            iSSSum = iSSSum + iSSCnt[i];
        }
        //co-ordinates are found in two and only two boxes
        if (iSSSum == 2)
        {
            functionReturnValue = true;
        }
        return functionReturnValue;

    }

    private void PolygonPlus(ref int[, ,] skArg)
	{
		// see example 22-1
		// to qualify as a Gordonian Polygon Plus  we need 5 naked pairs and a 6th cell with the naked pair plus n other p values
		// 17  xx  xx  xx 1279 xx  xx  xx  xx
		// xx  xx  xx  xx  17  xx  xx  17  xx   
		// 17  xx  xx  xx  xx  xx  xx  17  xx
		// The corners of the rectangle must reside in 3 box's
		//
		int r = 0;
		int c = 0;
		int nP1 = 0;
		int nP2 = 0;
		int rTarget = 0;
		int cTarget = 0;
		int i = 0;
		bool bEnoughNakedPairs = false;

		myDebug("Gordonian polygon Plus analysis entered.");
		mChangeCount = 0;
		mNakedPairListCnt = 0;
		for (r = 0; r <= 8; r++) {
			for (c = 0; c <= 8; c++) {
				if (bFindNakedPair(skArg, r, c, ref nP1, ref nP2)) {
					//record naked pair in a list containig the pairs and r,c's where they are found
					UpdateNPFoundList(nP1, nP2);
				}
			}
		}
		//quick test, if we don't have 5 naked pairs in the whole grid then get out of dodge
		bEnoughNakedPairs = false;
		for (i = 0; i <= mNakedPairListCnt - 1; i++) {
			if (mNakedPairList[i, (int)enNP.Cnt] >= 5) {
				bEnoughNakedPairs = true;
			}
		}
		if (!bEnoughNakedPairs) {
			return;
		}
		//
		nP1 = -1;
		for (i = 0; i <= mNakedPairListCnt - 1; i++) {
			//
			// test 1, do we have the same pair occurring 5 times
			//
			nP1 = mNakedPairList[i, (int)enNP.p1];
			nP2 = mNakedPairList[i, (int)enNP.p2];
			myDebug("Polygon Plus naked pairs " + nP1 + nP2 + " count is " + mNakedPairList[i, (int)enNP.Cnt]);

			//we need exactly 5 naked pairs in the one Zone
			//new term - Zone . There are 6 zones, 3 rows of box's and 3 columns of box's
			//
			//ie if each 9*9 box is an X then we need the 5 naked pairs to be in boxes specified
			//XXX    ...     ...     X..     .X.     ..X
			//...    XXX     ...     X..     .X.     ..X
			//...    ...     XXX     X..     .X.     ..X  
			// each group of 3 x's represents a zone
			//
			//do we have 5 and only 5 pairs in one zone
			if (mNakedPairList[i, (int)enNP.Cnt] >= 5) {
				if (bFindPolyPlusPattern(ref skArg, nP1, nP2, ref rTarget, ref cTarget)) {
					//we have found a pattern that matches
					//p1 and p2 can be removed from the target cell
					myDebug("Polyplus pattern detected see cell " + rTarget + cTarget);
					skArg[rTarget, cTarget, nP1] = 0;
					//remove p1
					mChangeCount = mChangeCount + 1;
					skArg[rTarget, cTarget, nP2] = 0;
					//remove p2
					mChangeCount = mChangeCount + 1;
					mvarMethod = "GRPP";
					return;
					//only do one iteration at a time.
				}
			}
		}
	}
    private bool bFindPolyPlusPattern(ref int[, ,] skarg, int p1, int p2, ref int rTargetRow, ref int rTargetCol)
    {
        bool functionReturnValue = false;
        //Try to find this pattern, in any of the 6 zones
        // 17  xx  xx  xx 1279 xx  xx  xx  xx
        // xx  xx  xx  xx  17  xx  xx  17  xx   
        // 17  xx  xx  xx  xx  xx  xx  17  xx
        // the zone totals array for this zone would look like
        //  1  0  0  0  0  0  0  0  0   1
        //  0  0  0  0  0  1  0  1  0   2
        //  1  0  0  0  0  0  0  1  0   2
        //                              r and c
        //  2  0  0  0  0  1  0  2  0   totals
        int iZone = 0;
        int r = 0;
        int c = 0;
        int[,] iZoneMapped = new int[9, 9];
        int[] iMappedRowTotals = new int[9];
        int[] iMappedColTotals = new int[9];
        bool bPolyPlusRow = false;
        bool bPolyPlusCol = false;
        int rTarget = 0;
        int cTarget = 0;
        functionReturnValue = false;

        for (iZone = 0; iZone <= mZoneMapCnt - 1; iZone++)
        {
            //reset all counters
            for (r = 0; r <= 8; r++)
            {
                iMappedRowTotals[r] = 0;
                for (c = 0; c <= 8; c++)
                {
                    iMappedColTotals[c] = 0;
                    iZoneMapped[r, c] = 0;
                }
            }
            for (r = mZoneMap[iZone, (int)ENz.rFr]; r <= mZoneMap[iZone, (int)ENz.rTo]; r++)
            {
                for (c = mZoneMap[iZone, (int)ENz.cFr]; c <= mZoneMap[iZone, (int)ENz.cTo]; c++)
                {
                    if (bFindSpecificPair(skarg, r, c, p1, p2))
                    {
                        iZoneMapped[r, c] = 1;
                    }
                }
            }
            //add up the totals for each row and column
            for (r = 0; r <= 8; r++)
            {
                for (c = 0; c <= 8; c++)
                {
                    iMappedRowTotals[r] = iMappedRowTotals[r] + iZoneMapped[r, c];
                }
            }
            for (c = 0; c <= 8; c++)
            {
                for (r = 0; r <= 8; r++)
                {
                    iMappedColTotals[c] = iMappedColTotals[c] + iZoneMapped[r, c];
                }
            }
            DetectPolyPlusPattern(ref iMappedRowTotals, ref bPolyPlusRow, ref rTarget);
            DetectPolyPlusPattern(ref iMappedColTotals, ref bPolyPlusCol, ref cTarget);
            if (bPolyPlusRow & bPolyPlusCol)
            {
                // 17  xx  xx  xx 1279 xx  xx  xx  xx
                // xx  xx  xx  xx  17  xx  xx  17  xx   
                // 17  xx  xx  xx  xx  xx  xx  17  xx
                //  see if the the target cell in the above example contains 17 plus n other candidates 
                if (bFindHiddenPair(skarg, rTarget, cTarget, p1, p2))
                {
                    myDebug("Polygon Plus Detected in Zone " + iZone + " pair is " + p1 + p2 + " Target for removal is" + rTarget + cTarget);
                    rTargetRow = rTarget;
                    rTargetCol = cTarget;
                    functionReturnValue = true;
                    return functionReturnValue;
                }
            }
        }
        return functionReturnValue;
    }

    private void DetectPolyPlusPattern(ref int[] rTotals, ref bool bPatternDetected, ref int rTarget)
    {
        //examine the arrray containing the totals for a row or column
        //count the number of 1's found, 2's found and 0's found, and others totals found
        //to be a gordonian polygon plus there must be 2 2's one 1 and  6 zeros ONLY
        //lots of arrays will be all zero especially when we are in the wrong zone
        int i = 0;
        int i2Cnt = 0;
        int i1Cnt = 0;
        int i0Cnt = 0;
        int iOtherCnt = 0;
        bPatternDetected = false;
        for (i = 0; i <= 8; i++)
        {
            switch (rTotals[i])
            {
                case 0:
                    i0Cnt = i0Cnt + 1;
                    break;
                case 1:
                    i1Cnt = i1Cnt + 1;
                    break;
                case 2:
                    i2Cnt = i2Cnt + 1;
                    break;
                default:
                    iOtherCnt = iOtherCnt + 1;
                    break;
            }
        }
        // to be a gordonian polygon plus there must be 2 2's one 1 and  6 zeros
        if (i0Cnt == 6 & i1Cnt == 1 & i2Cnt == 2)
        {
            bPatternDetected = true;
            // the row or column where the count naked pair count is 1 is our target row or column
            //there will be only one total with a value of 1 due to the above test
            for (i = 0; i <= 8; i++)
            {
                if (rTotals[i] == 1)
                {
                    rTarget = i;
                    break; 
                }
            }
        }

    }

    private bool bFindNakedPair(int[, ,] skArg, int r, int c, ref int nP1, ref int nP2)
    {
        bool functionReturnValue = false;
        //does this cell contain a naked pair, if so return the pair as args
        int p = 0;
        int pCnt = 0;
        pCnt = 0;
        functionReturnValue = false;
        //number of unique p values in this cell
        for (p = 1; p <= 9; p++)
        {
            if (skArg[r, c, p] == 1)
            {
                pCnt = pCnt + 1;
            }
        }
        if (pCnt != 2)
        {
            return functionReturnValue;
        }
        // we have a pair,now what are they  
        functionReturnValue = true;
        nP1 = -1;
        for (p = 1; p <= 9; p++)
        {
            if (skArg[r, c, p] == 1)
            {
                if (nP1 == -1)
                {
                    nP1 = p;
                    //first one
                }
                else
                {
                    nP2 = p;
                    //second one
                }
            }
        }
        return functionReturnValue;
    }
    private int nLookupBoxSS(int r, int c)
    {
        int functionReturnValue = 0;
        //lookup the box number associated with any row or column a box solution set is a 9*9 group of cells
        // box numbers are   18    19    20
        //                   21 ...
        //
        int iBox = 0;
        functionReturnValue = -1;
        for (iBox = BOX_SS_Fr; iBox <= BOX_SS_To; iBox++)
        {
            if (r >= mSSMap[iBox, (int)enSSMap.rFr] & c >= mSSMap[iBox, (int)enSSMap.cFr] & r <= mSSMap[iBox, (int)enSSMap.rTo] & c <= mSSMap[iBox, (int)enSSMap.cTo])
            {
                functionReturnValue = iBox;
            }
        }
        return functionReturnValue;

    }
    private bool bFindHiddenPair(int[, ,] skArg, int r, int c, int nP1, int nP2)
    {
        bool functionReturnValue = false;
        //does this cell contain the specified pair of candidates
        //plus at least one other candidate
        //eg if the pair is  27  and the cell contains 1278 then return true
        int p = 0;
        int pCnt = 0;
        pCnt = 0;
        functionReturnValue = false;
        //number of unique p values in this cell
        for (p = 1; p <= 9; p++)
        {
            if (skArg[r, c, p] == 1)
            {
                pCnt = pCnt + 1;
            }
        }
        if (pCnt < 3)
        {
            return functionReturnValue;
        }
        // we have at least 3 candidates, are the ones we want in the list ? 
        if (skArg[r, c, nP1] == 1 & skArg[r, c, nP2] == 1)
        {
            functionReturnValue = true;
        }
        return functionReturnValue;
    }
    private bool bFindTriplet(int[, ,] skArg, int r, int c, int nP1, int nP2, ref int nPx)
    {
        bool functionReturnValue = false;
        //
        //does this cell contain a naked pair, plus one other candidate ?, if so return the other candidate
        //eg if the naked pair is 23  look for 23x and return the x
        //
        int p = 0;
        int pCnt = 0;
        int[] pFound = {
			0,
			0,
			0
		};
        pCnt = 0;
        functionReturnValue = false;
        //Call myDebug("Search for OS pattern from  " & r & "," & c & " " & nP1 & nP2)
        //number of unique p values in this cell
        for (p = 1; p <= 9; p++)
        {
            if (skArg[r, c, p] == 1)
            {
                pCnt = pCnt + 1;
                if (pCnt > 3)
                {
                    return functionReturnValue;
                }
                pFound[pCnt - 1] = p;
                //record the p values found eg 2,3,x
            }
        }
        if (pCnt != 3)
        {
            return functionReturnValue;
        }
        // we have a pair but do we have the right pair
        if ((pFound[0] == nP1 | pFound[1] == nP1 | pFound[2] == nP1) & (pFound[0] == nP2 | pFound[1] == nP2 | pFound[2] == nP2))
        {
            for (p = 0; p <= 2; p++)
            {
                if (pFound[p] != nP1 & pFound[p] != nP2)
                {
                    nPx = pFound[p];
                    functionReturnValue = true;
                    //Call myDebug("OS Pattern found " & r & "," & c & " pattern is " & nP1 & nP2 & nPx)
                }
            }
        }
        return functionReturnValue;
    }
    private bool bFindSpecificPair(int[, ,] skArg, int r, int c, int nP1, int nP2)
    {
        bool functionReturnValue = false;
        //does this cell contain a the specified naked pair, if so return true
        int p = 0;
        int pCnt = 0;
        pCnt = 0;
        functionReturnValue = false;
        //number of unique p values in this cell
        for (p = 1; p <= 9; p++)
        {
            if (skArg[r, c, p] == 1)
            {
                pCnt = pCnt + 1;
            }
        }
        if (pCnt != 2)
        {
            return functionReturnValue;
        }
        // we have a pair, now see if the match the ones we are looking for  
        if (skArg[r, c, nP1] == 1 & skArg[r, c, nP2] == 1)
        {
            functionReturnValue = true;
        }
        return functionReturnValue;
    }
    private bool bFindSpecificTiplet(int[, ,] skArg, int r, int c, int nP1, int nP2, int nP3)
    {
        bool functionReturnValue = false;
        //does this cell contain a the specified naked pair as well as one other value
        //
        int p = 0;
        int pCnt = 0;
        pCnt = 0;
        functionReturnValue = false;
        //number of unique p values in this cell
        for (p = 1; p <= 9; p++)
        {
            if (skArg[r, c, p] == 1)
            {
                pCnt = pCnt + 1;
            }
        }
        if (pCnt != 3)
        {
            return functionReturnValue;
        }
        // we have a pair, now see if the match the ones we are looking for  
        if (skArg[r, c, nP1] == 1 & skArg[r, c, nP2] == 1 & skArg[r, c, nP3] == 1)
        {
            functionReturnValue = true;
        }
        return functionReturnValue;
    }
    private void UpdateNPFoundList(int nP1, int nP2)
    {
        //count all naked pairs and store how many times each one occurs
        int iPair = 0;
        //first pair
        if (mNakedPairListCnt == 0)
        {
            mNakedPairList[0, (int)enNP.p1] = nP1;
            mNakedPairList[0, (int)enNP.p2] = nP2;
            mNakedPairList[0, (int)enNP.Cnt] = 1;
            mNakedPairListCnt = 1;
            return;
        }

        //look for existing pair
        for (iPair = 0; iPair <= mNakedPairListCnt - 1; iPair++)
        {
            if (mNakedPairList[iPair, (int)enNP.p1] == nP1 & mNakedPairList[iPair, (int)enNP.p2] == nP2)
            {
                mNakedPairList[iPair, (int)enNP.Cnt] = mNakedPairList[iPair, (int)enNP.Cnt] + 1;
                return;
            }
        }
        //looks like a new pair add it to the table
        mNakedPairList[mNakedPairListCnt, (int)enNP.p1] = nP1;
        mNakedPairList[mNakedPairListCnt, (int)enNP.p2] = nP2;
        mNakedPairList[mNakedPairListCnt, (int)enNP.Cnt] = 1;
        mNakedPairListCnt = mNakedPairListCnt + 1;
    }
    private void NewRectangle(ref int[, ,] skArg)
	{
		// see example
		//to qualify for a Gordonian rectangle we need 3 naked pairs and one other cell with the naked pairs plus one candidate
		//
		// xx  xx  xx  xx  xx  xx  xx  xx  xx
		// xx  xx  58  xx  xx  xx  58  xx  xx   
		// xx  xx 578  xx  xx  xx  58  xx  xx   ..the 5 and 8 can be removed to give a result of 7
		//
		//
		int r = 0;
		int c = 0;
		int nP1 = 0;
		int nP2 = 0;
		int rTarget = 0;
		int cTarget = 0;
		int i = 0;
		bool bEnoughNakedPairs = false;

		myDebug("New Gordonian rectangle analysis entered.");
		mChangeCount = 0;
		mNakedPairListCnt = 0;
		for (r = 0; r <= 8; r++) {
			for (c = 0; c <= 8; c++) {
				if (bFindNakedPair(skArg, r, c, ref nP1, ref nP2)) {
					//record naked pair in a list containig the pairs and r,c's where they are found
					myDebug("NewRec found naked pair " + r + c + "  " + nP1 + nP2);
					UpdateNPFoundList(nP1, nP2);
				}
			}
		}
		DumpNakedPairsList();
		//quick test, if we don't have 3 naked pairs in the whole grid then get out of dodge
		bEnoughNakedPairs = false;
		for (i = 0; i <= mNakedPairListCnt - 1; i++) {
			if (mNakedPairList[i, (int)enNP.Cnt] >= 3) {
				bEnoughNakedPairs = true;
			}
		}
		if (!bEnoughNakedPairs) {
			return;
		}
		//
		nP1 = -1;
		for (i = 0; i <= mNakedPairListCnt - 1; i++) {
			//
			// test 1, do we have the same pair occurring 3 times
			//
			nP1 = mNakedPairList[i, (int)enNP.p1];
			nP2 = mNakedPairList[i, (int)enNP.p2];
			myDebug("GR Rectangle naked pairs " + nP1 + nP2 + " count is " + mNakedPairList[i, (int)enNP.Cnt]);

			//we need exactly 3 naked pairs in the one Zone
			//new term - Zone . There are 6 zones, 3 rows of box's and 3 columns of box's
			//
			//ie if each 9*9 box is an X then we need the 5 naked pairs to be in boxes specified
			//XXX    ...     ...     X..     .X.     ..X
			//...    XXX     ...     X..     .X.     ..X
			//...    ...     XXX     X..     .X.     ..X  
			// each group of 3 x's represents a zone
			//
			//do we have 3 and only 3 naked pairs in one zone
			if (mNakedPairList[i, (int)enNP.Cnt] >= 3) {
				if (bFindRectanglePattern(ref skArg, nP1, nP2, ref rTarget, ref cTarget)) {
					//we have found a pattern that matches
					//p1 and p2 can be removed from the target cell
					myDebug("GR rectangle pattern detected see cell " + rTarget + cTarget);
					skArg[rTarget, cTarget, nP1] = 0;
					//remove p1
					mChangeCount = mChangeCount + 1;
					skArg[rTarget, cTarget, nP2] = 0;
					//remove p2
					mChangeCount = mChangeCount + 1;
					mvarMethod = "GRNP";
					//GORDONIAN RECTANGLES WITH 3 NAKED PARIS
					return;
					//only do one iteration at a time.
				}
			}
		}
	}
    private bool bFindRectanglePattern(ref int[, ,] skarg, int p1, int p2, ref int rTargetRow, ref int rTargetCol)
    {
        bool functionReturnValue = false;
        //Try to find this pattern, in any of the 6 zones
        // xx  xx  xx  xx  xx  xx  xx  xx  xx
        // xx  xx  58  xx  xx  xx  58  xx  xx   
        // xx  xx 578  xx  xx  xx  58  xx  xx   ..the 5 and 8 can be removed to give a result of 7
        // the zone totals array for this zone would look like
        //  0  0  0  0  0  0  0  0  0   0
        //  0  0  1  0  0  0  1  0  0   2
        //  0  0  0  0  0  0  1  0  0   1
        //                              r and c
        //  0  0  1  0  0  0  2  0  0   totals 
        // the INTERSECTION OF THE TWO 1 TOTALS IS THE CELL TO EXAMINE FOR 578
        //
        int iZone = 0;
        int r = 0;
        int c = 0;
        int[,] iZoneMapped = new int[9, 9];
        int[] iMappedRowTotals = new int[9];
        int[] iMappedColTotals = new int[9];
        bool bRectangleRow = false;
        bool bRectangleCol = false;
        int rTarget = 0;
        int cTarget = 0;
        functionReturnValue = false;

        for (iZone = 0; iZone <= mZoneMapCnt - 1; iZone++)
        {
            //reset all counters
            for (r = 0; r <= 8; r++)
            {
                iMappedRowTotals[r] = 0;
                for (c = 0; c <= 8; c++)
                {
                    iMappedColTotals[c] = 0;
                    iZoneMapped[r, c] = 0;
                }
            }
            for (r = mZoneMap[iZone, (int)ENz.rFr]; r <= mZoneMap[iZone, (int)ENz.rTo]; r++)
            {
                for (c = mZoneMap[iZone, (int)ENz.cFr]; c <= mZoneMap[iZone, (int)ENz.cTo]; c++)
                {
                    if (bFindSpecificPair(skarg, r, c, p1, p2))
                    {
                        iZoneMapped[r, c] = 1;
                    }
                }
            }
            //add up the totals for each row and column
            for (r = 0; r <= 8; r++)
            {
                for (c = 0; c <= 8; c++)
                {
                    iMappedRowTotals[r] = iMappedRowTotals[r] + iZoneMapped[r, c];
                }
            }
            for (c = 0; c <= 8; c++)
            {
                for (r = 0; r <= 8; r++)
                {
                    iMappedColTotals[c] = iMappedColTotals[c] + iZoneMapped[r, c];
                }
            }
            DetectRectanglePattern(ref iMappedRowTotals, ref bRectangleRow, ref rTarget);
            DetectRectanglePattern(ref iMappedColTotals, ref bRectangleCol, ref cTarget);
            if (bRectangleRow & bRectangleCol)
            {
                // sell pattern above 
                if (bFindHiddenPair(skarg, rTarget, cTarget, p1, p2))
                {
                    myDebug("GR Rectangle Detected in Zone " + iZone + " pair is " + p1 + p2 + " Target for removal is" + rTarget + cTarget);
                    rTargetRow = rTarget;
                    rTargetCol = cTarget;
                    functionReturnValue = true;
                    return functionReturnValue;
                }
            }
        }
        return functionReturnValue;
    }

    private void DetectRectanglePattern(ref int[] rTotals, ref bool bPatternDetected, ref int rTarget)
    {
        //examine the arrray containing the totals for a row or column
        //count the number of 1's found, 2's found and 0's found, and others totals found
        //to be a gordonian rectangle there must be one 1, one 2 and 7 zeros ONLY
        //lots of arrays will be all zero especially when we are in the wrong zone
        // note this test is  different from the  POLYPLUS test
        //
        int i = 0;
        int i2Cnt = 0;
        int i1Cnt = 0;
        int i0Cnt = 0;
        int iOtherCnt = 0;
        bPatternDetected = false;
        for (i = 0; i <= 8; i++)
        {
            switch (rTotals[i])
            {
                case 0:
                    i0Cnt = i0Cnt + 1;
                    break;
                case 1:
                    i1Cnt = i1Cnt + 1;
                    break;
                case 2:
                    i2Cnt = i2Cnt + 1;
                    break;
                default:
                    iOtherCnt = iOtherCnt + 1;
                    break;
            }
        }
        // to be a gordonian rectangle there must be one 1, one 2 and 7 zeros
        if (i0Cnt == 7 & i1Cnt == 1 & i2Cnt == 1)
        {
            bPatternDetected = true;
            // the row or column where the count naked pair count is 1 is our target row or column
            //there will be only one total with a value of 1 due to the above test
            for (i = 0; i <= 8; i++)
            {
                if (rTotals[i] == 1)
                {
                    rTarget = i;
                    break; 
                }
            }
        }

    }
    private void DumpNakedPairsList()
    {
        int i = 0;
        for (i = 0; i <= mNakedPairListCnt - 1; i++)
        {
            myDebug("Naked Pairs detected " + mNakedPairList[i, (int)enNP.p1] + mNakedPairList[i, (int)enNP.p2] + " " + mNakedPairList[i, (int)enNP.Cnt]);
        }
    }
    private void myDebug(string sMess)
    {
        //Redirect all Output Window text to the Immediate Window" checked under the menu Tools > Options > Debugging > General.
        //System.Diagnostics.Debug.WriteLine(sMess);
    }
}
