<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="signup.aspx.cs" Inherits="RiddleHub.signup" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Sign Up</title>
    <link rel="stylesheet" href="pagestyles.css?v=20260305b" />
    <script src="iframe.js?v=20260305b"></script>
    <style>#signupContainer { max-width: 400px; }</style>
</head>
<body>
    <div id="sessionBridge" style="display:none"
        data-loggedin="<%= UtilFunctions.UtilFunctionsClass.IsLoggedIn(Session).ToString().ToLowerInvariant() %>"
        data-username="<%= System.Web.HttpUtility.HtmlAttributeEncode(System.Convert.ToString(Session["username"])) %>"></div>
    <div id="returnPage" style="display:none"><%= ReturnPage %></div>
    <div id="signupContainer" class="container page-card">
        <h1 class="page-title">Sign Up</h1>
        <form action="signup.aspx" method="post">
            <input type="hidden" name="csrf_token" value="<%= CsrfTokenEncoded %>" />
            <input type="hidden" name="return" value="<%= System.Web.HttpUtility.HtmlAttributeEncode(ReturnPage) %>" />
            <div class="form-group">
                <label for="usernameInput">Username:</label>
                <input type="text" name="username" id="usernameInput" class="form-control" autocomplete="username" maxlength="300" required="required" />
            </div>
            <div class="form-group">
                <label for="emailInput">Email:</label>
                <input type="email" name="email" id="emailInput" class="form-control" autocomplete="email" maxlength="300" required="required" />
            </div>
            <div class="form-group">
                <label for="passwordInput">Password:</label>
                <input type="password" name="password" id="passwordInput" class="form-control" autocomplete="new-password" maxlength="128" required="required" />
                <div id="passwordFeedback" class="invalid-feedback"></div>
            </div>
            <button type="submit" name="submit" class="btn btn-full">Sign up</button>
            <div id="Result"><asp:Literal ID="resultLiteral" runat="server"></asp:Literal></div>
        </form>
    </div>
    <script>
        const signupSucceeded = <%= SignupSucceeded ? "true" : "false" %>;
        const userName = "<%= System.Web.HttpUtility.JavaScriptStringEncode(SignedUpUsername ?? string.Empty) %>";
        const returnPage = document.getElementById("returnPage").textContent.trim() || "my";
        if (signupSucceeded) {
            relayMessage("AUTHSTATE" + JSON.stringify({ loggedIn: true, username: userName }));
            window.location.assign("/" + returnPage + ".aspx");
        }
        relayMessage("signup");
        document.getElementById("passwordInput").addEventListener("input", function () {
            const password = this.value;
            const feedback = document.getElementById("passwordFeedback");
            const requirements = [];
            if (password.length < 8) requirements.push("at least 8 characters long");
            if (password.length > 128) requirements.push("at most 128 characters long");
            if (!/\d/.test(password)) requirements.push("at least one digit");
            if (!/[A-Z]/.test(password)) requirements.push("at least one capital letter");
            feedback.textContent = requirements.length === 0
                ? "Password meets the requirements."
                : "Password must be " + requirements.join(", ") + ".";
            feedback.style.color = requirements.length === 0 ? "green" : "red";
        });
    </script>
</body>
</html>
