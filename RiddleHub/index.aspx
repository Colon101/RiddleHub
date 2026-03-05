<%@ Page Language="C#" %>

    <!DOCTYPE html>

    <html xmlns="http://www.w3.org/1999/xhtml">

    <head runat="server">
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <link rel="icon" type="image/png" href="logo.png" />
        <script>
            (function () {
                const savedTheme = localStorage.getItem("riddlehub-theme");
                if (savedTheme === "dark" || savedTheme === "light") {
                    document.documentElement.setAttribute("data-theme", savedTheme);
                }
            })();
        </script>
        <link rel="stylesheet" href="styles.css?v=20260305c" />
        <title>Riddle Hub</title>
        <meta name="description" content="a site for riddles" />
    </head>

    <body>
        <div class="navbar">
            <header class="header">
                <div class="logo">
                    <a href="#" class="pages" id="nav-home-icon" data-page="home">
                        <img src="logo.png" alt="Riddler logo" height="88" />
                    </a>
                </div>
                <div class="navigation">
                    <input type="checkbox" name="" id="" class="toggle-menu" />
                    <div class="hamburger">
                    </div>
                    <ul class="menu">
                        <li><a class="pages" id="nav-home" data-page="home" href="#">Home</a></li>
                        <li><a class="pages" id="nav-create" data-page="create" href="#">Create a Riddle</a></li>
                        <li><a class="pages" id="nav-my" data-page="my" href="#">My Riddles</a></li>
                        <li><a class="pages" id="nav-login" data-page="login" href="#">Login</a></li>
                        <li><a class="pages" id="nav-signup" data-page="signup" href="#">Sign Up</a></li>
                    </ul>
                    <button type="button" id="themeToggle" class="theme-toggle-btn" aria-label="Switch theme">
                        <span class="theme-toggle-icon" aria-hidden="true">
                            <svg id="themeIconMoon" viewBox="0 0 24 24" focusable="false">
                                <path d="M21 12.79A9 9 0 1 1 11.21 3a7 7 0 0 0 9.79 9.79z"></path>
                            </svg>
                            <svg id="themeIconSun" viewBox="0 0 24 24" focusable="false">
                                <circle cx="12" cy="12" r="4"></circle>
                                <path d="M12 2v2m0 16v2M4.93 4.93l1.41 1.41m11.32 11.32l1.41 1.41M2 12h2m16 0h2M4.93 19.07l1.41-1.41m11.32-11.32l1.41-1.41"></path>
                            </svg>
                        </span>
                    </button>
                </div>
            </header>
            <hr class="separator" />
        </div>

        <iframe src="" frameborder="0" id="content" width="100%" height="100%" style="z-index: 0; position: absolute;">
            <!--SRC ADDED BY JS!-->
            <p>Your browser does not support iframes.</p>
        </iframe>

        <script src="index.js?v=20260305c"></script>
    </body>

    </html>
