window.addEventListener("click", (event) => {
    const navDiv = document.querySelector(".navigation");
    if (navDiv && !navDiv.contains(event.target)) {
        document.querySelector(".toggle-menu").checked = false;
    }
});

let authState = {
    loggedIn: false,
    username: ""
};

const THEME_STORAGE_KEY = "riddlehub-theme";
const VALID_PAGES = new Set(["home", "create", "my", "account", "login", "signup", "signout"]);

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
        contentElement.contentWindow.postMessage("THEME" + themeName, window.location.origin);
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
    const candidate = (pageName || "").trim().toLowerCase();
    return VALID_PAGES.has(candidate) ? candidate : "home";
}

function parseRoute(routeValue) {
    const candidate = (routeValue || "").trim().toLowerCase();
    if (VALID_PAGES.has(candidate)) {
        return { hash: candidate, src: candidate + ".aspx" };
    }

    const editMatch = /^edit([1-9][0-9]*)$/.exec(candidate);
    if (editMatch) {
        return {
            hash: "edit" + editMatch[1],
            src: "editriddle.aspx?id=" + encodeURIComponent(editMatch[1])
        };
    }

    return { hash: "home", src: "home.aspx" };
}

function currentHashPage() {
    const route = parseRoute(window.location.hash.slice(1));
    return route.hash.startsWith("edit") ? "my" : normalizePage(route.hash);
}

function updateNavForAuth(isLoggedIn, username) {
    const loginLink = document.getElementById("nav-login");
    const signupLink = document.getElementById("nav-signup");
    authState.loggedIn = !!isLoggedIn;
    authState.username = typeof username === "string" ? username.slice(0, 300) : "";

    if (authState.loggedIn) {
        loginLink.textContent = authState.username;
        signupLink.textContent = "Sign Out";
    } else {
        loginLink.textContent = "Login";
        signupLink.textContent = "Sign Up";
    }
}

function updateHash(routeHash) {
    const safeHash = parseRoute(routeHash).hash;
    if (window.location.hash.slice(1) !== safeHash) {
        window.location.hash = "#" + safeHash;
    }
}

function loadRoute(routeValue, updateLocationHash) {
    const route = parseRoute(routeValue);
    const contentElement = document.getElementById("content");
    if (contentElement.getAttribute("src") !== route.src) {
        contentElement.src = route.src;
    }
    if (updateLocationHash) {
        updateHash(route.hash);
    }
}

function loadPageInIframe(pageId) {
    loadRoute(normalizePage(pageId), true);
}

window.addEventListener("message", (event) => {
    const contentElement = document.getElementById("content");
    if (event.origin !== window.location.origin ||
        !contentElement ||
        event.source !== contentElement.contentWindow ||
        typeof event.data !== "string") {
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
        updateHash("my");
        window.location.reload();
        return;
    }
    if (message === "reloadout") {
        updateHash("home");
        window.location.reload();
        return;
    }
    if (message.startsWith("AUTHSTATE")) {
        try {
            const payload = JSON.parse(message.substring("AUTHSTATE".length));
            if (payload && typeof payload.loggedIn === "boolean" &&
                (payload.username === undefined || typeof payload.username === "string")) {
                updateNavForAuth(payload.loggedIn, payload.username || "");
            }
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
        return;
    }
    if (message.startsWith("LOG")) {
        console.log("Relayed Message:" + message.substring(3).trim());
        return;
    }

    if (VALID_PAGES.has(message) || /^edit[1-9][0-9]*$/.test(message)) {
        updateHash(message);
    }
});

function hideNav() {
    document.querySelector(".toggle-menu").checked = false;
}

document.querySelectorAll(".pages").forEach((page) => {
    page.addEventListener("click", (event) => {
        event.preventDefault();
        const pageId = normalizePage(page.dataset.page || "home");
        const contentElement = document.getElementById("content");

        if (authState.loggedIn && pageId === "login") {
            loadPageInIframe("account");
            hideNav();
            return;
        }

        if (authState.loggedIn && pageId === "signup") {
            loadPageInIframe("signout");
            hideNav();
            return;
        }

        const safeReturnPage = currentHashPage();
        if (pageId === "login" || pageId === "signup") {
            contentElement.src = pageId + ".aspx?return=" + encodeURIComponent(safeReturnPage);
            updateHash(pageId);
        } else {
            loadPageInIframe(pageId);
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
window.addEventListener("hashchange", () => loadRoute(window.location.hash.slice(1), false));

applyTheme(getCurrentTheme());
loadRoute(window.location.hash.slice(1), false);
