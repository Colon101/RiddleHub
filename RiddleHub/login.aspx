<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="RiddleHub.Login" %>

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

        #loginContainer {
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
        input[type="password"],
        input[type="email"] {
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
    <div id="loginContainer" class="container">
        <form action="login.aspx" method="post" runat="server">
            <div class="form-group">
                <label for="username">Email:</label>
                <input type="email" name="username" id="usernameInput" class="form-control" required>
            </div>
            <div class="form-group">
                <label for="password">Password:</label>
                <input type="password" name="password" id="passwordInput" class="form-control" required>
            </div>
            <input type="submit" value="Login" name="submit" class="btn">
            <div id="Result">
                <asp:Literal ID="resultLiteral" runat="server"></asp:Literal>
            </div>
        </form>
    </div>

    <script src="iframe.js"></script>
    <script>
        function getTextBetweenQuotes(str) {
            let start = str.indexOf("'");
            if (start === -1) return null;  // No single quote found

            let end = str.indexOf("'", start + 1);
            if (end === -1) return null;  // No closing single quote found

            return str.slice(start + 1, end);
        }


        relayMessage("login")
        let res = document.getElementById("Result").textContent
        res = res.replaceAll("\n", "")
        res = res.trim();
        if (res.startsWith("Success")) {
            let arr = res.split(" ");
            relayMessage("USER" + getTextBetweenQuotes(res))
            window.location.assign('/my.aspx');
        }
    </script>
</body>

</html>
