<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="home.aspx.cs" Inherits="RiddleHub.home" %>

    <!DOCTYPE html>

    <html xmlns="http://www.w3.org/1999/xhtml">
    <script src="iframe.js?v=20260305b"></script>

    <head runat="server">
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Document</title>
        <link rel="stylesheet" href="pagestyles.css?v=20260305b">

        <style>
            #availableRiddlesContainer {
                max-width: 900px;
                margin-bottom: 0;
            }

            #availableRiddlesContainer h1 {
                margin-bottom: 20px;
            }

            #availableRiddlesContainer ul {
                list-style: none;
                padding: 0;
            }

            #availableRiddlesContainer li {
                margin-bottom: 12px;
                padding: 12px;
                border: 1px solid var(--border);
                border-radius: 8px;
                background: var(--surface-soft);
            }

            #availableRiddlesContainer p {
                margin: 0;
            }
        </style>
    </head>

    <body>
        <div id="sessionBridge" style="display:none" data-loggedin="<%= UtilFunctions.UtilFunctionsClass.IsLoggedIn(Session).ToString().ToLowerInvariant() %>" data-username="<%= Session["username"] ?? "" %>"></div>
        <div id="availableRiddlesContainer" class="container page-card">
            <h1 class="page-title">Available Riddles</h1>
            <ul id="Riddles" class="list-reset">
                <!--Riddles will be added dynamically :)-->
            </ul>
        </div>

        <script src="home.js?v=20260305b"></script>
        <script>
            relayMessage("home")
        </script>

    </body>

    </html>
