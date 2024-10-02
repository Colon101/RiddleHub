<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="success.aspx.cs" Inherits="RiddleHub.success" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }

        #riddlesContainer {
            max-width: 800px;
            margin: 50px auto;
            padding: 20px;
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }

        h1 {
            text-align: center;
            color: #333;
        }

        .riddle {
            margin-bottom: 30px;
            list-style: none;
        }

        h3 {
            margin-bottom: 10px;
        }

        p {
            margin-bottom: 5px;
        }

        .editBtn {
            padding: 8px 16px;
            border: none;
            border-radius: 4px;
            background-color: #007bff;
            color: #fff;
            font-size: 16px;
            cursor: pointer;
        }

        .center img {
            display: block;
            margin: 0 auto;
            transform: scale(0.5);
        }

        .editBtn:hover {
            background-color: #0056b3;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">

        <div id="riddlesContainer">
            <h1>Signed out successfully!</h1>
            <div id="signoutD">
            <asp:Literal ID="signout" runat="server"></asp:Literal>
                </div>
        </div>
    </form>
    <script src="iframe.js"></script>
    <script>
        let res = document.getElementById("signoutD").textContent.replaceAll("\n","").trim()
        if (res == "reload") {
            relayMessage("reloadout");
        }
    </script>
</body>
</html>
