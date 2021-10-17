const client = stitch.Stitch.initializeDefaultAppClient("digisign_realm-zxtsu");

if (client.auth.hasRedirectResult()) {
    client.auth.handleRedirectResult().then(user => {
        console.log(user);
    });
}

var errCt = 0;

window.login = async function(un, pw){
    try {
        if (!client.auth.isLoggedIn) {
            const credential = new stitch.UserPasswordCredential(un, pw);
            var authedUser = await client.auth.loginWithCredential(credential);
            if(authedUser) {
                console.log(`successfully logged in with id: ${authedUser.id}`);
                return true;
            }
            else {
                console.log(`login failed`);
                return false;
            }
        } else {
            console.log(client.auth.currentUser);
            return true;
        }
        
    }
    catch(e) {
        return false;
    }
}

window.goodLogin = function(authedUser) {
    console.log(`successfully logged in with id: ${authedUser.id}`);
    return true;
}

window.loginFail = function(err) {
    console.error(`login failed with error: ${err}`);
    return false;
}

window.getUserDetails = function() {
    var r = {};
    r.email = client.auth.currentUser.profile.email;
    r.id = client.auth.currentUser.id;
    console.log(r);
    return r;
}

window.realmShimObj_Function = async function(fnname, args) {
    console.log(fnname);
    console.log(args);
    var a = [args];
    var result = await client.callFunction(fnname,a);
    console.log(result);
    return result;
}

window.realmShim_Function = async function(fnname, args) {
    console.log(fnname);
    console.log(args);
    var a = args;
    if(!Array.isArray(a)) {
        a = [args];
    } 
    var result = await client.callFunction(fnname,a);
    console.log(result);
    return result;
}

window.realmShim_SetCustomUserData = function(key, value) {
    client.auth.user.customData[key] = value;
}

window.logout = function() {
    client.auth.isLoggedIn = false;
    client.auth.logout().then(() => {
        window.location = "/";
    });
}
