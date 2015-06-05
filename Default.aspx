<%@ Page Title="SudokuAnalyser" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent"></asp:Content>
 
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent" onload="myOnLoad"> 
<div id="idMenuDiv" class ="clCommands">
    <!-- Assign  class of clPreedit if the grid is to be edited before posting to the server  -->
<ul id="css3MainMenu" class="clTopMenu">
	<li class="clTopMenu"><a href="#" style="height:34px;line-height:34px;"><span><img src="Images/buStartNew.png" alt=""/>Start New</span></a>
	<ul>
		<li id="liMenuCreate" onclick="OnClickMenu(this.id)"><a href="#"><img src="Images/buPen.png" alt=""/>Create my own</a></li>
		<li id="liMenuDaily" onclick="OnClickMenu(this.id)"><a href="#"><img src="Images/buCheck.png" alt=""/>Puzzle of the Day</a></li>
		<li id="liMenuEasy" onclick="OnClickMenu(this.id)"><a href="#"><img src="Images/buCheck.png" alt=""/>Easy</a></li>
		<li id="liMenuMed" onclick="OnClickMenu(this.id)"><a href="#"><img src="Images/buCheck.png" alt=""/>Medium</a></li>
		<li id="liMenuDiff" onclick="OnClickMenu(this.id)"><a href="#"><img src="Images/buCheck.png" alt=""/>Difficult</a></li>
		<li id="liMenuVDiff" onclick="OnClickMenu(this.id)"><a href="#"><img src="Images/buCheck.png" alt=""/>Very Difficult</a></li>
		<li id="liMenuDiab" onclick="OnClickMenu(this.id)"><a href="#"><img src="Images/buCheck.png" alt=""/>Diabolical</a></li>
		<li id="liMenuColor" onclick="OnClickMenu(this.id)"><a href="#"><img src="Images/buCheck.png" alt=""/>Coloring</a></li>
		<li id="liMenuSword" onclick="OnClickMenu(this.id)"><a href="#"><img src="Images/buCheck.png" alt=""/>Swordfish</a></li>
		<li id="liMenuXW" onclick="OnClickMenu(this.id)"><a href="#"><img src="Images/buCheck.png" alt=""/>X-Wing</a></li>
		<li id="liMenuGliMenuCheckR" onclick="OnClickMenu(this.id)"><a href="#"><img src="Images/buCheck.png" alt=""/>Gordonian Rectangle</a></li>
	</ul></li>
	<li id="liMenuCheck" onclick="OnClickMenu(this.id)" class="clPreEdit"><a href="#" style="height:34px;line-height:34px;"><img src="Images/buCheckIt.png" alt=""/>Check It!</a></li>
	<li id="liMenuHints" onclick="OnClickMenu(this.id)" class="clPreEdit"><a href="#" style="height:34px;line-height:34px;"><img src="Images/buHints.png" alt=""/>Show Hints</a></li>
	<li class="clTopMenu"><a href="#" style="height:34px;line-height:34px;"><span><img src="Images/buGo.png" alt=""/>Solve</span></a>
	<ul>
		<li id="liMenuIter01" onclick="OnClickMenu(this.id)" class="clPreEdit"><a href="#"><img src="Images/buRotate.png" alt=""/>1 Iteration</a></li>
		<li id="liMenuIter04" onclick="OnClickMenu(this.id)" class="clPreEdit"><a href="#"><img src="Images/buFF1.png" alt=""/>4 Iterations</a></li>
		<li id="liMenuIter08" onclick="OnClickMenu(this.id)" class="clPreEdit"><a href="#"><img src="Images/buFF1.png" alt=""/>8 Iterations</a></li>
		<li id="liMenuIter16" onclick="OnClickMenu(this.id)" class="clPreEdit"><a href="#"><img src="Images/buFF1.png" alt=""/>16 Iterations</a></li>
		<li id="liMenuIterAll" onclick="OnClickMenu(this.id)" class="clPreEdit"><a href="#"><img src="Images/buFFToEnd.png" alt=""/>Solve Entire Puzzle</a></li>
	</ul></li>
    <li id="liMenuOption" onclick="OnClickOption(this.id)" class="clTopLast"><a href="#" style="height:34px;line-height:34px;"><img src="Images/buOptions.png" alt=""/>Options   </a></li>
	<li id="liMenuPrint" onclick="OnClickPrint(this.id)" class="clTopLast"><a href="#" style="height:34px;line-height:34px;"><img src="Images/buPrint.png" alt=""/>Print   </a></li>
	<li id="liMenuAbout" onclick="OnClickMenu(this.id)" class="clPreEdit"><a href="#" style="height:34px;line-height:34px;"><img src="Images/buAbout.png" alt=""/>Help   </a></li>
    </ul>
 </div> 
