var shutdownButton = document.getElementById("shutdownButton");
shutdownButton.addEventListener("touchstart", function (e) {

    if (confirm('Do you really want to turn me off?')) {
        var xhr = new XMLHttpRequest();
        xhr.open("GET", "shutdown?time=" + new Date().getTime(), true);
        xhr.responseType = "json";
        xhr.timeout = 1000;
        xhr.send();

        shutdownButton.classList.add("shutdown-active-button");
    }

    e.preventDefault();
    e.stopPropagation();
});