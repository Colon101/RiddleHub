<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="create.aspx.cs" Inherits="RiddleHub.Create" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Create a Riddle</title>
    <link rel="stylesheet" href="pagestyles.css?v=20260305b" />
    <script src="iframe.js?v=20260305b"></script>
    <style>#riddleCreator { max-width: 600px; margin-bottom: 0; }</style>
</head>
<body>
    <div id="sessionBridge" style="display:none"
        data-loggedin="<%= UtilFunctions.UtilFunctionsClass.IsLoggedIn(Session).ToString().ToLowerInvariant() %>"
        data-username="<%= System.Web.HttpUtility.HtmlAttributeEncode(System.Convert.ToString(Session["username"])) %>"></div>
    <div id="riddleCreator" class="container page-card">
        <h1 class="page-title">Create a Riddle</h1>
        <form class="riddle-form" action="create.aspx" method="post">
            <input type="hidden" name="csrf_token" value="<%= CsrfTokenEncoded %>" />
            <div class="form-group">
                <label for="riddleInput">Riddle:</label>
                <input type="text" name="riddle" id="riddleInput" maxlength="2000" required="required" />
            </div>
            <div class="form-group">
                <label for="hintInput">Hint: (optional)</label>
                <input type="text" name="hint" id="hintInput" maxlength="100" />
            </div>
            <div class="form-group">
                <label for="answerInput">Answer:</label>
                <input type="text" name="answer" id="answerInput" maxlength="100" required="required" />
            </div>
            <input type="submit" value="Create" name="submit" class="btn btn-full" />
        </form>
    </div>
    <script>relayMessage("create");</script>
</body>
</html>
