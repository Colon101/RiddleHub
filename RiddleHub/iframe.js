// Inside the iframe document
window.addEventListener("click", () => {
  parent.postMessage("clickedInsideIframe", "*");
});
function relayMessage(message) {
  parent.postMessage(message, "*");
}
function topLog(message) {
  relayMessage("LOG" + message);
}
function whatsMyUrl() {
  return window.location.href.split("/")[3].split(".")[0];
}
function isInIframe() {
  return window.self !== window.top;
}
function relaySessionBridge() {
  const bridge = document.getElementById("sessionBridge");
  if (!bridge) {
    return false;
  }
  const payload = {
    loggedIn: bridge.dataset.loggedin === "true",
    username: bridge.dataset.username || ""
  };
  relayMessage("AUTHSTATE" + JSON.stringify(payload));
  if (payload.loggedIn && payload.username) {
    relayMessage("USER" + payload.username);
  }
  return true;
}
function relayHiddenUsername() {
  const usernameElement = document.getElementById("usernametxt");
  if (!usernameElement) {
    return false;
  }
  const username = (usernameElement.textContent || "").trim();
  if (username.length > 0) {
    relayMessage("USER" + username);
    relayMessage("AUTHSTATE" + JSON.stringify({ loggedIn: true, username }));
  } else {
    relayMessage("AUTHSTATE" + JSON.stringify({ loggedIn: false, username: "" }));
  }
  return true;
}
function relayAuthState() {
  if (relayHiddenUsername()) {
    return;
  }
  relaySessionBridge();
}
if (!isInIframe()) {
  window.location.assign("/#" + whatsMyUrl());
} else {
  window.addEventListener("DOMContentLoaded", relayAuthState);
}
