window.authStorage = {
    setToken: function (token) {
        localStorage.setItem("jwt_token", token);
    },
    getToken: function () {
        return localStorage.getItem("jwt_token");
    },
    removeToken: function () {
        localStorage.removeItem("jwt_token");
    }
};