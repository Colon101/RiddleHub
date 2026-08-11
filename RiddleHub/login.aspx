<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="RiddleHub.Login" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Login</title>
    <link rel="stylesheet" href="pagestyles.css?v=20260305b" />
    <script src="iframe.js?v=20260305b"></script>
    <style>#loginContainer { max-width: 400px; }</style>
</head>
<body>
    <div id="sessionBridge" style="display:none"
        data-loggedin="<%= UtilFunctions.UtilFunctionsClass.IsLoggedIn(Session).ToString().ToLowerInvariant() %>"
        data-username="<%= System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(Session["username"])) %>"></div>
    <div id="returnPage" style="display:none"><%= ReturnPage %></div>
    <div id="loginContainer" class="container page-card">
        <h1 class="page-title">Login</h1>
        <form action="login.aspx" method="post" runat="server">
            <input type="hidden" name="csrf_token" value="<%= CsrfTokenEncoded %>" />
            <input type="hidden" name="return" value="<%= System.Web.HttpUtility.HtmlAttributeEncode(ReturnPage) %>" />
            <div class="form-group">
                <label for="usernameInput">Email:</label>
                <input type="email" name="username" id="usernameInput" class="form-control" autocomplete="username" required="required" />
            </div>
            <div class="form-group">
                <label for="passwordInput">Password:</label>
                <input type="password" name="password" id="passwordInput" class="form-control" autocomplete="current-password" maxlength="128" required="required" />
            </div>
            <input type="submit" value="Login" name="submit" class="btn btn-full" />
            <div id="Result"><asp:Literal ID="resultLiteral" runat="server"></asp:Literal></div>
        </form>
    </div>
    <script>
        const loginSucceeded = <%= LoginSucceeded ? "true" : "false" %>;
        const resetRequired = <%= PasswordResetRequired ? "true" : "false" %>;
        const userName = "<%= System.Web.HttpUtility.JavaScriptStringEncode(LoggedInUsername ?? string.Empty) %>";
        const returnPage = document.getElementById("returnPage").textContent.trim() || "my";
        relayMessage("login");
        if (resetRequired) {
            window.location.assign("/resetpassword.aspx?return=" + encodeURIComponent(returnPage));
        } else if (loginSucceeded && userName.length > 0) {
            relayMessage("USER" + userName);
            relayMessage("AUTHSTATE" + JSON.stringify({ loggedIn: true, username: userName }));
            window.location.assign("/" + returnPage + ".aspx");
        }
    </script>
</body>
</html>
