<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">
  void _OnPageIndexChanged(object sender, EventArgs e)
  {
      TObjectDataSource.SelectParameters["Filter"].DefaultValue = 
          txtFilter.Text.ToString();
      TObjectDataSource.DataBind();
  }
</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Emulators tool [tor-tool]: turtle.</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Label ID="totalLbl" runat="server" Text="Total Count: " /><br />
        <asp:DropDownList ID="ddlFilter" runat="server" Width="200px"
            onselectedindexchanged="drp1_SelectedIndexChanged" AutoPostBack="True">
            <asp:ListItem Text="All" Value="ALL"></asp:ListItem>
            <asp:ListItem Text="Function name" Value="FUNC"></asp:ListItem>
            <asp:ListItem Text="Testcase type" Value="TYPE"></asp:ListItem>
            <asp:ListItem Text="Tag/keyword" Value="TAG"></asp:ListItem>
        </asp:DropDownList>
        <asp:TextBox ID="txtFilter" runat="server" Columns="50" MaxLength="50"></asp:TextBox>
        <asp:Button ID ="btnFiltering" runat ="server" OnClick ="btnFiltering_Click" Text ="Search" Width ="103px" />
        <br />
        <br />
    <div>    
        <asp:GridView ID="GridView1" runat="server" 
            DataSourceID="TObjectDataSource"
            OnPageIndexChanged="_OnPageIndexChanged"
            DataKeyNames="Filter"
            AutoGenerateColumns='false'
            AllowPaging='true'
            PagerSettings-Mode="NumericFirstLast" 
            PagerSettings-PageButtonCount="10"
            EmptyDataText ="There are no data here yet!"
            HeaderStyle-BackColor="PapayaWhip"
            AlternatingRowStyle-BackColor="LightCyan" PageSize="25"
            OnDataBound="GridVIew_OnDataBound"
            >
            <Columns>
                <asp:BoundField HeaderText='' DataField='ROWNO' 
                    HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />
                <asp:HyperLinkField
                    DataNavigateUrlFields="TID"
                    DataNavigateUrlFormatString="Testcase.aspx?TID={0}"
                    DataTextField="TNAME"
                    HeaderText="TestCase"
                    HeaderStyle-Width="30%" />
                <asp:BoundField HeaderText='Type' DataField='TTYPE' 
                    HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Left" />
                <asp:BoundField HeaderText='Size' DataField='TSIZE' 
                    HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Left" />
                <asp:BoundField HeaderText='Location' DataField='TLOC' 
                    ItemStyle-HorizontalAlign="Left" />
            </Columns>
            <RowStyle ForeColor ="#000066" />
            <SelectedRowStyle BackColor ="#669999" Font-Bold ="True" ForeColor ="White" />
            <HeaderStyle BackColor ="#006699" Font-Bold ="True" ForeColor ="White" />
            <PagerStyle HorizontalAlign="Right" />
        </asp:GridView>
        
        <asp:ObjectDataSource 
            ID="TObjectDataSource" 
            runat="server" 
            TypeName="Samples.AspNet.ObjectDataSource.TestcaseProfileData" 
            SortParameterName="SortColumns"
            EnablePaging="true"
            SelectCountMethod="SelectCount"
            StartRowIndexParameterName="StartRecord"
            MaximumRowsParameterName="MaxRecords"
            SelectMethod="QueryTestcases"
            >
        <SelectParameters>
          <asp:Parameter Name="Filter" Type="string" />  
        </SelectParameters>
        </asp:ObjectDataSource>
    </div>
    </form>
</body>
</html>
