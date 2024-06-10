window.addEventListener("click", () => {
  const navDiv = document.querySelector(".navigation");
  if (!navDiv.contains(event.target)) {
    document.querySelector(".toggle-menu").checked = false;
  }
});
window.addEventListener("message", (event) => {
  if (event.data === "clickedInsideIframe") {
    hideNav();
  }
});
function hideNav() {
  document.querySelector(".toggle-menu").checked = false;
}
document.querySelectorAll(".pages").forEach((page) => {
  page.addEventListener("click", () => {
    pageid = page.id === "homeIcon" ? "home" : page.id;
    const contentElement = document.getElementById("content");
    contentElement.src = pageid + ".aspx";
    hideNav();
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
  document.getElementById("content").src =
    window.location.hash.slice(1) + ".aspx";
} else {
  document.getElementById("content").src = "home.aspx";
}
