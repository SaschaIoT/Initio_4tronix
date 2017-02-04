var up = false;
var fullLeft = false;
var fullRight = false;
var down = false;
var forward = false;
var backward = false;
var keyLeftRightActive = false;

var leftKeyDown = false;
var rightKeyDown = false;

var sendCommandTimeout = 100;
var sendCommandRequestTimeout = 1000;

//## Pointerlock (hide mouse and get mouse movements) ##

function isPointerLock() {

    var isPointerLock = document.pointerLockElement === document.body ||
                        document.mozPointerLockElement === document.body ||
                        document.webkitPointerLockElement === document.body;

    return isPointerLock;
}

function PointerLockChanged() {
    if (isPointerLock() === false) {
        Stop();
    }
}

document.addEventListener('pointerlockchange', PointerLockChanged, false);
document.addEventListener('mozpointerlockchange', PointerLockChanged, false);
document.addEventListener('webkitpointerlockchange', PointerLockChanged, false);

document.body.addEventListener("click", function (e) {

    if (isPointerLock() === false) {

        document.body.requestPointerLock = document.body.requestPointerLock ||
                document.body.mozRequestPointerLock ||
                document.body.webkitRequestPointerLock;

        document.body.requestPointerLock();
    }

}, false);

//## Control car with keyboard ##

document.body.onkeydown = function (event) {

    if (isPointerLock() === true) {

        event = event || window.event;
        var keycode = event.charCode || event.keyCode;

        //Key W down
        if (keycode === 87) {
            forward = true;
        //Key A down
        } else if (keycode === 65) {
            leftKeyDown = true;
            keyLeftRightActive = true;
            fullLeft = true;
        //Key S down
        } else if (keycode === 83) {
            backward = true;
        //Key D down
        } else if (keycode === 68) {
            rightKeyDown = true;
            keyLeftRightActive = true;
            fullRight = true;
        }
    }
}

document.body.onkeyup = function (event) {

    if (isPointerLock() === true) {

        event = event || window.event;
        var keycode = event.charCode || event.keyCode;

        //Key W up
        if (keycode === 87) {
            forward = false;
        //Key A up
        } else if (keycode === 65) {
            leftKeyDown = false;
            fullLeft = false;
            keyLeftRightActive = false;
        //Key S up
        } else if (keycode === 83) {
            backward = false;
        //Key D up
        } else if (keycode === 68) {
            rightKeyDown = false;
            fullRight = false;
            keyLeftRightActive = false;
        }
    }
}

// ## Control car with mouse ##

var mouseSensitivity = 80;
var mouseXPositive;
var mouseXNegative;
var mouseYNegative;
var mouseYPositive;
var mouseLastX = 0;
var mouseLastY = 0;
var mouseTimeout = 200;
var mouseXY = { x: 0, y: 0 };

document.body.addEventListener('mousemove', function (e) {

    if (isPointerLock() === true) {

        var movementX = e.movementX || e.webkitMovementX;
        var movementY = e.movementY || e.webkitMovementY;

        if (movementX === undefined) {
            movementX = 0;
        }

        if (movementY === undefined) {
            movementY = 0;
        }

        mouseXY.x = mouseXY.x + movementX;
        mouseXY.y = mouseXY.y + movementY;

        if (mouseXY.x >= (mouseLastX + mouseSensitivity)
            && keyLeftRightActive === false) {

            fullRight = true;

            if (mouseXPositive !== undefined) {
                window.clearTimeout(mouseXPositive);
            }

            mouseXY.x = 0;
            mouseLastX = 0;

            mouseXPositive = window.setTimeout(function () {
                if (rightKeyDown === false) {
                    fullRight = false;
                }
            }, mouseTimeout);

        }

        if (mouseXY.x <= (mouseLastX - mouseSensitivity)
            && keyLeftRightActive === false) {

            fullLeft = true;

            if (mouseXNegative !== undefined) {
                window.clearTimeout(mouseXNegative);
            }

            mouseXY.x = 0;
            mouseLastX = 0;

            mouseXNegative = window.setTimeout(function () {
                if (leftKeyDown === false) {
                    fullLeft = false;
                }
            }, mouseTimeout);
        }

        if (mouseXY.y <= (mouseLastY - mouseSensitivity)) {
            up = true;

            if (mouseYPositive !== undefined) {
                window.clearTimeout(mouseYPositive);
            }

            mouseLastY = mouseXY.y;

            mouseXY.y = 0;
            mouseLastY = 0;

            mouseYPositive = window.setTimeout(function () {
                up = false;
            }, mouseTimeout);

        }

        if (mouseXY.y >= (mouseLastY + mouseSensitivity)) {
            down = true;

            if (mouseYNegative !== undefined) {
                window.clearTimeout(mouseYNegative);
            }

            mouseLastY = 0;
            mouseXY.y = 0;

            mouseYNegative = window.setTimeout(function () {
                down = false;
            }, mouseTimeout);
        }
    }
}, false);

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

    var motorServoCommand = {
        up: up,
        down: down,
        fullLeft: fullLeft,
        fullRight: fullRight,
        forward: forward,
        backward: backward,
        speed: forward || backward || fullLeft || fullRight
    };

    var isCommandSendNecessary = IsCommandSendNecessary(motorServoCommand);

    if (!isCommandSendNecessary && !isLastCommandSendNecessary) {
        SendCommandAfterTimeout();
    } else {

        if (!isCommandSendNecessary) {
            isLastCommandSendNecessary = false;
        } else {
            isLastCommandSendNecessary = true;
        }

        var request = new XMLHttpRequest();

        var motorServoCommand = "<JsonObject>" + JSON.stringify(motorServoCommand) + "</JsonObject>";

        request.open("GET", "MotorServoCommand" + new Date().getTime() + ".html?motorServoCommand=" + motorServoCommand, true);

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

function Stop() {
    isLastCommandSendNecessary = true;
    up = false;
    fullLeft = false;
    fullRight = false;
    down = false;
    forward = false;
    backward = false;
    leftKeyDown = false;
    rightKeyDown = false;
}

window.onblur = function () {
    Stop();
};