<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="signup.aspx.cs" Inherits="RiddleHub.signup" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Document</title>
    <link rel="stylesheet" href="pagestyles.css">

<style>
    body {
        font-family: Arial, sans-serif;
        background-color: #f4f4f4;
        margin: 0;
        padding: 0;
    }

    #signupContainer {
        max-width: 400px;
        margin: 50px auto;
        padding: 20px;
        background-color: #fff;
        border-radius: 8px;
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    }

    .form-group {
        margin-bottom: 20px;
    }

    label {
        display: block;
        margin-bottom: 5px;
        color: #666;
    }

    input[type="text"],
    input[type="email"],
    input[type="password"] {
        width: 100%;
        padding: 10px;
        border: 1px solid #ccc;
        border-radius: 4px;
        font-size: 16px;
    }

    .btn {
        display: block;
        width: 100%;
        padding: 10px;
        border: none;
        border-radius: 4px;
        background-color: #007bff;
        color: #fff;
        font-size: 16px;
        cursor: pointer;
    }

    .btn:hover {
        background-color: #0056b3;
    }
</style>
    </head>
<body>

    <div id="signupContainer" class="container">
        <form action="signup.aspx" method="post">
            <div class="form-group">
                <label for="username">Username:</label>
                <input type="text" name="username" id="usernameInput" class="form-control">
            </div>
            <div class="form-group">
                <label for="email">Email:</label>
                <input type="email" name="email" id="emailInput" class="form-control">
            </div>
            <div class="form-group">
                <label for="password">Password:</label>
                <input type="password" name="password" id="passwordInput" class="form-control">           
                <div id="passwordFeedback" class="invalid-feedback"></div>

            </div>
            <button type="submit" name="submit" class="btn">Sign up</button>
            <asp:Literal ID="resultLiteral" runat="server"></asp:Literal>

        </form>
    </div>
    <script src="iframe.js"></script>
    <script>
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