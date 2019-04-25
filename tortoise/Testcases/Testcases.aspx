<%@ Page Title="Testcases" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Testcases.aspx.cs" Inherits="Testcases_Testcases" %>
<%@ MasterType virtualpath="~/MasterPage.master" %>

<script runat="server">
    void _OnPageIndexChanged(object sender, EventArgs e)
    {
        TObjectDataSource.SelectParameters["Filter"].DefaultValue = (string)ViewState["Filter"];
        TObjectDataSource.DataBind();
    }

</script>
<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div>
        <asp:DropDownList ID="ddlFilter" runat="server" Width="200px"
            onselectedindexchanged="drp1_SelectedIndexChanged" AutoPostBack="True">
            <asp:ListItem Text="Function name" Value="FUNC"></asp:ListItem>
            <asp:ListItem Text="Testcase type" Value="TYPE"></asp:ListItem>
            <asp:ListItem Text="Tag/keyword" Value="TAG"></asp:ListItem>
            <asp:ListItem Text="Filename" Value="FILE"></asp:ListItem>
            <asp:ListItem Text="Size" Value="SIZE" Enabled="false"></asp:ListItem>
        </asp:DropDownList>
        <asp:TextBox ID="txtFilter" runat="server" Columns="100" MaxLength="150"></asp:TextBox>
        <asp:Button ID ="btnFiltering" runat ="server" OnClick ="btnFiltering_Click" Text ="Search" Width ="103px" />
        <asp:Button ID="btnExport" runat="server" Text="Export" OnClick = "ExportToExcel" ToolTip="Download into CSV..." />
    </div>
    <div class="testcases">
         <!--EmptyDataTemplate
                No Records Available
                asp:Image ID="Image1" ImageUrl=""
                a href="Default.aspx" Try to Reload a 
            EmptyDataTemplate-->
        <asp:GridView ID="GridView1" runat="server" 
            DataSourceID="TObjectDataSource"
            OnPageIndexChanged="_OnPageIndexChanged"
            DataKeyNames="TNAME"
            AutoGenerateColumns='false'
            AllowPaging='true'
            PagerSettings-Mode="NumericFirstLast" 
            PagerSettings-PageButtonCount="10"
            HeaderStyle-BackColor="PapayaWhip"
            AlternatingRowStyle-BackColor="LightCyan" PageSize="25"
            OnDataBound="GridVIew_OnDataBound"
            EmptyDataText="No testcase found!" Width="100%" EnableSortingAndPagingCallbacks="False" AllowSorting="True">
            <Columns>
                <asp:BoundField HeaderText='' DataField='ROWNO' 
                    HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField
                    DataField="TNAME"
                    HeaderText="TestCase"
                    HeaderStyle-Width="30%" />
                <asp:BoundField HeaderText='Type' DataField='TTYPE' 
                    HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Left" />
                <asp:BoundField HeaderText='Size' DataField='TSIZE' 
                    HeaderStyle-Width="10%" ItemStyle-HorizontalAlign="Left" />
                <asp:TemplateField HeaderText="Skip" HeaderStyle-Width="3%">  
                    <EditItemTemplate>  
                        <asp:CheckBox ID="CheckBox1" runat="server" Checked='<%# Bind("HIDDEN") %>'  oncheckedchanged="Skip_CheckedChanged"/>  
                    </EditItemTemplate>  
                    <ItemTemplate>  
                        <asp:CheckBox ID="CheckBox1" AutoPostBack="true" runat="server"
                            Checked='<%# Eval("HIDDEN").ToString().Equals("1") ? true : false %>'
                            oncheckedchanged="Skip_CheckedChanged"/>  
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField HeaderText='Location' DataField='TLOC' 
                    ItemStyle-HorizontalAlign="Left" />
            </Columns>
            <RowStyle ForeColor ="#000066" />
            <SelectedRowStyle BackColor ="#669999" Font-Bold ="True" ForeColor ="White" />
            <HeaderStyle BackColor ="#009900" Font-Bold ="True" ForeColor ="White" />
            <PagerStyle HorizontalAlign="Right" />
        </asp:GridView>
         <strong>
        <asp:Label ID="Label1" runat="server" Text="Total Count: "></asp:Label>
         </strong>
        <asp:Label ID="Count" runat="server" Text="0"></asp:Label>
        <br />
        <asp:ObjectDataSource 
            ID="TObjectDataSource" 
            runat="server" 
            TypeName="TESTCASE_VIEW" 
            SortParameterName="SortColumns"
            EnablePaging="true"
            SelectCountMethod="SelectCount"
            StartRowIndexParameterName="StartRecord"
            MaximumRowsParameterName="MaxRecords"
            SelectMethod="QueryTestcases"
            OnSelected="TObjectDataSource_OnSelected">
            <SelectParameters>
              <asp:Parameter Name="Filter" Type="string" />  
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>
</asp:Content>

