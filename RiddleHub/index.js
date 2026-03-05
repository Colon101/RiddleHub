window.addEventListener("click", (event) => {
    const navDiv = document.querySelector(".navigation");
    if (!navDiv.contains(event.target)) {
        document.querySelector(".toggle-menu").checked = false;
    }
});

function normalizePage(pageName) {
    const validPages = new Set(["home", "create", "my", "login", "signup", "signout"]);
    if (!pageName || !validPages.has(pageName)) {
        return "home";
    }
    return pageName;
}

function currentHashPage() {
    const hash = window.location.hash.slice(1).trim();
    if (!hash) return "home";
    if (hash.startsWith("edit")) return "my";
    return normalizePage(hash);
}

function updateNavForAuth(isLoggedIn, username) {
    const loginLink = document.getElementById("login");
    const signupLink = document.getElementById("signup");

    if (isLoggedIn) {
        loginLink.textContent = "Hi! " + username;
        signupLink.textContent = "Account";
    } else {
        loginLink.textContent = "Login";
        signupLink.textContent = "Sign Up";
    }
}

function loadPageInIframe(pageId) {
    const contentElement = document.getElementById("content");
    contentElement.src = pageId + ".aspx";
}

window.addEventListener("message", (event) => {
    if (typeof event.data !== "string") {
        return;
    }

    const message = event.data.trim();
    if (message === "clickedInsideIframe") {
        hideNav();
        return;
    }
    if (message === "reload") {
        window.location.hash = "#my";
        window.location.reload();
        return;
    }
    if (message === "reloadout") {
        window.location.hash = "";
        window.location.reload();
        return;
    }
    if (message.startsWith("AUTHSTATE")) {
        try {
            const payload = JSON.parse(message.substring("AUTHSTATE".length));
            updateNavForAuth(payload.loggedIn, payload.username || "");
        } catch (error) {
            console.log("Failed parsing AUTHSTATE", error);
        }
        return;
    }
    if (message.startsWith("USER")) {
        updateNavForAuth(true, message.substring(4).trim());
        return;
    }
    if (message === "SIGNOUT") {
        updateNavForAuth(false, "");
        loadPageInIframe("home");
        window.location.hash = "#home";
        return;
    }
    if (message.startsWith("LOG")) {
        console.log("Relayed Message:" + message.substring(3).trim());
        return;
    }

    window.location.hash = "#" + message;
});

function hideNav() {
    document.querySelector(".toggle-menu").checked = false;
}

document.querySelectorAll(".pages").forEach((page) => {
    page.addEventListener("click", () => {
        const pageid = page.id === "homeIcon" ? "home" : page.id;
        const contentElement = document.getElementById("content");

        if (page.textContent.trim().startsWith("Hi!")) {
            contentElement.src = "my.aspx";
            window.location.hash = "#my";
            hideNav();
            return;
        }

        if (page.textContent.trim() === "Account") {
            contentElement.src = "my.aspx#account";
            window.location.hash = "#my";
            hideNav();
            return;
        }

        const safeReturnPage = normalizePage(currentHashPage());
        if (pageid === "login" || pageid === "signup") {
            contentElement.src = pageid + ".aspx?return=" + encodeURIComponent(safeReturnPage);
        } else {
            contentElement.src = pageid + ".aspx";
        }

        hideNav();
        if (page.textContent.trim() === "Sign Out") {
            contentElement.src = "signout.aspx";
            window.location.hash = "#signout";
        }
    });
});

const handleResize = () => {
    const contentElement = document.getElementById("content");
    const bodyHeight = window.innerHeight;
    const navHeight = document.querySelector(".navbar").offsetHeight;
    contentElement.style.height = bodyHeight - navHeight - 15 + "px";
    const bodyWidth = window.innerWidth;
    const navWidth = document.querySelector(".navbar").offsetWidth;
    contentElement.style.width = bodyWidth - navWidth - 15 + "px";
};
window.addEventListener("resize", handleResize);
document.addEventListener("mouseenter", handleResize);
document.addEventListener("focus", handleResize);
window.addEventListener("load", handleResize);
document.addEventListener("fullscreenchange", handleResize);
document.addEventListener("webkitfullscreenchange", handleResize);

if (window.location.hash.slice(1) !== "") {
    if (window.location.hash.slice(1).trim().startsWith("edit")) {
        document.getElementById("content").src = "editriddle.aspx?id=" + window.location.hash.slice(1).trim().substring(4);
    }
    else {
        document.getElementById("content").src = window.location.hash.slice(1) + ".aspx";
    }
} else {
    document.getElementById("content").src = "home.aspx";
}
