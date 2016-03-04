<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Tags.aspx.cs" Inherits="Tags" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>TUR-tool: Testcase Profiler/Cataloger (aka turtle).</title>
<script type="text/javascript">
    function deleteConfirm(pubid) {
        var result = confirm('Do you want to delete ' + pubid + ' ?');
        if (result) {
            return true;
        }
        else {
            return false;
        }
    }
</script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:GridView ID="TagView" runat="server" 
            DataSourceID="TagDS"
            DataKeyNames="TID"
            AutoGenerateColumns='false'
            AllowPaging='true'
            EmptyDataText ="There are no data here yet!"
            HeaderStyle-BackColor="PapayaWhip"
            AlternatingRowStyle-BackColor="LightCyan" PageSize="25"
            onrowcancelingedit="gridView_RowCancelingEdit"
            onrowdeleting="gridView_RowDeleting"
            onrowediting="gridView_RowEditing"
            onrowupdating="gridView_RowUpdating"
            onrowcommand="gridView_RowCommand"
            OnRowDataBound="gridView_RowDataBound">

            <Columns>
                <asp:BoundField HeaderText='ID' DataField='TID' ReadOnly="True" 
                    InsertVisible="False" HeaderStyle-Width="10%" />
                <asp:BoundField DataField="TAG_NAME" HeaderText="Tag" HeaderStyle-Width="20%" />
                <asp:BoundField DataField="TAG_DESCR" HeaderText="Description" HeaderStyle-Width="60%" />
                <asp:CommandField ShowEditButton="True" />
                <asp:CommandField ShowDeleteButton="True" />
            </Columns>
            <RowStyle ForeColor ="#000066" />
            <SelectedRowStyle BackColor ="#669999" Font-Bold ="True" ForeColor ="White" />
            <PagerStyle BackColor ="White" ForeColor ="#000066" HorizontalAlign ="Left" />
            <HeaderStyle BackColor ="#006699" Font-Bold ="True" ForeColor ="White" />
            
        </asp:GridView>
        <asp:Button ID="addTag" runat="server" CommandName="AddTag" Text="Add Tag"
            onclick="AddTag_Click" 
            OnClientClick="return confirm('This will mark _all_ of this supplier\'s
                products as discontinued. Are you certain you want to do this?');" />

        <asp:ObjectDataSource 
            ID="TagDS" 
            runat="server" 
            TypeName="Samples.AspNet.ObjectDataSource.TagData" 
            SortParameterName="SortColumns"
            EnablePaging="true"
            SelectCountMethod="SelectCount"
            StartRowIndexParameterName="StartRecord"
            MaximumRowsParameterName="MaxRecords" 
            SelectMethod="QueryTags"
            UpdateMethod="UpdateTag"
            DeleteMethod="DeleteTag" 
            InsertMethod="InsertTag">
        <SelectParameters>
          <asp:Parameter Name="TAG_NAME" Type="string" />  
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="TID" Type="String" />
            <asp:Parameter Name="TAG_NAME" Type="String" />
            <asp:Parameter Name="TAG_DESCR" Type="String" />
         </UpdateParameters>
         <DeleteParameters>
            <asp:Parameter Name="TID" Type="String" />
         </DeleteParameters>
        <InsertParameters>
            <asp:Parameter Name="TAG_NAME" Type="String" />
            <asp:Parameter Name="TAG_DESCR" Type="String" />
        </InsertParameters>
        </asp:ObjectDataSource>
    </div>
    </form>
</body>
</html>
