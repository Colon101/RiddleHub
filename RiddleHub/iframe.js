// Inside the iframe document
window.addEventListener("click", () => {
  parent.postMessage("clickedInsideIframe", "*");
});