<%-- END OF MENU COMMANDS --%> 
 <table id="tblOuterWrapper" border ="1">
     <tr>
      <td>   
    <asp:Table ID="tblsoln" class="clWrapper clPrintGrid" runat="server" ClientIDMode="Static" gridlines="None">
    <asp:TableRow> <%-- wrapper/top row of boxes --%>
      <asp:TableCell> 
<asp:Table id="tblTopLeft" runat="server" gridlines="Both">
<asp:TableRow id="trSK0a" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr0c0"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr0c1"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr0c2"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" >
</asp:TextBox>
</asp:TableCell> <%--wcol 0--%>
</asp:TableRow>
<asp:TableRow id="trSK1a" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr1c0"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server"> 
<asp:TextBox ID="tbASPr1c1"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr1c2"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow> 
<asp:TableRow id="trSK2a" runat="server"> 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr2c0"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr2c1"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr2c2"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow>
       </asp:Table>  <%-- end of tblTopLeft--%>        
       </asp:TableCell> <%-- wrapper column --%>
       <asp:TableCell>
<asp:Table ID="tblTopMiddle" runat="server" gridlines="Both">
<asp:TableRow id="trSK0b" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr0c3"  runat="server" readonly="true" maxlength="14" TextMode="MultiLine" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr0c4"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr0c5"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" >
</asp:TextBox>
</asp:TableCell>
</asp:TableRow>
<asp:TableRow  id="trSK1b" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr1c3"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server"> 
<asp:TextBox ID="tbASPr1c4"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr1c5"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow> 
<asp:TableRow id="trSK2b" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr2c3"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr2c4"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr2c5"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow>
</asp:Table> <%-- end of tblTopMiddle--%> 
       </asp:TableCell> <%-- wrapper col 1--%>
       <asp:TableCell>  <%-- wrapper col 2--%>
<asp:Table ID="tblTopRight" runat="server" gridlines="Both">
<asp:TableRow id="trSK0c" runat="server"> 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr0c6"  runat="server" readonly="true" maxlength="14" TextMode="MultiLine" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr0c7"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr0c8"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" >
</asp:TextBox>
</asp:TableCell>
</asp:TableRow>
<asp:TableRow id="trSK1c" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr1c6"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server"> 
<asp:TextBox id="tbASPr1c7"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr1c8"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow> 
<asp:TableRow id="trSK2c" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr2c6"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr2c7"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr2c8"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow>
</asp:Table> <%-- end of tblTopRight--%> 
       </asp:TableCell> <%-- wrapper col 2--%>
      </asp:TableRow> <%-- wrapper/top row of boxes --%>
    <asp:TableRow> <%-- wrapper/middle row of boxes --%>   
      <asp:TableCell> 
<asp:Table ID="tblMiddleLeft" runat="server" gridlines="Both">
<asp:TableRow ID="trSK3a" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr3c0"  runat="server" readonly="true" maxlength="14" TextMode="MultiLine" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr3c1"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr3c2"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" >
</asp:TextBox>
</asp:TableCell> <%--wcol 0--%>
</asp:TableRow>
<asp:TableRow id="trSK4a" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr4c0"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server"> 
<asp:TextBox ID="tbASPr4c1"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr4c2"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow> 
<asp:TableRow id="trSK5a" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr5c0"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr5c1"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr5c2"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
 </asp:TableCell>
</asp:TableRow>
</asp:Table>  <%-- end of tblMiddleLeft--%>        
       </asp:TableCell> <%-- wrapper column --%>
       <asp:TableCell>
<asp:Table ID="tblCenter" runat="server" gridlines="Both">
<asp:TableRow id="trSK3b" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr3c3"  runat="server" readonly="true" maxlength="14" TextMode="MultiLine" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr3c4"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr3c5"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" >
</asp:TextBox>
</asp:TableCell>
</asp:TableRow>
<asp:TableRow id="trSK4b" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr4c3"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server"> 
<asp:TextBox ID="tbASPr4c4"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr4c5"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow> 
<asp:TableRow id="trSK5b" runat="server"> 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr5c3"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr5c4"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr5c5"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow>
</asp:Table> <%-- end of tblCenter--%> 
       </asp:TableCell> <%-- wrapper col 1--%>
       <asp:TableCell>  <%-- wrapper col 2--%>
