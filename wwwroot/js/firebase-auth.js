// wwwroot/js/firebase-auth.js

// ============================================
// FIREBASE LOGIN FUNCTION (for Blazor interop)
// ============================================
window.firebaseLogin = async function() {
    try {
        const provider = new firebase.auth.GoogleAuthProvider();
        provider.setCustomParameters({ prompt: 'select_account' });

        const result = await firebase.auth().signInWithPopup(provider);
        const idToken = await result.user.getIdToken();

        return idToken;
    } catch (error) {
        console.error('Firebase login error:', error);
        return null;
    }
};

// ============================================
// LEGACY: For traditional form submission (backup)
// ============================================
document.addEventListener('DOMContentLoaded', function() {
    const googleBtn = document.getElementById('firebaseLoginBtn');
    if (googleBtn) {
        googleBtn.addEventListener('click', async function() {
            const token = await window.firebaseLogin();
            if (token) {
                const tokenInput = document.getElementById('firebaseTokenInput');
                const loginForm = document.getElementById('loginForm');
                if (tokenInput && loginForm) {
                    tokenInput.value = token;
                    loginForm.submit();
                }
            }
        });
    }
});