function log() {
    document.getElementById('results').innerText = '';

    Array.prototype.forEach.call(arguments, function (msg) {
        if (msg instanceof Error) {
            msg = "Error: " + msg.message;
        }
        else if (typeof msg !== 'string') {
            msg = JSON.stringify(msg, null, 2);
        }
        document.getElementById('results').innerHTML += msg + '\r\n';
    });
}

document.getElementById("login").addEventListener("click", login, false);
document.getElementById("api").addEventListener("click", api, false);
document.getElementById("logout").addEventListener("click", logout, false);

var config = {
    authority: "http://localhost:5000",
    client_id: "js",
    redirect_uri: "http://localhost:3000/callback.html",
    response_type: "id_token token",
    scope: "openid profile https://quickstarts/api api1 sysId offline_access",
    post_logout_redirect_uri: "http://localhost:3000/index.html",
    // silent renew will get a new access_token via an iframe 
    // just prior to the old access_token expiring (60 seconds prior)
    automaticSilentRenew: true,
    silent_redirect_uri: window.location.origin + "/silent.html",
    // this will toggle if profile endpoint is used
    loadUserInfo: true,
    accessTokenExpiringNotificationTime: 30,
};
var mgr = new Oidc.UserManager(config);

mgr.getUser().then(function (user) {
    if (user) {
        log("User logged in", user.profile);
    }
    else {
        log("User not logged in");
    }
});

mgr.events.addAccessTokenExpiring(function() {
    console.log("token is Expiring");
});

function login() {
    mgr.signinRedirect();
}

function api() {
    mgr.getUser().then(function (user) {
        var url = "http://localhost:4000/api/values";

        var xhr = new XMLHttpRequest();
        xhr.open("GET", url);
        xhr.onload = function() {
            log(xhr.status, JSON.parse(xhr.responseText));
        };

        xhr.setRequestHeader("Authorization", "Bearer " + user.access_token);
        xhr.send();
    });
}

function logout() {
    mgr.signoutRedirect();
}