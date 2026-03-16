// firebase-config.js
const firebaseConfig = {
    apiKey: "AIzaSyByu1m2VjIxdXAhfX7Jk49sFjeKvcqT8Ww",
    authDomain: "fir-app1-eedca.firebaseapp.com",
    databaseURL: "https://fir-app1-eedca-default-rtdb.asia-southeast1.firebasedatabase.app",
    projectId: "fir-app1-eedca",
    storageBucket: "fir-app1-eedca.firebasestorage.app",
    messagingSenderId: "775683906550",
    appId: "1:775683906550:web:05fb2d8ccc6923db1ac5d7",
    measurementId: "G-NWJQKVZR8S"
};

// Initialize Firebase once globally
if (!firebase.apps.length) {
    firebase.initializeApp(firebaseConfig);
}