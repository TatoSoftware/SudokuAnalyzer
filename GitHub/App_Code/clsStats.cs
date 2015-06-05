using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
// Class to keep track of the totals of every technique and is used only used by clsSK
// Note that it is very rare for any Sudoku app to be able to report on what techniques are needed to solved a puzzle.
public class clsStats
{

    private const int mSTATSCNT = 20;
    private int[] miCounts = new int[mSTATSCNT + 1];
    private string[] msCodes = { "NS", "HS" ,"NP", "NT","NQ", "HP","LB", "LR","LC", "HT","HQ", "XW","SW", "CJ","CO", "BF","GR", "GO","GP", "NI"};
    private string[] msDesc = {"Naked Single",
                                "Hidden Single",
                                "Naked Pair",
                                "Naked Triple",
                                "Naked Quad",
                                "Hidden Pair",
                                "Locked Box",
                                "Locked Row",
                                "Locked Col",
                                "Hidden Triple",
                                "Hidden Quad",
                                "X-Wing",
                                "Sword Fish",
                                "Co-Joined Chains",
                                "Coloring",
                                "Brute Force",
                                "Standard Gordonian Rectange",
                                "Onesided Gordonian Rectangle",
                                "Gordonian Polygon Plus",
                                "Nishio"};

	public clsStats()
	{
        // constructor logic
        miCounts.Initialize();
        myDebug("clsStats initialized");
	}
    public void UpdateStats(string sType, int iCnt)
    // eg mystats.UpdateStats("BF",3);   3 p values changed by brute force
    {
        int i;
        for (i = 0; i <= mSTATSCNT - 1; i++)
        {
            if (sType == msCodes[i]) 
                miCounts[i] = miCounts[i] + iCnt;
        }
    } //end UpdateStats
    public void GetXMLStats(ref XmlDocument XML_CS)
    {
        //
        // now add in the statistics to the end of the xml document
        // 20 or so entries like
        //
        // <stats>BF 9999 Brute Force</stats>
        //
        int i = 0;
        string sValue = null;
        XmlElement elem;
        XmlNode root;
        string sCnt = null;

        for (i = 0; i <= mSTATSCNT - 1; i++)
        {
            // example result
            // <stats>BF 9999 Brute Force</stats>
            sCnt = String.Format("{0:0000}", miCounts[i]);  //csNote format zeros
            sValue = msCodes[i] + " " + sCnt + " " + msDesc[i];  
            //Create a new node.
            elem = XML_CS.CreateElement("stats");
            elem.InnerText = sValue;
            //Add the node to the document.
            root = XML_CS.DocumentElement;
            root.AppendChild(elem);
        }

    } //end GetXMLStats
    private void myDebug(string sMess)
    {
        //Redirect all Output Window text to the Immediate Window" checked under the menu Tools > Options > Debugging > General.
        //System.Diagnostics.Debug.WriteLine(sMess);
    }
}