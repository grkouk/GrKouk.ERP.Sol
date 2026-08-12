// WebAuthn (passkey) browser flows for ASP.NET Core Identity.
//
// Uses the JSON-based WebAuthn APIs (parseCreationOptionsFromJSON / parseRequestOptionsFromJSON / toJSON),
// which is the shape ASP.NET Core Identity's passkey APIs expect on the wire.
//
// Note: all of this requires a secure context (HTTPS, or localhost). It will not run over plain HTTP,
// and the Relying Party ID must be a domain name - a bare IP address is not a valid RP ID.
(function () {
    "use strict";

    var endpoints = {
        creationOptions: "/api/passkey/creation-options",
        register: "/api/passkey/register",
        requestOptions: "/api/passkey/request-options",
        signIn: "/api/passkey/signin"
    };

    function isSupported() {
        return typeof window.PublicKeyCredential !== "undefined" &&
            typeof window.PublicKeyCredential.parseCreationOptionsFromJSON === "function" &&
            typeof window.PublicKeyCredential.parseRequestOptionsFromJSON === "function";
    }

    function antiforgeryToken() {
        var field = document.querySelector('input[name="__RequestVerificationToken"]');
        return field ? field.value : "";
    }

    async function postJson(url, body) {
        var response = await fetch(url, {
            method: "POST",
            credentials: "same-origin",
            headers: {
                "Content-Type": "application/json",
                "X-Requested-With": "XMLHttpRequest",
                "RequestVerificationToken": antiforgeryToken()
            },
            body: JSON.stringify(body || {})
        });

        if (response.ok) {
            return response;
        }

        var message = "Request failed (" + response.status + ").";
        try {
            var problem = await response.json();
            if (problem && problem.error) {
                message = problem.error;
            }
        } catch (e) {
            // Response had no JSON body; keep the status-based message.
        }
        throw new Error(message);
    }

    // Registration: create a new credential and hand it to the server.
    async function register(displayName) {
        if (!isSupported()) {
            throw new Error("This browser does not support passkeys.");
        }

        var optionsResponse = await postJson(endpoints.creationOptions, {});
        var optionsJson = await optionsResponse.json();
        var options = window.PublicKeyCredential.parseCreationOptionsFromJSON(optionsJson);

        var credential = await navigator.credentials.create({ publicKey: options });
        if (!credential) {
            throw new Error("No passkey was created.");
        }

        await postJson(endpoints.register, {
            credentialJson: JSON.stringify(credential.toJSON()),
            name: displayName || "Passkey"
        });
    }

    // Sign in: prove possession of an existing credential.
    async function signIn() {
        if (!isSupported()) {
            throw new Error("This browser does not support passkeys.");
        }

        var optionsResponse = await postJson(endpoints.requestOptions, {});
        var optionsJson = await optionsResponse.json();
        var options = window.PublicKeyCredential.parseRequestOptionsFromJSON(optionsJson);

        var credential = await navigator.credentials.get({ publicKey: options });
        if (!credential) {
            throw new Error("No passkey was selected.");
        }

        await postJson(endpoints.signIn, {
            credentialJson: JSON.stringify(credential.toJSON()),
            name: null
        });
    }

    function showError(container, error) {
        if (!container) {
            return;
        }
        // AbortError / NotAllowedError just mean the user dismissed the browser prompt.
        if (error && (error.name === "NotAllowedError" || error.name === "AbortError")) {
            container.textContent = "";
            return;
        }
        container.textContent = error && error.message ? error.message : "Something went wrong.";
    }

    function wireUp() {
        if (!isSupported()) {
            // Hide passkey affordances entirely rather than offering something that cannot work.
            Array.prototype.forEach.call(document.querySelectorAll("[data-passkey-requires-support]"), function (el) {
                el.style.display = "none";
            });
            return;
        }

        var signInButton = document.getElementById("passkey-signin");
        if (signInButton) {
            signInButton.addEventListener("click", async function () {
                var errorBox = document.getElementById("passkey-error");
                showError(errorBox, null);
                signInButton.disabled = true;
                try {
                    await signIn();
                    window.location.href = signInButton.dataset.returnUrl || "/";
                } catch (error) {
                    showError(errorBox, error);
                } finally {
                    signInButton.disabled = false;
                }
            });
        }

        var registerButton = document.getElementById("passkey-register");
        if (registerButton) {
            registerButton.addEventListener("click", async function () {
                var errorBox = document.getElementById("passkey-error");
                var nameInput = document.getElementById("passkey-name");
                showError(errorBox, null);
                registerButton.disabled = true;
                try {
                    await register(nameInput ? nameInput.value : "Passkey");
                    window.location.reload();
                } catch (error) {
                    showError(errorBox, error);
                } finally {
                    registerButton.disabled = false;
                }
            });
        }
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", wireUp);
    } else {
        wireUp();
    }
})();
