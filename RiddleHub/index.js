window.addEventListener("click", (event) => {
    const navDiv = document.querySelector(".navigation");
    if (!navDiv.contains(event.target)) {
        document.querySelector(".toggle-menu").checked = false;
    }
});

let authState = {
    loggedIn: false,
    username: ""
};

const THEME_STORAGE_KEY = "riddlehub-theme";

function normalizedTheme(themeValue) {
    return themeValue === "dark" ? "dark" : "light";
}

function getCurrentTheme() {
    return normalizedTheme(localStorage.getItem(THEME_STORAGE_KEY));
}

function updateThemeToggleLabel(themeName) {
    const themeToggleButton = document.getElementById("themeToggle");
    const moonIcon = document.getElementById("themeIconMoon");
    const sunIcon = document.getElementById("themeIconSun");
    const darkEnabled = themeName === "dark";

    if (themeToggleButton) {
        const switchToLabel = darkEnabled ? "light" : "dark";
        themeToggleButton.title = `Switch to ${switchToLabel} mode`;
        themeToggleButton.setAttribute("aria-label", `Switch to ${switchToLabel} mode`);
    }
    if (moonIcon) {
        moonIcon.style.display = darkEnabled ? "none" : "block";
    }
    if (sunIcon) {
        sunIcon.style.display = darkEnabled ? "block" : "none";
    }
}

function setIframeThemeSynchronously(themeName) {
    const contentElement = document.getElementById("content");
    if (!contentElement) {
        return;
    }

    try {
        if (contentElement.contentDocument && contentElement.contentDocument.documentElement) {
            contentElement.contentDocument.documentElement.setAttribute("data-theme", themeName);
        }
        if (contentElement.contentWindow && contentElement.contentWindow.localStorage) {
            contentElement.contentWindow.localStorage.setItem(THEME_STORAGE_KEY, themeName);
        }
    } catch (error) {
        console.log("Theme sync skipped", error);
    }
}

function pushThemeToIframe(themeName) {
    const contentElement = document.getElementById("content");
    if (contentElement && contentElement.contentWindow) {
        contentElement.contentWindow.postMessage("THEME" + themeName, "*");
    }
}

function applyTheme(themeName) {
    const safeTheme = normalizedTheme(themeName);
    document.documentElement.setAttribute("data-theme", safeTheme);
    localStorage.setItem(THEME_STORAGE_KEY, safeTheme);
    updateThemeToggleLabel(safeTheme);

    setIframeThemeSynchronously(safeTheme);
    pushThemeToIframe(safeTheme);
}

function normalizePage(pageName) {
    const validPages = new Set(["home", "create", "my", "account", "login", "signup", "signout"]);
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
    const loginLink = document.getElementById("nav-login");
    const signupLink = document.getElementById("nav-signup");
    authState.loggedIn = !!isLoggedIn;
    authState.username = username || "";

    if (authState.loggedIn) {
        loginLink.textContent = authState.username;
        signupLink.textContent = "Sign Out";
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
    if (message === "REQTHEME") {
        const currentTheme = getCurrentTheme();
        setIframeThemeSynchronously(currentTheme);
        pushThemeToIframe(currentTheme);
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
    page.addEventListener("click", (event) => {
        event.preventDefault();
        const pageid = page.dataset.page || "home";
        const contentElement = document.getElementById("content");

        if (authState.loggedIn && pageid === "login") {
            contentElement.src = "account.aspx";
            window.location.hash = "#account";
            hideNav();
            return;
        }

        if (authState.loggedIn && pageid === "signup") {
            contentElement.src = "signout.aspx";
            window.location.hash = "#signout";
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
    });
});

const themeToggleButton = document.getElementById("themeToggle");
if (themeToggleButton) {
    themeToggleButton.addEventListener("click", () => {
        const nextTheme = getCurrentTheme() === "dark" ? "light" : "dark";
        applyTheme(nextTheme);
    });
}

document.getElementById("content").addEventListener("load", () => {
    const currentTheme = getCurrentTheme();
    setIframeThemeSynchronously(currentTheme);
    pushThemeToIframe(currentTheme);
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

applyTheme(getCurrentTheme());

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
