<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="signout.aspx.cs" Inherits="RiddleHub.Signout" %>

    <!DOCTYPE html>

    <html xmlns="http://www.w3.org/1999/xhtml">
    <script src="iframe.js?v=20260305b"></script>

    <head runat="server">
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>Sign Out</title>
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
                <h1 class="page-title">Signing you out...
                </h1>
                <div class="center">
                    <img src="loading.gif" />
                </div>
            </div>
        </form>
        <script>relayMessage("signout")</script>
    </body>

    </html>
