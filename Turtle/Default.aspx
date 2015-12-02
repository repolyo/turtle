<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<script runat="server">
  void EmployeesGridView_OnSelectedIndexChanged(object sender, EventArgs e)
  {
      EmployeesObjectDataSource.SelectParameters["Filter"].DefaultValue = 
          txtFilter.Text.ToString();
      EmployeesObjectDataSource.DataBind();
  }
</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        Keyword/Tag:
        <asp:TextBox ID="txtFilter" runat="server" Columns="50" MaxLength="50"></asp:TextBox>
        <asp:Button ID ="btnFiltering" runat ="server" OnClick ="btnFiltering_Click" Text ="Search" Width ="103px" />
        <br />
        <br />
    <div>    
        <asp:GridView ID="GridView1" runat="server" 
            DataSourceID="EmployeesObjectDataSource"
            OnPageIndexChanged="EmployeesGridView_OnSelectedIndexChanged"
            DataKeyNames="Filter"
            AutoGenerateColumns='false'
            AllowPaging='true'
            EmptyDataText ="There are no data here yet!"
            HeaderStyle-BackColor="PapayaWhip"
            AlternatingRowStyle-BackColor="LightCyan" PageSize="25">
            <Columns>
                <asp:BoundField HeaderText='' DataField='ROWNO' 
                    HeaderStyle-Width="30" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField HeaderText='TestCase' DataField='TNAME' 
                    HeaderStyle-Width="150" ItemStyle-HorizontalAlign="Left" />
                <asp:BoundField HeaderText='Location' DataField='TLOC' 
                    HeaderStyle-Width="500" ItemStyle-HorizontalAlign="Left" />
            </Columns>
            <RowStyle ForeColor ="#000066" />
            <SelectedRowStyle BackColor ="#669999" Font-Bold ="True" ForeColor ="White" />
            <PagerStyle BackColor ="White" ForeColor ="#000066" HorizontalAlign ="Left" />
            <HeaderStyle BackColor ="#006699" Font-Bold ="True" ForeColor ="White" />
        </asp:GridView>

        <asp:ObjectDataSource 
            ID="EmployeesObjectDataSource" 
            runat="server" 
            TypeName="Samples.AspNet.ObjectDataSource.NorthwindData" 
            SortParameterName="SortColumns"
            EnablePaging="true"
            SelectCountMethod="SelectCount"
            StartRowIndexParameterName="StartRecord"
            MaximumRowsParameterName="MaxRecords" 
            SelectMethod="GetAllEmployees" >
        <SelectParameters>
          <asp:Parameter Name="Filter" Type="string" />  
        </SelectParameters>
        </asp:ObjectDataSource>
    </div>
    </form>
</body>
</html>
