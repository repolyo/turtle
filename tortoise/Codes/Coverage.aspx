<%@ Page Title="Code coverage report" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Coverage.aspx.cs" Inherits="Codes_Coverage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">    
    <div class="testcases">
        <asp:GridView ID="GridView1" runat="server" 
            DataSourceID="CoverageDataSource"
            DataKeyNames="EMULATOR"
            AutoGenerateColumns='false'
            AllowPaging='true'
            PagerSettings-Mode="NumericFirstLast" 
            PagerSettings-PageButtonCount="10"
            HeaderStyle-BackColor="PapayaWhip"
            AlternatingRowStyle-BackColor="LightCyan" PageSize="25"
            OnDataBound="Codes_OnDataBound"
            EmptyDataText="No test coverage!" Width="100%" EnableSortingAndPagingCallbacks="True" AllowSorting="True">
            <Columns>
                <asp:BoundField HeaderText='' DataField='ROWNO' 
                    HeaderStyle-Width="5%" ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField HeaderText="Emulator" DataField="EMULATOR"/>
                <asp:BoundField HeaderText="LCOV" DataField="LCOV"/>
                <asp:BoundField HeaderText="Total Functions" DataField="TOTAL"/>
                <asp:BoundField HeaderText="Tested" DataField="USED"/>
                <asp:BoundField HeaderText='Coverage' DataField='COVERAGE'
                    HeaderStyle-Width="30%"
                    ItemStyle-HorizontalAlign="Left" />
            </Columns>
            <RowStyle ForeColor ="#000066" />
            <SelectedRowStyle BackColor ="#669999" Font-Bold ="True" ForeColor ="White" />
            <HeaderStyle BackColor ="#009900" Font-Bold ="True" ForeColor ="White" />
            <PagerStyle HorizontalAlign="Right" />
        </asp:GridView>

        <asp:ObjectDataSource 
            ID="CoverageDataSource" runat="server" TypeName="FUNCTION_COVERAGE_VIEW" 
            SortParameterName="SortColumns"
            EnablePaging="true"
            SelectCountMethod="SelectCount"
            StartRowIndexParameterName="StartRecord"
            MaximumRowsParameterName="MaxRecords"
            SelectMethod="QueryFunctionCoverage">
            <SelectParameters>
              <asp:Parameter Name="Filter" Type="string" />  
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>
</asp:Content>

