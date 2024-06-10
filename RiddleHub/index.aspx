<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="RiddleHub.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="UTF-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <link rel="icon" type="image/png" href="logo.png" />
    <link rel="stylesheet" href="styles.css"/>
    <title>Riddle Hub</title>
    <meta name="description" content="a site for riddles" />
</head>

<body>
    <div class="navbar">
        <header class="header">
            <div class="logo">
                <a href="#" class="pages" id="homeIcon">
                    <img src="logo.png" alt="Riddler logo" height="88px"/>
                </a>
            </div>
            <div class="navigation">
                <input type="checkbox" name="" id="" class="toggle-menu"/>
                <div class="hamburger">

                </div>
                <ul class="menu">
                    <li><a class="pages" id="home" href="#">Home</a></li>
                    <li><a class="pages" id="create" href="#create">Create a Riddle</a></li>
                    <li><a class="pages" id="my" href="#my">My Riddles</a></li>
                    <li><a class="pages" id="login" href="#login">Login</a></li>
                    <li><a class="pages" id="signup" href="#signup">Sign Up</a></li>
                </ul>
            </div>
        </header>
        <hr class="separator"/>
    </div>

    <iframe src="" frameborder="0" id="content" width="100%" height="100%" style="z-index: 0;position: absolute;">
        <!--SRC ADDED BY JS!-->
        <p>Your browser does not support iframes.</p>
    </iframe>

    <script src="index.js"></script>
</body>
</html>
