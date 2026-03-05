<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="RiddleHub.Login" %>

    <!DOCTYPE html>

    <html xmlns="http://www.w3.org/1999/xhtml">
    <script src="iframe.js?v=20260305b"></script>

    <head runat="server">
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Document</title>
        <link rel="stylesheet" href="pagestyles.css?v=20260305b">

        <style>
            #loginContainer {
                max-width: 400px;
            }
        </style>
    </head>

    <body>
        <div id="sessionBridge" style="display:none" data-loggedin="<%= UtilFunctions.UtilFunctionsClass.IsLoggedIn(Session).ToString().ToLowerInvariant() %>" data-username="<%= Session["username"] ?? "" %>"></div>
        <div id="returnPage" style="display:none"><%= ReturnPage %></div>
        <div id="loginContainer" class="container page-card">
            <h1 class="page-title">Login</h1>
            <form action="login.aspx" method="post" runat="server">
                <div class="form-group">
                    <label for="username">Email:</label>
                    <input type="email" name="username" id="usernameInput" class="form-control" required>
                </div>
                <div class="form-group">
                    <label for="password">Password:</label>
                    <input type="password" name="password" id="passwordInput" class="form-control" required>
                </div>
                <input type="submit" value="Login" name="submit" class="btn btn-full">
                <div id="Result">
                    <asp:Literal ID="resultLiteral" runat="server"></asp:Literal>
                </div>
            </form>
        </div>

        <script>
            const loginSucceeded = <%= LoginSucceeded ? "true" : "false" %>;
            const userName = "<%= System.Web.HttpUtility.JavaScriptStringEncode(LoggedInUsername ?? string.Empty) %>";
            relayMessage("login");
            if (loginSucceeded && userName.length > 0) {
                relayMessage("USER" + userName)
                relayMessage("AUTHSTATE" + JSON.stringify({ loggedIn: true, username: userName }))
                let returnPage = document.getElementById('returnPage').textContent.trim() || 'my';
                window.location.assign('/' + returnPage + '.aspx');
            }
        </script>
    </body>

    </html>