<asp:Table id="tblMiddleRight" runat="server" gridlines="Both">
<asp:TableRow id="trSK3c" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr3c6"  runat="server" readonly="true" maxlength="14" TextMode="MultiLine" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr3c7"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr3c8"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" >
</asp:TextBox>
</asp:TableCell>
</asp:TableRow>
<asp:TableRow id="trSK4c" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr4c6"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server"> 
<asp:TextBox id="tbASPr4c7"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr4c8"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow> 
<asp:TableRow id="trSK5c" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr5c6"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr5c7"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox id="tbASPr5c8"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow>
</asp:Table> <%-- end of tblMiddleRight--%> 
    </asp:TableCell> <%-- wrapper col 2--%>
    </asp:TableRow> <%-- wrapper/middle row of boxes --%>
    <asp:TableRow> <%-- wrapper/bottom row of boxes --%>
      <asp:TableCell> <%--Bottom Left box--%>
<asp:Table id="tblBottomLeft" runat="server" gridlines="Both">
<asp:TableRow id="trSK6a" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr6c0"  runat="server" readonly="true" maxlength="14" TextMode="MultiLine" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr6c1"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell  Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr6c2"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" >
</asp:TextBox>
</asp:TableCell> <%--wcol 0--%>
</asp:TableRow>
<asp:TableRow id="trSK7a" runat="server" > 
<asp:TableCell  Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr7c0"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell  Class="clTableCell" runat="server"> 
<asp:TextBox ID="tbASPr7c1"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell  Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr7c2"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow> 
<asp:TableRow id="trSK8a" runat="server"> 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr8c0"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell  Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr8c1"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell  Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr8c2"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow>
</asp:Table>  <%-- end of tblTopLeft--%>  
      </asp:TableCell> <%--end of Bottom Left box--%>
      <asp:TableCell> <%--Bottom middle box--%>
<asp:Table id="tbBottomMiddle" runat="server" gridlines="Both">
<asp:TableRow id="trSK6b" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr6c3"  runat="server" readonly="true" maxlength="14" TextMode="MultiLine" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell  Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr6c4"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr6c5"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" >
</asp:TextBox>
</asp:TableCell> <%--wcol 0--%>
</asp:TableRow>
<asp:TableRow id="trSK7b" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr7c3"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server"> 
<asp:TextBox ID="tbASPr7c4"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr7c5"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow> 
<asp:TableRow id="trSK8b" runat="server"> 
<asp:TableCell  Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr8c3"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell  Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr8c4"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell  Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr8c5"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow>
</asp:Table>  <%-- end of tblBottom Middle--%>  
      </asp:TableCell> <%--end of Bottom middle box--%>
      <asp:TableCell> <%--Bottom right box--%>
<asp:Table id="tblBottomRight" runat="server" gridlines="Both">
<asp:TableRow id="trSK6c" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr6c6"  runat="server" readonly="true" maxlength="14" TextMode="MultiLine" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr6c7"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr6c8"  runat="server" maxlength="14" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" >
</asp:TextBox>
</asp:TableCell> <%--wcol 0--%>
</asp:TableRow>
<asp:TableRow id="trSK7c" runat="server" > 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr7c6"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server"> 
<asp:TextBox ID="tbASPr7c7"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr7c8"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow> 
<asp:TableRow id="trSK8c" runat="server"> 
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr8c6"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr8c7"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
<asp:TableCell Class="clTableCell" runat="server" >
<asp:TextBox ID="tbASPr8c8"  runat="server" maxlength="14" Rows="3" TextMode="MultiLine" readonly="true" ClientIDMode="Static" onclick="OnClickSKCell(this.id);" ></asp:TextBox>
</asp:TableCell>
</asp:TableRow>
</asp:Table>  <%-- end of tblBottom Right--%>
      </asp:TableCell> <%--wrapper --%>
    </asp:TableRow> <%-- wrapper/bottom row of boxes --%>
   </asp:Table> <%-- end of tblSoln--%> 
    
       </td>  
       <td class="clASPHints">        
          <asp:Panel ID="pnlCard"  runat="server" HorizontalAlign="Left" Height="450px" ScrollBars="Auto" onclick="OnClickPanel(this.id);">
              <asp:Label ID="lblASPmess" ClientIDMode="Static" Visible="true" runat="server" Font-Size="Large"></asp:Label>
             <%-- this code updated on the client side, not via the server--%>
              <div class="clOptPage" id="divOptPage">
                  <p id="idOptBanner"><b>Personalize your Sudoku experience.</b></p>
                  <input id="idOptRegions" type="checkbox" checked="checked" />Highlight related rows, columns and boxes.<br /><br />
                  <input id="idOptScalers" type="checkbox" checked="checked" />Highlight related solved cells.<br /><br />
                  <input id="idOptSave" class="clOptButtons" type="button" onclick="onClickSave('idOptSave')" value="Save Options" /> <input name="idOptCancel" class="clOptButtons" type="button" onclick="onClickCancel('idOptCancel')" value="Cancel" />
              <p id="idOptCookie"><b>Note: This feature uses cookies.</b></p>  
              </div>
                  </asp:Panel>
       </td>
     </tr>
        </table> 
