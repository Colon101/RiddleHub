<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="resetpassword.aspx.cs" Inherits="RiddleHub.resetpassword" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Reset Password</title>
    <link rel="stylesheet" href="pagestyles.css?v=20260305b" />
    <script src="iframe.js?v=20260305b"></script>
</head>
<body>
    <div class="container page-card" style="max-width:460px">
        <h1 class="page-title">Choose a new password</h1>
        <p class="muted">This account used the legacy password format. Choose a different password to finish signing in.</p>
        <% if (!string.IsNullOrEmpty(StatusMessageEncoded)) { %>
            <div class="status <%= IsErrorStatus ? "error" : string.Empty %>"><%= StatusMessageEncoded %></div>
        <% } %>
        <% if (!ResetSucceeded) { %>
            <form action="resetpassword.aspx?return=<%= Server.UrlEncode(ReturnPage) %>" method="post">
                <input type="hidden" name="csrf_token" value="<%= CsrfTokenEncoded %>" />
                <div class="form-group">
                    <label for="newPasswordInput">New password</label>
                    <input type="password" name="new_password" id="newPasswordInput" maxlength="128" autocomplete="new-password" required="required" />
                </div>
                <div class="form-group">
                    <label for="confirmPasswordInput">Confirm new password</label>
                    <input type="password" name="confirm_password" id="confirmPasswordInput" maxlength="128" autocomplete="new-password" required="required" />
                </div>
                <button type="submit">Reset password</button>
            </form>
        <% } %>
    </div>
    <script>
        const resetSucceeded = <%= ResetSucceeded ? "true" : "false" %>;
        const username = "<%= System.Web.HttpUtility.JavaScriptStringEncode(LoggedInUsername ?? string.Empty) %>";
        if (resetSucceeded) {
            relayMessage("AUTHSTATE" + JSON.stringify({ loggedIn: true, username: username }));
            window.location.assign("/<%= ReturnPage %>.aspx");
        }
    </script>
</body>
</html>
