const PARENT_ORIGIN = window.location.origin;
const THEME_STORAGE_KEY = "riddlehub-theme";
const VALID_PARENT_MESSAGES = new Set([
  "clickedInsideIframe",
  "REQTHEME",
  "reload",
  "reloadout",
  "SIGNOUT",
  "home",
  "create",
  "my",
  "account",
  "login",
  "signup",
  "signout"
]);

function isInIframe() {
  return window.self !== window.top;
}

function safeTheme(themeName) {
  return themeName === "dark" ? "dark" : "light";
}

function applyTheme(themeName) {
  const nextTheme = safeTheme(themeName);
  document.documentElement.setAttribute("data-theme", nextTheme);
  localStorage.setItem(THEME_STORAGE_KEY, nextTheme);
}

function applyStoredTheme() {
  applyTheme(localStorage.getItem(THEME_STORAGE_KEY));
}

function isAllowedOutboundMessage(message) {
  if (VALID_PARENT_MESSAGES.has(message) || /^edit[1-9][0-9]*$/.test(message)) {
    return true;
  }
  if (message.startsWith("USER")) {
    return message.substring(4).length <= 300;
  }
  if (message.startsWith("LOG")) {
    return message.length <= 1000;
  }
  if (message.startsWith("AUTHSTATE")) {
    try {
      const payload = JSON.parse(message.substring("AUTHSTATE".length));
      return payload &&
        typeof payload.loggedIn === "boolean" &&
        typeof payload.username === "string" &&
        payload.username.length <= 300;
    } catch (error) {
      return false;
    }
  }
  return false;
}

function relayMessage(message) {
  if (isInIframe() && typeof message === "string" && isAllowedOutboundMessage(message)) {
    parent.postMessage(message, PARENT_ORIGIN);
  }
}

function topLog(message) {
  relayMessage("LOG" + String(message).slice(0, 997));
}

function currentPageRoute() {
  const pageName = window.location.pathname
    .split("/")
    .pop()
    .replace(/\.aspx$/i, "")
    .toLowerCase();
  if (pageName === "editriddle") {
    const id = new URLSearchParams(window.location.search).get("id") || "";
    return /^[1-9][0-9]*$/.test(id) ? "edit" + id : "my";
  }
  return VALID_PARENT_MESSAGES.has(pageName) ? pageName : "home";
}

function relaySessionBridge() {
  const bridge = document.getElementById("sessionBridge");
  if (!bridge) {
    return false;
  }
  const payload = {
    loggedIn: bridge.dataset.loggedin === "true",
    username: (bridge.dataset.username || "").slice(0, 300)
  };
  relayMessage("AUTHSTATE" + JSON.stringify(payload));
  return true;
}

function relayHiddenUsername() {
  const usernameElement = document.getElementById("usernametxt");
  if (!usernameElement) {
    return false;
  }
  const username = (usernameElement.textContent || "").trim().slice(0, 300);
  relayMessage("AUTHSTATE" + JSON.stringify({
    loggedIn: username.length > 0,
    username: username
  }));
  return true;
}

function relayAuthState() {
  if (!relayHiddenUsername()) {
    relaySessionBridge();
  }
}

window.addEventListener("click", () => {
  relayMessage("clickedInsideIframe");
});

window.addEventListener("message", (event) => {
  if (event.origin !== PARENT_ORIGIN || event.source !== parent || typeof event.data !== "string") {
    return;
  }
  if (event.data === "THEMEdark") {
    applyTheme("dark");
  } else if (event.data === "THEMElight") {
    applyTheme("light");
  }
});

applyStoredTheme();

if (!isInIframe()) {
  window.location.assign("/#" + currentPageRoute());
} else {
  window.addEventListener("DOMContentLoaded", () => {
    applyStoredTheme();
    relayAuthState();
    relayMessage("REQTHEME");
  });
}
