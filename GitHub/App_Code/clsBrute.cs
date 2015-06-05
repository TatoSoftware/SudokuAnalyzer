using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
//
// logic was used to solve the puzzle but no soluton was found. See if there are more
// than one solution, try by brute force, not using any logic. If two are found
// in under 2m tries then great, if not give up, since there could only be 1 solution.
//
internal class clsBrute
{
    //two throttles on web front end to ensure we don't burn up the processor
    //sizes can be bumpted up for testing, or down for the web 
    //arbitrary but works for all test cases was 2000
    const int MAXTABLESIZE = 4000;
    //100K max needed for all samples is 63,000
    const int MAXGRIDSTOGENERATE = 100 * 1000;
    const string NEWLINE = "\n";

    //for every cell identify each p value and other info
    int[,] mTableA = new int[81, 12];
    public enum Ta
    {
        r = 0,
        //row of sk array
        c = 1,
        //col of sk array
        cnt = 2,
        //number of candidates
        p0 = 2
        //candidate list, un-reduced, columns 3...11
    }

    int mTaCnt;
    //pointer to strings of solutions for each level
    int[,] mTBPtr = new int[9, 2];
    public enum Tb
    {
        FromPtr = 0,
        //start and end pointers to strings for this level
        ToPtr = 1
    }

    int mTbCnt;
    //table of possible solutions for a row eg 213456789
    string[] msTableB = new string[MAXTABLESIZE + 1];
    //optimized table b  - same data as TableB but values unpacked
    int[,] mTableC = new int[MAXTABLESIZE + 1, 9];

    int mTcCnt;
    //used to store the zillion trial solutions
    int[,] mTrialSK = new int[9, 9];
    //used to store all (2) solutions found
    int[, ,] mSolvedSK = new int[2, 9, 9];
    //count candidates removed via brute force
    int mNoOfChanges;

    //tells results of analysis
    string mReasonCode;

    bool mbIsSolveable;

    public string ReasonCode
    {
        //property could be used by C# front end
        get { return mReasonCode; }
    }
    public bool IsSolveable
    {
        get { return mbIsSolveable; }
    }
    public int NoOfChanges
    {
        get { return mNoOfChanges; }
    }

