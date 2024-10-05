<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="create.aspx.cs" Inherits="RiddleHub.Create" %>

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

        #riddleCreator {
            max-width: 600px;
            margin: 50px auto;
            padding: 20px;
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            height: contain;
            margin-bottom: 0;
        }


        h1 {
            text-align: center;
            color: #333;
        }

        .form-group {
            margin-bottom: 20px;
        }

        label {
            display: block;
            margin-bottom: 5px;
            color: #666;
        }

        input[type="text"] {
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
    <div id="riddleCreator" class="container">
        <h1>Create a Riddle</h1>
        <form class="riddle-form">
            <div class="form-group">
                <label for="riddle">Riddle:</label>
                <input type="text" name="riddle" id="riddleInput" class="form-control" required>
            </div>
            <div class="form-group">
                <label for="hint">Hint: (optional)</label>
                <input type="text" name="hint" id="hintInput" class="form-control">
            </div>
            <div class="form-group">
                <label for="answer">Answer:</label>
                <input type="text" name="answer" id="answerInput" class="form-control" required>
            </div>
            <input type="submit" value="Create" class="btn">
        </form>
    </div>
    <script src="iframe.js"></script>
    <script>
        relayMessage("create")
    </script>
</body>

</html>
