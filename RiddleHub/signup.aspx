<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="signup.aspx.cs" Inherits="RiddleHub.signup" %>

    <!DOCTYPE html>

    <html xmlns="http://www.w3.org/1999/xhtml">
    <script src="iframe.js?v=20260305b"></script>

    <head runat="server">
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Document</title>
        <link rel="stylesheet" href="pagestyles.css?v=20260305b">

        <style>
            #signupContainer {
                max-width: 400px;
            }
        </style>
    </head>

    <body>

        <div id="sessionBridge" style="display:none" data-loggedin="<%= UtilFunctions.UtilFunctionsClass.IsLoggedIn(Session).ToString().ToLowerInvariant() %>" data-username="<%= Session["username"] ?? "" %>"></div>
        <div id="returnPage" style="display:none"><%= ReturnPage %></div>
        <div id="signupContainer" class="container page-card">
            <h1 class="page-title">Sign Up</h1>
            <form action="signup.aspx" method="post">
                <div class="form-group">
                    <label for="username">Username:</label>
                    <input type="text" name="username" id="usernameInput" class="form-control" autocomplete="username"
                        required="required">
                </div>
                <div class="form-group">
                    <label for="email">Email:</label>
                    <input type="email" name="email" id="emailInput" class="form-control" autocomplete="email"
                        required="required">
                </div>
                <div class="form-group">
                    <label for="password">Password:</label>
                    <input type="password" name="password" id="passwordInput" class="form-control"
                        autocomplete="new-password" required="required">
                    <div id="passwordFeedback" class="invalid-feedback"></div>
                </div>
                <button type="submit" name="submit" class="btn btn-full">Sign up</button>
                <div id="Result">
                    <asp:Literal ID="resultLiteral" runat="server"></asp:Literal>
                </div>
            </form>

        </div>

        <script>
            let res = document.getElementById("Result").textContent
            res = res.replaceAll("\n", "")
            res = res.replaceAll(" ", "");
            if (res !== "" && !res.startsWith("Error")) {
                const userName = document.getElementById('usernameInput').value.trim();
                relayMessage("AUTHSTATE" + JSON.stringify({ loggedIn: true, username: userName }));
                let returnPage = document.getElementById('returnPage').textContent.trim() || 'my';
                window.location.assign('/' + returnPage + '.aspx');
            }
            relayMessage("signup")
            document.getElementById('passwordInput').addEventListener('input', function () {
                const password = this.value;
                const feedback = document.getElementById('passwordFeedback');
                const lengthRequirement = password.length >= 8;
                const digitRequirement = /\d/.test(password);
                const capitalRequirement = /[A-Z]/.test(password);

                let requirements = [];
                if (!lengthRequirement) {
                    requirements.push('at least 8 characters long');
                }
                if (!digitRequirement) {
                    requirements.push('at least one digit');
                }
                if (!capitalRequirement) {
                    requirements.push('at least one capital letter');
                }

                if (requirements.length === 0) {
                    feedback.textContent = 'Password is strong!';
                    feedback.style.color = 'green';
                } else {
                    feedback.textContent = `Password must be ${requirements.join(', ')}.`;
                    feedback.style.color = 'red';
                }
            });

        </script>
    </body>

    </html>
