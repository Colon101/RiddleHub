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
    return;
  }
  const payload = {
    loggedIn: bridge.dataset.loggedin === "true",
    username: bridge.dataset.username || ""
  };
  relayMessage("AUTHSTATE" + JSON.stringify(payload));
}
if (!isInIframe()) {
  window.location.assign("/#" + whatsMyUrl());
} else {
  window.addEventListener("DOMContentLoaded", relaySessionBridge);
}
