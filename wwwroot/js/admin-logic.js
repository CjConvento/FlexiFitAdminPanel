/* wwwroot/js/admin-logic.js */

document.addEventListener('DOMContentLoaded', function () {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.querySelector('.main-content');

    const applySidebarState = (state) => {
        if (state === 'collapsed') {
            sidebar?.classList.add('collapsed');
            mainContent?.classList.add('expanded');
        } else {
            sidebar?.classList.remove('collapsed');
            mainContent?.classList.remove('expanded');
        }
    };

    const savedState = localStorage.getItem('sidebar-state');
    if (savedState) {
        applySidebarState(savedState);
    }

    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function (e) {
            e.preventDefault();
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('expanded');
            const currentState = sidebar.classList.contains('collapsed') ? 'collapsed' : 'expanded';
            localStorage.setItem('sidebar-state', currentState);
        });
    }
});

/**
 * GLOBAL FUNCTIONS
 */

// 1. Delete Confirmation
function confirmDelete(name) {
    return confirm(`Babala: Sigurado ka bang gusto mong permanenteng burahin si ${name}? Hindi na ito mababawi.`);
}

// 2. Status Filter Logic (Active/Inactive)
// Inilagay natin sa global para matawag ng button onclick
function filterByStatus(status) {
    const rows = document.querySelectorAll('.food-row, .workout-row'); // Support para sa parehong tables

    rows.forEach(row => {
        const rowStatus = row.getAttribute('data-status');

        if (status === 'all') {
            row.style.display = '';
        } else if (rowStatus === status) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}

// 3. Table Search Filter (Pinaganda para flexible)
function filterTable(inputId = "searchInput", rowClass = ".food-row") {
    const input = document.getElementById(inputId);
    const filter = input.value.toUpperCase();
    const rows = document.querySelectorAll(rowClass + ", .workout-row");

    rows.forEach(row => {
        const text = row.textContent || row.innerText;
        if (text.toUpperCase().indexOf(filter) > -1) {
            row.style.display = "";
        } else {
            row.style.display = "none";
        }
    });
}


// 1. Firebase Logic para sa Google Add
const googleProvider = new firebase.auth.GoogleAuthProvider();

document.getElementById('btnGoogleAdd').addEventListener('click', function () {
    firebase.auth().signInWithPopup(googleProvider).then((result) => {
        const user = result.user;

        // AUTO-FILL FORM FIELDS
        // Gamitin ang .value para sa mga input fields
        if (document.getElementById('modalName')) {
            document.getElementById('modalName').value = user.displayName || "";
        }

        if (document.getElementById('modalEmail')) {
            document.getElementById('modalEmail').value = user.email || "";
        }

        if (document.getElementById('modalUsername')) {
            // Inayos ang split: '@' lang dapat, hindi '@@'
            const emailParts = user.email.split('@');
            document.getElementById('modalUsername').value = emailParts[0];
        }

        if (document.getElementById('modalUid')) {
            document.getElementById('modalUid').value = user.uid;
        }

        if (document.getElementById('modalProvider')) {
            document.getElementById('modalProvider').value = 'GOOGLE';
        }

        alert("Google Account Linked! Just choose a role and save.");
    }).catch((error) => {
        console.error("Auth Error:", error);
        alert("Error during Google Sign-in: " + error.message);
    });
});


/**
 * 4. AUTOMATED PROVIDER RESET
 * Nililinis ang form at ibinabalik sa 'EMAIL' ang provider kapag sinara ang modal
 * para hindi aksidenteng maging 'GOOGLE' ang manual input.
 */
document.addEventListener('DOMContentLoaded', function () {
    const userModal = document.getElementById('addUserModal');
    if (userModal) {
        userModal.addEventListener('hidden.bs.modal', function () {
            const form = document.getElementById('addUserForm');
            if (form) {
                form.reset(); // Nililinis lahat ng inputs

                // Ibinabalik sa default 'EMAIL' ang provider
                const providerInput = document.getElementById('modalProvider');
                if (providerInput) {
                    providerInput.value = 'EMAIL';
                }

                // Nililinis ang UID field
                const uidInput = document.getElementById('modalUid');
                if (uidInput) {
                    uidInput.value = '';
                }

                console.log("Form reset: Provider returned to EMAIL.");
            }
        });
    }
});

/**
 * 5. UI NOTIFICATIONS (Toast or Alert)
 * Para sa mas magandang user experience pagkatapos ng Delete o Create
 */
function showAdminNotify(message, type = 'success') {
    // Pwede mong lagyan ng Toast logic dito sa hinaharap
    console.log(`[${type.toUpperCase()}]: ${message}`);
}

/**
 * 6. DYNAMIC FORM VALIDATION
 * Sinisiguro na may laman ang email at username bago i-submit
 */
const addUserForm = document.getElementById('addUserForm');
if (addUserForm) {
    addUserForm.addEventListener('submit', function (e) {
        const email = document.getElementById('modalEmail')?.value;
        const provider = document.getElementById('modalProvider')?.value;

        // Simple check: Kung Google, dapat may UID
        if (provider === 'GOOGLE') {
            const uid = document.getElementById('modalUid')?.value;
            if (!uid) {
                alert("Mali: Walang Google UID na nakuha. Pakipindot ulit ang Google button.");
                e.preventDefault();
                return false;
            }
        }

        showAdminNotify("Processing request...");
    });
}

