<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="my.aspx.cs" Inherits="My" %>

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

        #riddlesContainer {
            max-width: 800px;
            margin: 50px auto;
            padding: 20px;
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }

        h1 {
            text-align: center;
            color: #333;
        }

        .riddle {
            margin-bottom: 30px;
            list-style: none;
        }

        h3 {
            margin-bottom: 10px;
        }

        p {
            margin-bottom: 5px;
        }

        .editBtn {
            padding: 8px 16px;
            border: none;
            border-radius: 4px;
            background-color: #007bff;
            color: #fff;
            font-size: 16px;
            cursor: pointer;
        }

            .editBtn:hover {
                background-color: #0056b3;
            }
    </style>
</head>
<body>
    <div id="riddlesContainer" class="container">
        <h1>My Riddles</h1>
        <ul>
            <li class="riddle">
                <h3>Riddle: What is it that no one wants to have, but no one wants to lose, either?</h3>
                <p>Answer: A lawsuit</p>
                <button class="editBtn">Edit</button>
            </li>
            <li class="riddle">
                <h3>Riddle: What has two hands, a round face, always runs, yet always stays in place, too?</h3>
                <p>Hint: TikTok</p>
                <p>Answer: A clock!</p>
                <button class="editBtn">Edit</button>
            </li>
        </ul>
    </div>
    <script src="iframe.js"></script>
    <script>
        relayMessage("my")
    </script>
</body>

</html>
