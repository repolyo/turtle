<%@ Page Title="Checksums" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Checksums.aspx.cs" Inherits="Testcases_Checksums" %>
<%@ Register
    Assembly="AjaxControlToolkit"
    Namespace="AjaxControlToolkit"
    TagPrefix="ajaxToolkit" %>

<script runat="server">
    void _OnPageIndexChanged(object sender, EventArgs e)
    {
        ChecksumDS.SelectParameters["Filter"].DefaultValue = (string)ViewState["Filter"];
        ChecksumDS.DataBind();
    }
</script>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div>
        <asp:DropDownList ID="PersonaCbx" runat="server" Width="200px" Visible="true"
            AutoPostBack="True" OnSelectedIndexChanged="PersonaCbx_SelectedIndexChanged">
            <asp:ListItem Text="None" Value="0"></asp:ListItem>
            <asp:ListItem Text="pdlsapp" Value="4"></asp:ListItem>
            <asp:ListItem Text="sim-color" Value="1"></asp:ListItem>
            <asp:ListItem Text="sim-mono" Value="2"></asp:ListItem>
            <asp:ListItem Text="sim64-color" Value="3"></asp:ListItem>
        </asp:DropDownList>
        <asp:Button ID="btnShow" runat="server" Text="Update checksum..." style="float:right" ToolTip="Update master checksums..." /><br />

        <asp:ScriptManager ID="asm" runat="server"></asp:ScriptManager>
        <ajaxToolkit:ModalPopupExtender ID="mdlpopup" BackgroundCssClass="modalbackground" runat="server" TargetControlID="btnShow"
                PopupControlID="pnl" OkControlID="btnok" Y="300" ></ajaxToolkit:ModalPopupExtender>

        <asp:Panel ID="pnl" runat="server" BorderColor="ActiveBorder" CssClass="modalpopup" BorderStyle="Solid" BorderWidth="2px">
            
            <ajaxToolkit:AjaxFileUpload ID="AjaxFileUpload1" runat="server"
                Width="400" Height="150"
                OnUploadComplete="AjaxFileUpload1_UploadComplete" AllowedFileTypes="txt,cs,checksum"
                ClearFileListAfterUpload="True" />
            <asp:Button ID="btnok" runat="server" Text="OK" />
        </asp:Panel>
    </div>
    <div class="testcases">
        <asp:GridView ID="ChecksumGrid" runat="server" 
            DataSourceID="ChecksumDS"
            OnPageIndexChanged="_OnPageIndexChanged"
            DataKeyNames="TLOC"
            AutoGenerateColumns='false'
            AllowPaging='true'
            PagerSettings-Mode="NumericFirstLast" 
            PagerSettings-PageButtonCount="10"
            HeaderStyle-BackColor="PapayaWhip"
            AlternatingRowStyle-BackColor="LightCyan" PageSize="25"
            OnDataBound="ChecksumGrid_OnDataBound"
            EmptyDataText="No testcase found!" Width="100%" AllowSorting="True">
            <Columns>
                <asp:BoundField HeaderText='' DataField='ROWNO' 
                    HeaderStyle-Width="50px" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField
                    DataField="TLOC"
                    HeaderText="TestCase"
                    HeaderStyle-Width="30%" />
                <asp:BoundField HeaderText='Checksums' DataField='CHECKSUMS' ItemStyle-HorizontalAlign="Left" />
            </Columns>
            <RowStyle ForeColor ="#000066" />
            <SelectedRowStyle BackColor ="#669999" Font-Bold ="True" ForeColor ="White" />
            <HeaderStyle BackColor ="#009900" Font-Bold ="True" ForeColor ="White" />
            <PagerStyle HorizontalAlign="Right" />
        </asp:GridView>
        <strong>
        <asp:Label ID="Label1" runat="server" Text="Total Count: " Font-Bold="True" Font-Names="Calibri"></asp:Label>
        </strong>
        <asp:Label ID="Count" runat="server" Text="0"></asp:Label>
        <br />
        <asp:ObjectDataSource 
            ID="ChecksumDS" 
            runat="server" 
            TypeName="TESTCASE_CHECKSUMS_VIEW" 
            SortParameterName="SortColumns"
            EnablePaging="true"
            SelectCountMethod="SelectCount"
            StartRowIndexParameterName="StartRecord"
            MaximumRowsParameterName="MaxRecords"
            SelectMethod="QueryTestcases"
            OnSelected="ChecksumDS_OnSelected">
            <SelectParameters>
                <asp:Parameter Name="Filter" Type="string" />  
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>
</asp:Content>

