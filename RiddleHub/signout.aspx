<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="signout.aspx.cs" Inherits="RiddleHub.Signout" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Sign Out</title>
    <link rel="stylesheet" href="pagestyles.css?v=20260305b" />
    <script src="iframe.js?v=20260305b"></script>
</head>
<body>
    <div class="container page-card" style="max-width:520px">
        <h1 class="page-title">Sign out?</h1>
        <p class="muted">Confirm to end this authenticated session.</p>
        <form method="post" action="signout.aspx">
            <input type="hidden" name="csrf_token" value="<%= CsrfTokenEncoded %>" />
            <button type="submit" class="btn-danger">Sign Out</button>
        </form>
    </div>
    <script>relayMessage("signout");</script>
</body>
</html>
