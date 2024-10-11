window.addEventListener("click", (event) => {
    const navDiv = document.querySelector(".navigation");
    if (!navDiv.contains(event.target)) {
        document.querySelector(".toggle-menu").checked = false;
    }
});
window.addEventListener("message", (event) => {
    if (typeof event.data != "string") {
        return;
    }
    if (event.data === "clickedInsideIframe") {
        hideNav();
    } else if (event.data === "reload") {
        window.location.hash = "#my";
        window.location.reload();
    } else if (event.data === "reloadout") {
        window.location.hash = "";
        window.location.reload();
    } else if (event.data.trim().startsWith("USER")) {
        let user = event.data.trim().substring(4).trim();
        document.getElementById("login").textContent = "Hi! " + user;
        document.getElementById("signup").textContent = "Sign Out";
    } else if (event.data.trim() == "SIGNOUT") {
        document.getElementById("login").textContent = "Login";
        document.getElementById("signup").textContent = "Sign Up";
        const contentElement = document.getElementById("content");

        contentElement.src = "home.aspx";
    } else if (event.data.trim().startsWith("LOG")) {
        let msg = event.data.trim().substring(3).trim();
        console.log("Relayed Message:" + msg);
    } else {
        if (typeof event.data == "string") {
            window.location.hash = "#" + event.data;
        }
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
        if (page.textContent.trim() === "Sign Out") {
            contentElement.src = "signout.aspx";
            window.location.hash = "#signout";
        }
        if (page.textContent.trim().startsWith("Hi!")) {
            contentElement.src = "my.aspx";
            window.location.hash = "#my";
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

// Handleing the hashlocation to run the iframe
if (window.location.hash.slice(1) !== "") {
    if (window.location.hash.slice(1).trim().startsWith("edit")) {
        document.getElementById("content").src = "editriddle.aspx?id=" + window.location.hash.slice(1).trim().substring(4);
    }
    else {
        document.getElementById("content").src =
            window.location.hash.slice(1) + ".aspx";
    }
} else {
    document.getElementById("content").src = "home.aspx";
}

