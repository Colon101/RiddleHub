<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="home.aspx.cs" Inherits="RiddleHub.home" %>

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

            #availableRiddlesContainer {
                max-width: 900px;
                margin: 30px auto;
                margin-bottom: 0;
                padding: 20px;
                background-color: #fff;
                border-radius: 8px;
                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            }

            h1 {
                margin-bottom: 20px;
                text-align: center;
                color: #333;
            }

            ul {
                list-style: none;
                padding: 0;
            }

            li {
                margin-bottom: 20px;
            }

            p {
                margin: 0;
            }

            a {
                color: #007bff;
                text-decoration: none;
                margin-right: 0;
            }

            a:hover {
                text-decoration: underline;
            }
        </style>
    </head>

    <body>
        <div id="availableRiddlesContainer" class="container">
            <h1>Available Riddles</h1>
            <ul id="Riddles">
                <!--Riddles will be added dynamically :)-->
            </ul>
        </div>

        <script src="home.js"></script>
        <script src="iframe.js"></script>
        <script>
            relayMessage("home")

        </script>

    </body>

    </html>