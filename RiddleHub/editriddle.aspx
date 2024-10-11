<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="editriddle.aspx.cs" Inherits="RiddleHub.editriddle" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <div id="riddleId">
    <asp:Literal ID="riddleInfo" runat="server"></asp:Literal> 
    </div>
    <form id="form1" runat="server" action="editriddle.aspx">
        <div>
        </div>
    </form>
    <script src="iframe.js">
    </script>
    <script>
        relayMessage("edit"+document.getElementById("riddleId").textContent.trim())
    </script>
</body>
</html>
