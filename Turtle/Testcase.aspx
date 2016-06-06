<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Testcase.aspx.cs" Inherits="Testcase" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>TUR-tool: Testcase Profiler/Cataloger (aka turtle).</title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:Label ID="tcName" runat="server" Text="Testcase: " /><br />
    <asp:Label ID="tcLoc" runat="server" Text="Location: " /><br />
    <asp:Label ID="tcCS" runat="server" Text="Checksums: " /><br />
    <asp:Label ID="tcTime" runat="server" Text="Took: " />
    <br />
    <br />
    <div>
        <asp:GridView ID="FuncView" runat="server" 
            DataSourceID="TestcaseDS"
            DataKeyNames="TID"
            AutoGenerateColumns='false'
            AllowPaging='true'
            EmptyDataText ="There are no data here yet!"
            HeaderStyle-BackColor="PapayaWhip"
            AlternatingRowStyle-BackColor="LightCyan" PageSize="25">
            <Columns>
                <asp:BoundField HeaderText='' DataField='ROWNO' 
                    HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />

                <asp:HyperLinkField
                    DataNavigateUrlFields="FUNC_NAME"
                    DataNavigateUrlFormatString="Default.aspx?Filter={0}"
                    DataTextField="FUNC_NAME"
                    HeaderText="Function"
                    HeaderStyle-Width="30%"
                    ItemStyle-HorizontalAlign="Left" />

                <asp:BoundField HeaderText='File' DataField='SOURCE_FILE' 
                    HeaderStyle-Width="60%" ItemStyle-HorizontalAlign="Left" />
                <asp:BoundField HeaderText='Line No' DataField='LINE_NO' 
                    HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />
            </Columns>
            <RowStyle ForeColor ="#000066" />
            <SelectedRowStyle BackColor ="#669999" Font-Bold ="True" ForeColor ="White" />
            <PagerStyle BackColor ="White" ForeColor ="#000066" HorizontalAlign ="Left" />
            <HeaderStyle BackColor ="#006699" Font-Bold ="True" ForeColor ="White" />
        </asp:GridView>

        <asp:ObjectDataSource 
            ID="TestcaseDS" 
            runat="server" 
            TypeName="Samples.AspNet.ObjectDataSource.TestcaseData" 
            SortParameterName="SortColumns"
            EnablePaging="true"
            SelectCountMethod="SelectCount"
            StartRowIndexParameterName="StartRecord"
            MaximumRowsParameterName="MaxRecords" 
            SelectMethod="QueryFunctions" >
        <SelectParameters>
          <asp:Parameter Name="TID" Type="string" />  
        </SelectParameters>
        </asp:ObjectDataSource>
    </div>
    </form>
</body>
</html>
