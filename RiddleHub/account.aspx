<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="account.aspx.cs" Inherits="RiddleHub.account" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<script src="iframe.js"></script>

<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Account</title>
    <link rel="stylesheet" href="pagestyles.css" />
    <style>
        body { font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }
        #accountContainer { max-width: 820px; margin: 36px auto; padding: 20px; background-color: #fff; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }
        h1, h2 { color: #333; }
        .muted { color: #666; margin-top: 0; }
        .section { border: 1px solid #e2e8f0; border-radius: 8px; padding: 14px; margin-top: 18px; }
        .danger { border-color: #fecaca; background-color: #fef2f2; }
        .form-group { margin-bottom: 14px; }
        label { display: block; margin-bottom: 5px; color: #555; }
        input[type="text"], input[type="password"] { width: 100%; padding: 10px; border: 1px solid #ccc; border-radius: 4px; font-size: 16px; box-sizing: border-box; }
        button { border: none; border-radius: 4px; padding: 10px 14px; color: #fff; font-size: 15px; cursor: pointer; background-color: #007bff; }
        button:hover { background-color: #0056b3; }
        button.warn { background-color: #c62828; }
        button.warn:hover { background-color: #8e1f1f; }
        .status { margin-top: 12px; border-radius: 8px; padding: 10px 12px; border: 1px solid #c7d2fe; background: #eef2ff; color: #1e3a8a; }
        .status.error { border-color: #fecaca; background: #fef2f2; color: #7f1d1d; }
    </style>
</head>

<body>
    <div id="sessionBridge" style="display:none" data-loggedin="true" data-username="<%= Session["username"] ?? "" %>"></div>
    <div id="usernametxt" style="display: none"><asp:Literal ID="username" runat="server"></asp:Literal></div>
    <div id="accountContainer" class="container">
        <h1>Account Settings</h1>
        <p class="muted">Logged in as <strong><%= CurrentUsernameEncoded %></strong></p>

        <% if (!string.IsNullOrEmpty(StatusMessageEncoded)) { %>
            <div class="status <%= IsErrorStatus ? "error" : string.Empty %>"><%= StatusMessageEncoded %></div>
        <% } %>

        <div class="section">
            <h2>Change Username</h2>
            <form action="account.aspx" method="post">
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
                <input type="hidden" name="action" value="change_password" />
                <div class="form-group">
                    <label for="currentPasswordInput">Current password</label>
                    <input type="password" name="current_password" id="currentPasswordInput" required="required" />
                </div>
                <div class="form-group">
                    <label for="newPasswordInput">New password</label>
                    <input type="password" name="new_password" id="newPasswordInput" required="required" />
                </div>
                <button type="submit">Update Password</button>
            </form>
        </div>

        <div class="section danger">
            <h2>Delete Account</h2>
            <p class="muted">This deletes your account and all your riddles permanently.</p>
            <form action="account.aspx" method="post">
                <input type="hidden" name="action" value="delete_account" />
                <div class="form-group">
                    <label for="deletePasswordInput">Confirm with current password</label>
                    <input type="password" name="confirm_password" id="deletePasswordInput" required="required" />
                </div>
                <button type="submit" class="warn">Delete Account</button>
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

        const accountDeleted = <%= AccountDeleted ? "true" : "false" %>;
        if (accountDeleted) {
            relayMessage("SIGNOUT");
            window.location.assign("/home.aspx");
        }
    </script>
</body>

</html>
