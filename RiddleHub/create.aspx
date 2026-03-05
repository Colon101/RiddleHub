<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="create.aspx.cs" Inherits="RiddleHub.Create" %>

    <!DOCTYPE html>
    <html xmlns="http://www.w3.org/1999/xhtml">
    <script src="iframe.js?v=20260305b"></script>

    <head runat="server">
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>Document</title>
        <link rel="stylesheet" href="pagestyles.css?v=20260305b" />
        <style>
            #riddleCreator {
                max-width: 600px;
                margin-bottom: 0;
            }
        </style>
    </head>

    <body>
        <div id="sessionBridge" style="display:none" data-loggedin="<%= UtilFunctions.UtilFunctionsClass.IsLoggedIn(Session).ToString().ToLowerInvariant() %>" data-username="<%= Session["username"] ?? "" %>"></div>
        <div id="riddleCreator" class="container page-card">
            <h1 class="page-title">Create a Riddle</h1>
            <form class="riddle-form" action="create.aspx" method="post">
                <div class="form-group">
                    <label for="riddle">Riddle:</label>
                    <input type="text" name="riddle" id="riddleInput" class="form-control" required="required" />
                </div>
                <div class="form-group">
                    <label for="hint">Hint: (optional)</label>
                    <input type="text" name="hint" id="hintInput" class="form-control" />
                </div>
                <div class="form-group">
                    <label for="answer">Answer:</label>
                    <input type="text" name="answer" id="answerInput" class="form-control" required="required" />
                </div>
                <input type="submit" value="Create" name="submit" class="btn btn-full" />
            </form>
        </div>

        <script>
            relayMessage("create")
        </script>
    </body>

    </html>
