var sendCommandTimeout = 100;
var sendCommandRequestTimeout = 1000;

var up = false;
var fullLeft = false;
var fullRight = false;
var down = false;
var forward = false;
var backward = false;

//# Control car with touches ##

var upElement = document.getElementById("up");
var fullLeftElement = document.getElementById("left");
var fullRightElement = document.getElementById("right");
var downElement = document.getElementById("down");
var forwardElement = document.getElementById("forward");
var backwardElement = document.getElementById("backward");

function processTouches(touches) {
    var touchesElements = new Array();

    for (var touchId = 0; touchId < touches.length; touchId++) {

        var touchLocation = touches.item(touchId);
        var element = document.elementFromPoint(touchLocation.clientX, touchLocation.clientY);

        if (element !== undefined && element.id !== undefined) {

            if (element.id === upElement.id) {

                touchesElements.push(upElement.id);
                upElement.classList.add("up-arrow-hover");
                up = true;
            }

            if (element.id === fullLeftElement.id) {

                touchesElements.push(fullLeftElement.id);
                fullLeftElement.classList.add("left-arrow-hover");
                fullLeft = true;
            }

            if (element.id === fullRightElement.id) {

                touchesElements.push(fullRightElement.id);
                fullRightElement.classList.add("right-arrow-hover");
                fullRight = true;
            }

            if (element.id === downElement.id) {

                touchesElements.push(downElement.id);
                downElement.classList.add("down-arrow-hover");
                down = true;
            }

            if (element.id === forwardElement.id) {

                touchesElements.push(forwardElement.id);
                forwardElement.classList.add("up-arrow-hover");
                forward = true;

            }

            if (element.id === backwardElement.id) {

                touchesElements.push(backwardElement.id);
                backwardElement.classList.add("down-arrow-hover");
                backward = true;
            }
        }
    }

    if (!arrayContains("up", touchesElements)) {
        upElement.classList.remove("up-arrow-hover");
        up = false;
    }

    if (!arrayContains("left", touchesElements)) {
        fullLeftElement.classList.remove("left-arrow-hover");
        fullLeft = false;
    }

    if (!arrayContains("right", touchesElements)) {
        fullRightElement.classList.remove("right-arrow-hover");
        fullRight = false;
    }

    if (!arrayContains("down", touchesElements)) {
        downElement.classList.remove("down-arrow-hover");
        down = false;
    }

    if (!arrayContains("forward", touchesElements)) {
        forwardElement.classList.remove("up-arrow-hover");
        forward = false;
    }

    if (!arrayContains("backward", touchesElements)) {
        backwardElement.classList.remove("down-arrow-hover");
        backward = false;
    }
}

function removeTouches(touches) {
    for (var touchId = 0; touchId < touches.length; touchId++) {
        var touchLocation = touches.item(touchId);
        var element = document.elementFromPoint(touchLocation.clientX, touchLocation.clientY);

        if (element.id === upElement.id) {
            upElement.classList.remove("up-arrow-hover");
            up = false;
        }

        if (element.id === fullLeftElement.id) {
            fullLeftElement.classList.remove("left-arrow-hover");
            fullLeft = false;
        }

        if (element.id === fullRightElement.id) {
            fullRightElement.classList.remove("right-arrow-hover");
            fullRight = false;
        }

        if (element.id === downElement.id) {
            downElement.classList.remove("down-arrow-hover");
            down = false;
        }

        if (element.id === forwardElement.id) {
            forwardElement.classList.remove("up-arrow-hover");
            forward = false;
        }

        if (element.id === backwardElement.id) {
            backwardElement.classList.remove("down-arrow-hover");
            backward = false;
        }
    }
}

function arrayContains(value, array) {
    return (array.indexOf(value) > -1);
}

//## Send commands to server ##

var isLastCommandSendNecessary = false;

function IsCommandSendNecessary(motorServoCommand) {
    if (!motorServoCommand.up
        && !motorServoCommand.fullLeft
        && !motorServoCommand.fullRight
        && !motorServoCommand.down
        && !motorServoCommand.speed) {
        return false;
    } else {
        return true;
    }
}

function SendCommandAfterTimeout() {
    setTimeout(function () { SendCommand(); }, sendCommandTimeout);
}

function SendCommand() {

    var command = {
        up: up,
        fullLeft: fullLeft,
        fullRight: fullRight,
        down: down,
        forward: forward,
        backward: backward,
        speed: forward || backward || fullLeft || fullRight
    };

    var isCommandSendNecessary = IsCommandSendNecessary(command);

    if (!isCommandSendNecessary && !isLastCommandSendNecessary) {
        SendCommandAfterTimeout();
    } else {

        if (!isCommandSendNecessary) {
            isLastCommandSendNecessary = false;
        } else {
            isLastCommandSendNecessary = true;
        }

        var request = new XMLHttpRequest();

        var command = "<JsonObject>" + JSON.stringify(command) + "</JsonObject>";

        request.open("GET", "MotorServoCommand" + new Date().getTime() + ".html?motorServoCommand=" + command, true);

        request.timeout = sendCommandRequestTimeout;

        request.ontimeout = function () {
            SendCommandAfterTimeout();
        }

        request.onerror = function () {
            SendCommandAfterTimeout();
        }

        request.onload = function () {
            SendCommandAfterTimeout();
        }

        request.send();
    }
}

SendCommand();

//## Stop car if window is not active ##

window.onblur = function () {
    isLastCommandSendNecessary = false;
    up = false;
    fullLeft = false;
    fullRight = false;
    down = false;
    forward = false;
    backward = false;
};

function preventDefaults(event) {
    event = event || window.event;
    var target = event.target || event.srcElement;
    if (!target.className.match(/\baltNav\b/)) {
        event.returnValue = false;
        event.cancelBubble = true;
        if (event.preventDefault) {
            event.preventDefault();
        }
        return false;
    }
}

//## Events ##

document.body.addEventListener('touchmove', function (event) {

    processTouches(event.touches);

    preventDefaults(event);

}, true);

document.body.addEventListener('touchstart', function (event) {

    processTouches(event.touches);

    preventDefaults(event);

}, true);

document.addEventListener('touchend', function (e) {
    removeTouches(e.changedTouches);
    preventDefaults(e);
}, false);

document.addEventListener('touchcancel', function (e) {
    removeTouches(e.changedTouches);
    preventDefaults(e);
}, false);