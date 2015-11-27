<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        Enter Filter:
        <asp:TextBox ID="txtFilter" runat="server" Columns="10" MaxLength="10"></asp:TextBox>
        <asp:Button ID ="btnFiltering" runat ="server" OnClick ="btnFiltering_Click" Text ="Filtering" Width ="103px" /><br />
    <div>    
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns='false'
            AllowPaging='true'
            OnPageIndexChanging="grdData_PageIndexChanging"
            EmptyDataText ="There are no data here yet!"
            HeaderStyle-BackColor="PapayaWhip"
            AlternatingRowStyle-BackColor="LightCyan">
            <Columns>
                <asp:TemplateField 
                    HeaderStyle-Width="30"
                    ItemStyle-HorizontalAlign="Center" >
                    <ItemTemplate>
                    <%# Container.DataItemIndex + 1 %>
                    </ItemTemplate>
                </asp:TemplateField>
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
    </div>
    </form>
</body>
</html>
