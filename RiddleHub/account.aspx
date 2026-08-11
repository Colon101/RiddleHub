<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="account.aspx.cs" Inherits="RiddleHub.account" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Account</title>
    <link rel="stylesheet" href="pagestyles.css?v=20260305b" />
    <script src="iframe.js?v=20260305b"></script>
    <style>
        #accountContainer { max-width: 820px; margin-bottom: 0; }
        .muted { margin-top: 0; }
    </style>
</head>
<body>
    <div id="sessionBridge" style="display:none" data-loggedin="true"
        data-username="<%= System.Web.HttpUtility.HtmlAttributeEncode(System.Convert.ToString(Session["username"])) %>"></div>
    <div id="usernametxt" style="display:none"><asp:Literal ID="username" runat="server"></asp:Literal></div>
    <div id="accountContainer" class="container page-card">
        <h1 class="page-title">Account Settings</h1>
        <p class="muted">Logged in as <strong><%= CurrentUsernameEncoded %></strong></p>
        <% if (!string.IsNullOrEmpty(StatusMessageEncoded)) { %>
            <div class="status <%= IsErrorStatus ? "error" : string.Empty %>"><%= StatusMessageEncoded %></div>
        <% } %>

        <div class="section">
            <h2>Change Username</h2>
            <form action="account.aspx" method="post">
                <input type="hidden" name="csrf_token" value="<%= CsrfTokenEncoded %>" />
                <input type="hidden" name="action" value="change_username" />
                <div class="form-group">
                    <label for="newUsernameInput">New username</label>
                    <input type="text" name="new_username" id="newUsernameInput" maxlength="300" required="required" />
                </div>
                <button type="submit">Update Username</button>
            </form>
        </div>

        <div class="section">
            <h2>Change Password</h2>
            <form action="account.aspx" method="post">
                <input type="hidden" name="csrf_token" value="<%= CsrfTokenEncoded %>" />
                <input type="hidden" name="action" value="change_password" />
                <div class="form-group">
                    <label for="currentPasswordInput">Current password</label>
                    <input type="password" name="current_password" id="currentPasswordInput" maxlength="128" autocomplete="current-password" required="required" />
                </div>
                <div class="form-group">
                    <label for="newPasswordInput">New password</label>
                    <input type="password" name="new_password" id="newPasswordInput" maxlength="128" autocomplete="new-password" required="required" />
                </div>
                <button type="submit">Update Password</button>
            </form>
        </div>

        <div class="section section-danger">
            <h2>Delete Account</h2>
            <p class="muted">This deletes your account and all your riddles permanently.</p>
            <form action="account.aspx" method="post">
                <input type="hidden" name="csrf_token" value="<%= CsrfTokenEncoded %>" />
                <input type="hidden" name="action" value="delete_account" />
                <div class="form-group">
                    <label for="deletePasswordInput">Confirm with current password</label>
                    <input type="password" name="confirm_password" id="deletePasswordInput" maxlength="128" autocomplete="current-password" required="required" />
                </div>
                <button type="submit" class="btn-danger">Delete Account</button>
            </form>
        </div>
    </div>
    <script>
        relayMessage("account");
        const username = (document.getElementById("usernametxt").textContent || "").trim();
        if (username.length > 0) {
            relayMessage("USER" + username);
            relayMessage("AUTHSTATE" + JSON.stringify({ loggedIn: true, username: username }));
        }
        if (<%= AccountDeleted ? "true" : "false" %>) {
            relayMessage("SIGNOUT");
            window.location.assign("/home.aspx");
        }
    </script>
</body>
</html>