    public bool Analyze(ref int[, ,] skArg)
    {
        bool functionReturnValue = false;
        //
        //Analyze the grid to see if we have for the grid
        //        Return  IsSolveable  false
        //   1) At least two solutions  - proven to be not a valid puzzle  
        //        Return  IsSolveable true
        //   1) A single row generates to many permutations to consider. See  MAXTABLESIZE
        //   2) Analysis stopped after analysing more than (2m) grids. See MAXGRIDSTOGENERATE
        //    
        int r = 0;
        int c = 0;
        int p = 0;

        int iX = 0;

        mTableA.Initialize();
        //maybe works first time
        mTBPtr.Initialize();
        msTableB.Initialize();
        mReasonCode = "";
        mTaCnt = 0;
        mTbCnt = 0;
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                for (p = 1; p <= 9; p++)
                {
                    if (skArg[r, c, p] == 1)
                    {
                        AddItemTableA(r, c, p);
                    }
                }
                mTaCnt = mTaCnt + 1;
                //new row in tablea defined for every list of p candidates
            }
        }
        mTaCnt = mTaCnt - 1;
        for (r = 0; r <= 8; r++)
        {
            BuildTableB(r);
        }
        if (mReasonCode == "MAXTABLESIZE")
        {
            mbIsSolveable = false;
            functionReturnValue = false;
            return functionReturnValue;
        }
        //
        //for speed purposed convert from strings to individual integer values
        //
        for (iX = 0; iX <= mTbCnt - 1; iX++)
        {
            for (p = 0; p <= 8; p++)
            {
                mTableC[iX, p] = Convert.ToInt32(msTableB[iX].Substring(p, 1));
            }
        }
        mTcCnt = mTbCnt;
        //
        // now we can generate a ton of grids and see if any of them have a solution
        //
        GenerateSolutions(ref skArg);
        functionReturnValue = mbIsSolveable;
        return functionReturnValue;
    }
    private void AddItemTableA(int r, int c, int p)
    {
        //
        //create table one row for every r and c in the entire grid showing the possible p values
        //
        int iNewPCnt = 0;
        mTableA[mTaCnt, (int)Ta.r] = r;
        mTableA[mTaCnt, (int)Ta.c] = c;
        mTableA[mTaCnt, (int)Ta.cnt] = mTableA[mTaCnt, (int)Ta.cnt] + 1;
        //no of candidates
        iNewPCnt = mTableA[mTaCnt, (int)Ta.cnt];
        //use count to index array
        mTableA[mTaCnt, (int)Ta.p0 + iNewPCnt] = p;
    }

    private void DumpTableA()
    {
        //
        int i = 0;
        int j = 0;
        string sDisplay = null;
        myDebug("dump table a");
        for (i = 0; i <= mTaCnt; i++)
        {
            sDisplay = "Index " + i + " r " + mTableA[i, (int)Ta.r] + " c " + mTableA[i, (int)Ta.c] + " cnt " + mTableA[i, (int)Ta.cnt] + " Pvalues ";

            for (j = 3; j <= 11; j++)
            {
                sDisplay = sDisplay + mTableA[i, j];
            }
            myDebug(sDisplay);
        }

    }
    private void BuildTableB(int vLevel)
	{
		//pointer to strings of solutions for each level
		//produce table of possible solutions for a row eg 213456789
		//testing, dump out all possible valid p values for an entire row
		int p0 = 0;
		int p1 = 0;
		int p2 = 0;
		int p3 = 0;
		int p4 = 0;
		int p5 = 0;
		int p6 = 0;
		int p7 = 0;
		int p8 = 0;
		int iLvlPtr = 0;
		string s0 = null;
		string s1 = null;
		string s2 = null;
		string s3 = null;
		string s4 = null;
		string s5 = null;
		string s6 = null;
		string s7 = null;
		string s8 = null;
		string sNewP = null;
		//update starting point in output array
		mTBPtr[vLevel, (int)Tb.FromPtr] = mTbCnt;
		//
		iLvlPtr = vLevel * 9;
        s0 = Convert.ToString(1); 
		//index to TableA 9 rows in tablea define each row i the grid
		for (p0 = 1; p0 <= mTableA[iLvlPtr, (int)Ta.cnt]; p0++) {
			s0 = Convert.ToString(mTableA[iLvlPtr, p0 + 2]);
			//s0 is first p value in set of 9

			for (p1 = 1; p1 <= mTableA[iLvlPtr + 1, (int)Ta.cnt]; p1++) {
				sNewP = Convert.ToString(mTableA[iLvlPtr + 1, p1 + 2]);
				//don't iterate/introduce duplicates
				if (!s0.Contains(sNewP)) {
					s1 = s0 + sNewP;

					for (p2 = 1; p2 <= mTableA[iLvlPtr + 2, (int)Ta.cnt]; p2++) {
						sNewP = Convert.ToString(mTableA[iLvlPtr + 2, p2 + 2]);
						//don't iterate/introduce duplicates
						if (!s1.Contains(sNewP)) {
							s2 = s1 + sNewP;

							for (p3 = 1; p3 <= mTableA[iLvlPtr + 3, (int)Ta.cnt]; p3++) {
								sNewP = Convert.ToString(mTableA[iLvlPtr + 3, p3 + 2]);
								//don't iterate/introduce duplicates
								if (!s2.Contains(sNewP)) {
									s3 = s2 + sNewP;

									for (p4 = 1; p4 <= mTableA[iLvlPtr + 4, (int)Ta.cnt]; p4++) {
										sNewP = Convert.ToString(mTableA[iLvlPtr + 4, p4 + 2]);
										//don't iterate/introduce duplicates
										if (!s3.Contains(sNewP)) {
											s4 = s3 + sNewP;

											for (p5 = 1; p5 <= mTableA[iLvlPtr + 5, (int)Ta.cnt]; p5++) {
												sNewP = Convert.ToString(mTableA[iLvlPtr + 5, p5 + 2]);
												//don't iterate/introduce duplicates
												if (!s4.Contains(sNewP)) {
													s5 = s4 + sNewP;

													for (p6 = 1; p6 <= mTableA[iLvlPtr + 6, (int)Ta.cnt]; p6++) {
														sNewP = Convert.ToString(mTableA[iLvlPtr + 6, p6 + 2]);
														//don't iterate/introduce duplicates
														if (!s5.Contains(sNewP)) {
															s6 = s5 + sNewP;

															for (p7 = 1; p7 <= mTableA[iLvlPtr + 7, (int)Ta.cnt]; p7++) {
																sNewP = Convert.ToString(mTableA[iLvlPtr + 7, p7 + 2]);
																//don't iterate/introduce duplicates
																if (!s6.Contains(sNewP)) {
																	s7 = s6 + sNewP;

																	for (p8 = 1; p8 <= mTableA[iLvlPtr + 8, (int)Ta.cnt]; p8++) {
																		sNewP = Convert.ToString(mTableA[iLvlPtr + 8, p8 + 2]);

																		if (!s7.Contains(sNewP)) {
																			s8 = s7 + sNewP;
																		} else {
																			s8 = s7;
																		}
																		//need a full set of 9 pvalues and double check no duplicates
																		if (bIsValidCandidate(s8)) {
																			if (mTbCnt < MAXTABLESIZE) {
																				msTableB[mTbCnt] = s8;
																				//item
																				mTBPtr[vLevel, (int)Tb.ToPtr] = mTbCnt;
																				//pointer to intem
																				mTbCnt = mTbCnt + 1;
																			} else {
																				mReasonCode = "MAXTABLESIZE";
																				goto GET_OUT_NOW;
																			}
																		}

																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

    GET_OUT_NOW: return;
       
    }
    private bool bIsValidCandidate(string vPValues)
    {
        bool functionReturnValue = false;
        //
        //in the supplied string return true iff the string contains in any order the values
        // 123456789, with no duplicates
        //
        int i = 0;
        int iUniques = 0;
        functionReturnValue = false;

        //optimize, 
        if (vPValues.Length != 9)
        {
            return functionReturnValue;
        }
        string sP;
        for (i = 1; i <= 9; i++)
        {
            sP = Convert.ToString(i);
            if (vPValues.Contains(sP))
            {
                iUniques = iUniques + 1;
            }
        }
        //no duplicates each one counted once
        if (iUniques == 9)
        {
            functionReturnValue = true;
        }
        return functionReturnValue;

    }
    private void GenerateSolutions(ref int[, ,] skArg)
    {
        //
        // only need to see if there is more than 1 solution
        // input table has all possible rows that may be part of a solution
        // generate all permutations of all possible rows
        //
        int ss0 = 0;
        int ss1 = 0;
        int ss2 = 0;
        int ss3 = 0;
        int ss4 = 0;
        int ss5 = 0;
        int ss6 = 0;
        int ss7 = 0;
        int ss8 = 0;
        //solution sets 4 possible row 1's is 4 ss's
        int iSolnCnt = 0;
        int iSolvedCnt = 0;
        iSolnCnt = 0;
        iSolvedCnt = 0;
        for (ss0 = mTBPtr[0, (int)Tb.FromPtr]; ss0 <= mTBPtr[0, (int)Tb.ToPtr]; ss0++)
        {
            mTrialSK[0, 0] = mTableC[ss0, 0];
            mTrialSK[0, 1] = mTableC[ss0, 1];
            mTrialSK[0, 2] = mTableC[ss0, 2];
            mTrialSK[0, 3] = mTableC[ss0, 3];
            mTrialSK[0, 4] = mTableC[ss0, 4];
            mTrialSK[0, 5] = mTableC[ss0, 5];
            mTrialSK[0, 6] = mTableC[ss0, 6];
            mTrialSK[0, 7] = mTableC[ss0, 7];
            mTrialSK[0, 8] = mTableC[ss0, 8];
            for (ss1 = mTBPtr[1, (int)Tb.FromPtr]; ss1 <= mTBPtr[1, (int)Tb.ToPtr]; ss1++)
            {
                mTrialSK[1, 0] = mTableC[ss1, 0];
                mTrialSK[1, 1] = mTableC[ss1, 1];
                mTrialSK[1, 2] = mTableC[ss1, 2];
                mTrialSK[1, 3] = mTableC[ss1, 3];
                mTrialSK[1, 4] = mTableC[ss1, 4];
                mTrialSK[1, 5] = mTableC[ss1, 5];
                mTrialSK[1, 6] = mTableC[ss1, 6];
                mTrialSK[1, 7] = mTableC[ss1, 7];
                mTrialSK[1, 8] = mTableC[ss1, 8];

                if (bNoRowDups(0,1) & bNoBoxDups(0, 1))
                {
                    for (ss2 = mTBPtr[2, (int)Tb.FromPtr]; ss2 <= mTBPtr[2, (int)Tb.ToPtr]; ss2++)
                    {
                        mTrialSK[2, 0] = mTableC[ss2, 0];
                        mTrialSK[2, 1] = mTableC[ss2, 1];
                        mTrialSK[2, 2] = mTableC[ss2, 2];
                        mTrialSK[2, 3] = mTableC[ss2, 3];
                        mTrialSK[2, 4] = mTableC[ss2, 4];
                        mTrialSK[2, 5] = mTableC[ss2, 5];
                        mTrialSK[2, 6] = mTableC[ss2, 6];
                        mTrialSK[2, 7] = mTableC[ss2, 7];
                        mTrialSK[2, 8] = mTableC[ss2, 8];


                        if (bNoRowDups(1, 2) & bNoBoxDups(0, 2))
                        {
                            for (ss3 = mTBPtr[3, (int)Tb.FromPtr]; ss3 <= mTBPtr[3, (int)Tb.ToPtr]; ss3++)
                            {
                                mTrialSK[3, 0] = mTableC[ss3, 0];
                                mTrialSK[3, 1] = mTableC[ss3, 1];
                                mTrialSK[3, 2] = mTableC[ss3, 2];
                                mTrialSK[3, 3] = mTableC[ss3, 3];
                                mTrialSK[3, 4] = mTableC[ss3, 4];
                                mTrialSK[3, 5] = mTableC[ss3, 5];
                                mTrialSK[3, 6] = mTableC[ss3, 6];
                                mTrialSK[3, 7] = mTableC[ss3, 7];
                                mTrialSK[3, 8] = mTableC[ss3, 8];


                                if (bNoRowDups(3, 2))
                                {
                                    for (ss4 = mTBPtr[4, (int)Tb.FromPtr]; ss4 <= mTBPtr[4, (int)Tb.ToPtr]; ss4++)
                                    {
                                        mTrialSK[4, 0] = mTableC[ss4, 0];
                                        mTrialSK[4, 1] = mTableC[ss4, 1];
                                        mTrialSK[4, 2] = mTableC[ss4, 2];
                                        mTrialSK[4, 3] = mTableC[ss4, 3];
                                        mTrialSK[4, 4] = mTableC[ss4, 4];
                                        mTrialSK[4, 5] = mTableC[ss4, 5];
                                        mTrialSK[4, 6] = mTableC[ss4, 6];
                                        mTrialSK[4, 7] = mTableC[ss4, 7];
                                        mTrialSK[4, 8] = mTableC[ss4, 8];


                                        if (bNoRowDups(4, 3) & bNoBoxDups(3, 4))
                                        {
                                            for (ss5 = mTBPtr[5, (int)Tb.FromPtr]; ss5 <= mTBPtr[5, (int)Tb.ToPtr]; ss5++)
                                            {
                                                mTrialSK[5, 0] = mTableC[ss5, 0];
                                                mTrialSK[5, 1] = mTableC[ss5, 1];
                                                mTrialSK[5, 2] = mTableC[ss5, 2];
                                                mTrialSK[5, 3] = mTableC[ss5, 3];
                                                mTrialSK[5, 4] = mTableC[ss5, 4];
                                                mTrialSK[5, 5] = mTableC[ss5, 5];
                                                mTrialSK[5, 6] = mTableC[ss5, 6];
                                                mTrialSK[5, 7] = mTableC[ss5, 7];
                                                mTrialSK[5, 8] = mTableC[ss5, 8];


                                                if (bNoRowDups(5, 4) & bNoBoxDups(3, 5))
                                                {
                                                    for (ss6 = mTBPtr[6, (int)Tb.FromPtr]; ss6 <= mTBPtr[6, (int)Tb.ToPtr]; ss6++)
                                                    {
                                                        mTrialSK[6, 0] = mTableC[ss6, 0];
                                                        mTrialSK[6, 1] = mTableC[ss6, 1];
                                                        mTrialSK[6, 2] = mTableC[ss6, 2];
                                                        mTrialSK[6, 3] = mTableC[ss6, 3];
                                                        mTrialSK[6, 4] = mTableC[ss6, 4];
                                                        mTrialSK[6, 5] = mTableC[ss6, 5];
                                                        mTrialSK[6, 6] = mTableC[ss6, 6];
                                                        mTrialSK[6, 7] = mTableC[ss6, 7];
                                                        mTrialSK[6, 8] = mTableC[ss6, 8];



                                                        if (bNoRowDups(6, 5))
                                                        {
                                                            for (ss7 = mTBPtr[7, (int)Tb.FromPtr]; ss7 <= mTBPtr[7, (int)Tb.ToPtr]; ss7++)
                                                            {
                                                                mTrialSK[7, 0] = mTableC[ss7, 0];
                                                                mTrialSK[7, 1] = mTableC[ss7, 1];
                                                                mTrialSK[7, 2] = mTableC[ss7, 2];
                                                                mTrialSK[7, 3] = mTableC[ss7, 3];
                                                                mTrialSK[7, 4] = mTableC[ss7, 4];
                                                                mTrialSK[7, 5] = mTableC[ss7, 5];
                                                                mTrialSK[7, 6] = mTableC[ss7, 6];
                                                                mTrialSK[7, 7] = mTableC[ss7, 7];
                                                                mTrialSK[7, 8] = mTableC[ss7, 8];


                                                                if (bNoRowDups(7, 6) & bNoBoxDups(6, 7))
                                                                {
                                                                    for (ss8 = mTBPtr[8, (int)Tb.FromPtr]; ss8 <= mTBPtr[8, (int)Tb.ToPtr]; ss8++)
                                                                    {
                                                                        mTrialSK[8, 0] = mTableC[ss8, 0];
                                                                        mTrialSK[8, 1] = mTableC[ss8, 1];
                                                                        mTrialSK[8, 2] = mTableC[ss8, 2];
                                                                        mTrialSK[8, 3] = mTableC[ss8, 3];
                                                                        mTrialSK[8, 4] = mTableC[ss8, 4];
                                                                        mTrialSK[8, 5] = mTableC[ss8, 5];
                                                                        mTrialSK[8, 6] = mTableC[ss8, 6];
                                                                        mTrialSK[8, 7] = mTableC[ss8, 7];
                                                                        mTrialSK[8, 8] = mTableC[ss8, 8];


                                                                        if (bNoRowDups(8, 7) & bNoBoxDups(6, 8))
                                                                        {
                                                                            //
                                                                            // we have generated a new, unique SK grid, see
                                                                            // if it is a valid sudoku solution 
                                                                            //
                                                                            iSolnCnt = iSolnCnt + 1;
                                                                            if (bIsSolved())
                                                                            {
                                                                                iSolvedCnt = iSolvedCnt + 1;
                                                                                //save a valid solution, it MAY BE returned
                                                                                SaveSolution(iSolvedCnt);
                                                                                //pass solution number 1 or 2
                                                                            }
                                                                            //if more than 1 stop, not a valid puzzle
                                                                            if (iSolvedCnt > 1)
                                                                            {
                                                                                goto ALL_DONE;
                                                                            }
                                                                            //at this point give up
                                                                            if (iSolnCnt > MAXGRIDSTOGENERATE)
                                                                            {
                                                                                goto ALL_DONE;
                                                                            }
                                                                        }
                                                                        //ss8 ignore dups
                                                                    }
                                                                }
                                                                //ss7 ignore dups
                                                            }
                                                        }
                                                        //ss6 ignore dups
                                                    }
                                                }
                                                //ss5 ignore dups
                                            }
                                        }
                                        //ss4 ignore dups
                                    }
                                }
                                //ss3 ignore dups
                            }
                        }
                        //ss2 ignore dups
                    }
                }
                //ss1 ignore dups
            }
        }
    ALL_DONE:
    if(iSolvedCnt > 1) 
    {
        mbIsSolveable = false;
        mReasonCode = "2SOLUTIONS";
        ReturnSolution(ref skArg, 2);
        //return 2 solutions found
    }
    if (iSolvedCnt == 0)
    {
        mbIsSolveable = false;
        mReasonCode = "0SOLUTIONS";
    }
    if (iSolvedCnt == 1)
    {
        //
        //tell a small lie, if only 1 solution found within MAXGRIDSTOGENERATE (100Kgrids) say that
        //there is only 1 solution , even though in theory there could be more.
        //on the web it could take 4-5 seconds to determine this so don't waste the effort
        //
        mbIsSolveable = true;
        //one solution only
        mReasonCode = "1SOLUTION";
        ReturnSolution(ref skArg, 1);
        //return 1 soln found
    }
        //Call myDebug("Totals solutions " & iSolnCnt & " Solved Solutions " & iSolvedCnt)
    }
    private bool bNoRowDups(int viA, int viB)
    {
        bool functionReturnValue = false;
        //check the two 'rows' for any values found in both rows, if so no need to produce more permutations
        functionReturnValue = true;
        int c = 0;
        for (c = 0; c <= 8; c++)
        {
            if (mTrialSK[viA, c] == mTrialSK[viB, c])
            {
                functionReturnValue = false;
                return functionReturnValue;
            }
        }
        return functionReturnValue;
    }
    private bool bNoBoxDups(int viFromR, int viToR)
    {
        bool functionReturnValue = false;
        int r = 0;
        int c = 0;
        int pValue = 0;
        int[] iPCntBox1 = new int[10];
        //use 3 arrays therefore no need to initialize to zero!!
        int[] iPCntBox2 = new int[10];
        int[] iPCntBox3 = new int[10];
        functionReturnValue = true;
        for (r = viFromR; r <= viToR; r++)
        {
            //left box
            for (c = 0; c <= 2; c++)
            {
                pValue = mTrialSK[r, c];
                iPCntBox1[pValue] = iPCntBox1[pValue] + 1;
                //first conflict get out now
                if (iPCntBox1[pValue] > 1)
                {
                    functionReturnValue = false;
                    return functionReturnValue;
                }
            }
        }
        //
        for (r = viFromR; r <= viToR; r++)
        {
            //middle box box
            for (c = 3; c <= 5; c++)
            {
                pValue = mTrialSK[r, c];
                iPCntBox2[pValue] = iPCntBox2[pValue] + 1;
                //first conflict get out now
                if (iPCntBox2[pValue] > 1)
                {
                    functionReturnValue = false;
                    return functionReturnValue;
                }
            }
        }
        for (r = viFromR; r <= viToR; r++)
        {
            //right box
            for (c = 6; c <= 8; c++)
            {
                pValue = mTrialSK[r, c];
                iPCntBox3[pValue] = iPCntBox3[pValue] + 1;
                //first conflict get out now
                if (iPCntBox3[pValue] > 1)
                {
                    functionReturnValue = false;
                    return functionReturnValue;
                }
            }
        }
        return functionReturnValue;


    }

    private bool bIsSolved()
    {
        bool functionReturnValue = false;
        //
        // Need to check all solution sets
        // each and every one  must contain the numbers 1..9
        //  
        //  problem, there are many, many puzzles that add up to 405 but are not solved completely
        //  thus the 405 test says there are multiple solutions very quickly but the more rigorous
        //  test takes forever
        //
        int r = 0;
        int c = 0;
        int p = 0;

        int[] pCnts = new int[10];
        //make sure we have 9 occurances of every number 1..9
        //define the  top left corner of each box in the mini grid
        int[,] iBoxMap = {
			{
				0,
				0
			},
			{
				0,
				3
			},
			{
				0,
				6
			},
			{
				3,
				0
			},
			{
				3,
				3
			},
			{
				3,
				6
			},
			{
				6,
				0
			},
			{
				6,
				3
			},
			{
				6,
				6
			}
		};
        int iBox = 0;

        functionReturnValue = true;
        //
        //most like in this scenario a column will have a duplicate
        //make sure 1 2 3 4 5 6 7 8 9 in every column
        //
        for (c = 0; c <= 8; c++)
        {
            for (p = 1; p <= 9; p++)
            {
                pCnts[p] = 0;
            }
            for (r = 0; r <= 8; r++)
            {
                pCnts[mTrialSK[r, c]] = pCnts[mTrialSK[r, c]] + 1;
            }
            for (p = 1; p <= 9; p++)
            {
                if (pCnts[p] != 1)
                {
                    functionReturnValue = false;
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
                    pCnts[mTrialSK[r, c]] = pCnts[mTrialSK[r, c]] + 1;
                }
            }
            for (p = 1; p <= 9; p++)
            {
                if (pCnts[p] != 1)
                {
                    functionReturnValue = false;
                    return functionReturnValue;
                }
            }
        }
        //
        //very unlikely as all rows in this object(brute force) are already tested to be valid
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
                pCnts[mTrialSK[r, c]] = pCnts[mTrialSK[r, c]] + 1;
            }
            for (p = 1; p <= 9; p++)
            {
                if (pCnts[p] != 1)
                {
                    functionReturnValue = false;
                    return functionReturnValue;
                }
            }
        }
        return functionReturnValue;

    }
    private void DumpTrialSK(int vID)
    {
        int r = 0;
        int c = 0;
        string sOut = null;
        sOut = "Dump of Grid Number " + vID + NEWLINE;
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                sOut = sOut + mTrialSK[r, c] + " ";
            }
            sOut = sOut + NEWLINE;
        }
        myDebug(sOut);
    }
    private void SaveSolution(int vSolnNo)
    {
        //saves two solutions for returning to the user
        int r = 0;
        int c = 0;
        int iSolnId = 0;
        //
        if (vSolnNo == 1)
        {
            iSolnId = 0;
            //1st soln
        }
        else
        {
            iSolnId = 1;
            //any other soln's
        }
        //
        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                mSolvedSK[iSolnId, r, c] = mTrialSK[r, c];
            }
        }

    }
    private void ReturnSolution(ref int[, ,] skArg, int vRtnCnt)
    {
        //trial solution(s) found 
        //return either 1 or 2 solutions
        //
        int r = 0;
        int c = 0;
        int p = 0;
        mNoOfChanges = 0;

        for (r = 0; r <= 8; r++)
        {
            for (c = 0; c <= 8; c++)
            {
                for (p = 1; p <= 9; p++)
                {
                    //always return 1 solution
                    //1st soln
                    if (mSolvedSK[0, r, c] == p)
                    {
                        if (skArg[r, c, p] != 1)
                        {
                            mNoOfChanges = mNoOfChanges + 1;
                            skArg[r, c, p] = 1;
                        }
                    }
                    else
                    {
                        if (skArg[r, c, p] != 0)
                        {
                            mNoOfChanges = mNoOfChanges + 1;
                            skArg[r, c, p] = 0;
                        }
                    }
                    //
                    //if requested add 2nd soluntions p values to 1st solution
                    //
                    if (vRtnCnt == 2)
                    {
                        //2nd soln
                        if (mSolvedSK[1, r, c] == p)
                        {
                            skArg[r, c, p] = 1;
                        }
                    }
                }
            }
        }
        //xml field is only 4 digits
        if (mNoOfChanges > 9999)
        {
            mNoOfChanges = 9999;
        }
    }
    private void myDebug(string sMess)
    {
        //Redirect all Output Window text to the Immediate Window" checked under the menu Tools > Options > Debugging > General.
       // System.Diagnostics.Debug.WriteLine(sMess);
    }
}
