using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
// Library of generic XML methods callable from anywhere
public class clsXML
{
	public clsXML()
	{
	}
    public string getAttr(XmlDocument myXML, string myAttrName)
    {
        // looks for the attribute (eg myname) specified and returns the attribute value or null if the name is wrong or value not set
        // sXML = "<case id='ir2' myname='001'><reason>abc</reason></case>";
        XmlElement myelement;
        XmlAttribute myAtt;
        string myValue;

        myelement = myXML.DocumentElement;
        myValue = "";
        if (myelement.HasAttribute(myAttrName))
        //        if (myelement.HasAttributes)  works for generic case of n attributes
        {
            // get attribute value in 2 steps
            myAtt = myelement.GetAttributeNode(myAttrName);     //redundant but good for code sample
            myValue = myAtt.Value;
            // works get node value in one step
            myValue = myelement.GetAttribute(myAttrName);
        }
        return myValue;
    }
    public string getNodeValue(XmlDocument myXML, string sNodeName)
    {
        // returns THE FIRST abc from sXML = "<case id='ir2' myname='001'><reason>abc</reason></case>";
        //
        string sNodeValue = null;
        XmlNodeList nodeList = null;
        //returns the value of the first element identified by vNode eg - 'status' or 'reason'
        sNodeValue = "";
        nodeList = myXML.GetElementsByTagName(sNodeName);
        if (nodeList.Count > 0)
        {
            sNodeValue = nodeList[0].InnerText;
        }
        return sNodeValue;
    }
}