<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Upload.aspx.cs" Inherits="Upload" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Upload file</title>
</head>
<body> 
     <form id="form1" runat="server" enctype="multipart/form-data">
        <div>
            <label for="caption">Image Caption</label>
            <input name="caption" type="text" />
        </div>
        <div>
            <label for="image1">Image File</label>
            <input id="file" name="file" type="file" />
        </div>
        <div>
            <asp:Button runat="server" ID="btnUpload" OnClick="UploadButton_Click" Text="Upload" />
        </div>
    </form>
</body>
</html>

