<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="success.aspx.cs" Inherits="RiddleHub.success" %>

    <!DOCTYPE html>

    <html xmlns="http://www.w3.org/1999/xhtml">
    <script src="iframe.js?v=20260305b"></script>

    <head runat="server">
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>Signed Out</title>
        <link rel="stylesheet" href="pagestyles.css?v=20260305b" />
        <style>
            #riddlesContainer {
                max-width: 800px;
                margin-bottom: 0;
            }
        </style>
    </head>

    <body>
        <form id="form1" runat="server">

            <div id="riddlesContainer" class="container page-card">
                <h1 class="page-title">Signed out successfully!</h1>
                <div id="signoutD">
                    <asp:Literal ID="signout" runat="server"></asp:Literal>
                </div>
            </div>
        </form>
        <script>
            let res = document.getElementById("signoutD").textContent.replaceAll("\n", "").trim()
            if (res == "reload") {
                relayMessage("SIGNOUT");
            }
        </script>
    </body>

    </html>