<!-- div hierarchy/nesting is critical to ensure two rows of buttons align on the left-->
<div id="idButtonsDiv" style="clear:right"> <%--1. javascript shows/hides this id 2. put pencil marks on next line--%>
    <div>
        <img  id="buJS1" src="Images/buNewS1.png" class="clButtons" onclick="OnClickEnter(this.id);" alt=""/>
        <img  id="buJS2" src="Images/buNewS2.png" class="clButtons" onclick="OnClickEnter(this.id);" alt=""/>
        <img  id="buJS3" src="Images/buNewS3.png" class="clButtons" onclick="OnClickEnter(this.id);" alt=""/>
        <img  id="buJS4" src="Images/buNewS4.png" class="clButtons" onclick="OnClickEnter(this.id);" alt=""/>
        <img  id="buJS5" src="Images/buNewS5.png" class="clButtons" onclick="OnClickEnter(this.id);" alt=""/>
        <img  id="buJS6" src="Images/buNewS6.png" class="clButtons" onclick="OnClickEnter(this.id);" alt=""/>
        <img  id="buJS7" src="Images/buNewS7.png" class="clButtons" onclick="OnClickEnter(this.id);" alt=""/>
        <img  id="buJS8" src="Images/buNewS8.png" class="clButtons" onclick="OnClickEnter(this.id);" alt=""/>
        <img  id="buJS9" src="Images/buNewS9.png" class="clButtons" onclick="OnClickEnter(this.id);" alt=""/>
        <img  src="Images/buEnter.png" class="clButtons" alt=""/>
    </div>
    <div>
        <img id="buJP1" src="Images/buNewP1.png" class="clButtons" onclick="OnClickPencil(this.id);" alt=""/>
        <img id="buJP2" src="Images/buNewP2.png" class="clButtons" onclick="OnClickPencil(this.id);" alt=""/>
        <img id="buJP3" src="Images/buNewP3.png" class="clButtons" onclick="OnClickPencil(this.id);" alt=""/>
        <img id="buJP4" src="Images/buNewP4.png" class="clButtons" onclick="OnClickPencil(this.id);" alt=""/>
        <img id="buJP5" src="Images/buNewP5.png" class="clButtons" onclick="OnClickPencil(this.id);" alt=""/>
        <img id="buJP6" src="Images/buNewP6.png" class="clButtons" onclick="OnClickPencil(this.id);" alt=""/>
        <img id="buJP7" src="Images/buNewP7.png" class="clButtons" onclick="OnClickPencil(this.id);" alt=""/>
        <img id="buJP8" src="Images/buNewP8.png" class="clButtons" onclick="OnClickPencil(this.id);" alt=""/>
        <img id="buJP9" src="Images/buNewP9.png" class="clButtons" onclick="OnClickPencil(this.id);" alt=""/>
        <img  src="Images/buPencil.png" class="clButtons" alt=""/>
    </div> 
 </div>  <!-- end if idButtonsDiv -->
      <asp:HiddenField id="aspHFChangeLog" runat="server" ClientIDMode="Static" Value=""/>  
      <asp:HiddenField id="aspHFSolution" runat="server" ClientIDMode="Static" Value=""/>  
      <asp:HiddenField id="aspHFCaseid" runat="server" ClientIDMode="Static" Value=""/>
<audio id="au1">
       <source src="Images/click.mp3" type="audio/mp3" />
       <source src="Images/click.ogg" type="audio/ogg" />
       <embed  src="Images/click.mp3" />
</audio>
<%--    <a href="ServerErrorDetected.html">Test error page</a>--%>

</asp:Content>
