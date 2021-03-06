﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>
<%@ Register
    Assembly="AjaxControlToolkit"
    Namespace="AjaxControlToolkit"
    TagPrefix="ajaxToolkit" %>

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
    <style type="text/css">
        .modalbackground 
        {
            background-color:Gray;
            opacity: 0.5;
            filter:Alpha(opacity=50);
        }
        .modalpopup 
        {
            background-color:White;
            padding:6px 6px 6px 6px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="asm" runat="server" />
        <ajaxToolkit:ModalPopupExtender ID="mdlpopup" BackgroundCssClass="modalbackground" runat="server" TargetControlID="btnShow"
                PopupControlID="pnl" OkControlID="btnok" Y="300" ></ajaxToolkit:ModalPopupExtender>

        <asp:Panel ID="pnl" runat="server" BorderColor="ActiveBorder" CssClass="modalpopup" BorderStyle="Solid" BorderWidth="2px">
            
            <ajaxToolkit:AjaxFileUpload ID="AjaxFileUpload1" runat="server"
                Width="400" Height="150"
                OnUploadComplete="AjaxFileUpload1_UploadComplete" AllowedFileTypes="txt,cs,checksum"
                ClearFileListAfterUpload="True" />
            <asp:Button ID="btnok" runat="server" Text="OK" />
        </asp:Panel>

        <asp:DropDownList ID="PersonaCbx" runat="server" Width="200px" Visible="false"
            onselectedindexchanged="drp2_SelectedIndexChanged" AutoPostBack="True">
            <asp:ListItem Text="All" Value="0"></asp:ListItem>
            <asp:ListItem Text="sim-atlantis" Value="5"></asp:ListItem>
            <asp:ListItem Text="sim_voy-ix86-Linux-RHEL5" Value="8"></asp:ListItem>
        </asp:DropDownList><br />
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
        
        <asp:Button ID="btnShow" runat="server" Text="Update checksum..." style="float:right" ToolTip="Update master checksums..." /><br />
        <asp:Label ID="Label1" runat="server" Text="Total Count: " /><asp:Label ID="totalLbl" runat="server" Text="" /><br />
    <div>    
         <!--EmptyDataTemplate
                No Records Available
                asp:Image ID="Image1" ImageUrl=""
                a href="Default.aspx" Try to Reload a 
            EmptyDataTemplate-->
        <asp:GridView ID="GridView1" runat="server" 
            DataSourceID="TObjectDataSource"
            OnPageIndexChanged="_OnPageIndexChanged"
            DataKeyNames="Filter"
            AutoGenerateColumns='false'
            AllowPaging='true'
            PagerSettings-Mode="NumericFirstLast" 
            PagerSettings-PageButtonCount="10"
            HeaderStyle-BackColor="PapayaWhip"
            AlternatingRowStyle-BackColor="LightCyan" PageSize="25"
            OnDataBound="GridVIew_OnDataBound"
            EmptyDataText="No testcase found!">
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
                <asp:BoundField HeaderText='Location' DataField='TLOC' 
                    ItemStyle-HorizontalAlign="Left" />
            </Columns>
            <RowStyle ForeColor ="#000066" />
            <SelectedRowStyle BackColor ="#669999" Font-Bold ="True" ForeColor ="White" />
            <HeaderStyle BackColor ="#006699" Font-Bold ="True" ForeColor ="White" />
            <PagerStyle HorizontalAlign="Right" />
        </asp:GridView>
        <br />
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
    
    <br />
    <asp:Label ID="querySQL" runat="server" Text='' /><br />
    </form>
</body>
</html>
