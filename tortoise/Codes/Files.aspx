<%@ Page Title="Functions" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Files.aspx.cs" Inherits="Codes_Files" %>
<%@ Register
    Assembly="AjaxControlToolkit"
    Namespace="AjaxControlToolkit"
    TagPrefix="ajaxToolkit" %>

<%@ MasterType virtualpath="~/MasterPage.master" %>

<script runat="server">
    void _OnPageIndexChanged(object sender, EventArgs e)
    {
        TObjectDataSource.SelectParameters["Filter"].DefaultValue = (string)ViewState["Filter"];
        TObjectDataSource.DataBind();
    }

</script>
<asp:Content ID="Content3" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div>
        <asp:Button ID="func_map_btn" runat="server" Text="Upload function maps..." style="float:right" ToolTip="Upload function maps..." /><br />
        <asp:ScriptManager ID="asm" runat="server"></asp:ScriptManager>
        <ajaxToolkit:ModalPopupExtender ID="mdlpopup" BackgroundCssClass="modalbackground" runat="server" TargetControlID="func_map_btn"
                PopupControlID="pnl" OkControlID="btnok" Y="300" ></ajaxToolkit:ModalPopupExtender>

        <asp:Panel ID="pnl" runat="server" BorderColor="ActiveBorder" CssClass="modalpopup" BorderStyle="Solid" BorderWidth="2px">
            
            <ajaxToolkit:AjaxFileUpload ID="AjaxFileUpload1" runat="server"
                Width="400" Height="150"
                OnUploadComplete="AjaxFileUpload1_UploadComplete" AllowedFileTypes="txt,out"
                OnClientUploadComplete="OnClientUploadComplete"
                OnClientUploadError="showUploadError"
                ClearFileListAfterUpload="True" />
            <asp:Button ID="btnok" runat="server" Text="OK" />
        </asp:Panel>
    </div>
    <div class="testcases">
        <asp:GridView ID="GridView1" runat="server" 
            DataSourceID="TObjectDataSource"
            OnPageIndexChanged="_OnPageIndexChanged"
            DataKeyNames="SOURCE_FILE"
            AutoGenerateColumns='false'
            AllowPaging='true'
            PagerSettings-Mode="NumericFirstLast" 
            PagerSettings-PageButtonCount="10"
            HeaderStyle-BackColor="PapayaWhip"
            AlternatingRowStyle-BackColor="LightCyan" PageSize="25"
            OnDataBound="GridVIew_OnDataBound"
            EmptyDataText="No source files found!" Width="100%" EnableSortingAndPagingCallbacks="True" AllowSorting="True">
            <Columns>
                <asp:BoundField HeaderText='' DataField='ROWNO' 
                    HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField
                    HeaderText="Source File" DataField="SOURCE_FILE"/>
                <asp:BoundField HeaderText='Function' DataField='FUNC_NAME'
                    HeaderStyle-Width="30%"
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
            TypeName="FUNC" 
            SortParameterName="SortColumns"
            EnablePaging="true"
            SelectCountMethod="SelectCount"
            StartRowIndexParameterName="StartRecord"
            MaximumRowsParameterName="MaxRecords"
            SelectMethod="QueryTestcases">
            <SelectParameters>
              <asp:Parameter Name="Filter" Type="string" />  
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>
</asp:Content>

