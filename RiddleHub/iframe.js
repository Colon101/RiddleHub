// Inside the iframe document
window.addEventListener("click", () => {
  parent.postMessage("clickedInsideIframe", "*");
});
function relayMessage(message) {
    parent.postMessage(message , "*");
}

