/* wwwroot/js/admin-logic.js */

// Global variable para sa Chart instances
let doughnutChart = null;
let growthChart = null;

document.addEventListener('DOMContentLoaded', function () {

    // --- 1. SIDEBAR LOGIC ---
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
    if (savedState) applySidebarState(savedState);

    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('expanded');
            const currentState = sidebar.classList.contains('collapsed') ? 'collapsed' : 'expanded';
            localStorage.setItem('sidebar-state', currentState);
        });
    }

    // --- 2. DOUGHNUT CHART (Data Split) ---
    const pieCanvas = document.getElementById('distPieChart');
    if (pieCanvas) {
        // Check if chart already exists, destroy it first
        if (doughnutChart) {
            doughnutChart.destroy();
        }

        // Check if required variables are defined (from Dashboard)
        if (typeof workoutCount !== 'undefined' && typeof foodCount !== 'undefined' && typeof userCount !== 'undefined') {
            doughnutChart = new Chart(pieCanvas.getContext('2d'), {
                type: 'doughnut',
                data: {
                    labels: ['Workouts', 'Foods', 'Users'],
                    datasets: [{
                        data: [workoutCount, foodCount, userCount],
                        backgroundColor: ['#10b981', '#fbbf24', '#3b82f6'],
                        borderWidth: 0,
                        hoverOffset: 20,
                        borderRadius: 8
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    cutout: '75%',
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: {
                                color: '#94a3b8',
                                usePointStyle: true,
                                padding: 25,
                                font: { size: 12 }
                            }
                        },
                        tooltip: {
                            backgroundColor: '#1e293b',
                            padding: 12,
                            bodyColor: '#f8fafc',
                            callbacks: {
                                label: function (context) {
                                    let sum = 0;
                                    context.dataset.data.map(data => { sum += data; });
                                    let percentage = ((context.raw * 100) / sum).toFixed(1) + "%";
                                    return ` ${context.label}: ${context.raw} (${percentage})`;
                                }
                            }
                        }
                    }
                }
            });
        }
    }

    // --- 3. GROWTH CHART (Line Chart) ---
    const growthCanvas = document.getElementById('growthChart');
    if (growthCanvas) {
        // Destroy existing chart if any
        if (growthChart) {
            growthChart.destroy();
        }

        const ctxL = growthCanvas.getContext('2d');
        growthChart = new Chart(ctxL, {
            type: 'line',
            data: {
                labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
                datasets: [{
                    label: 'Activity',
                    data: [12, 19, 3, 5, 2, 3, 15],
                    borderColor: '#3b82f6',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    y: { grid: { color: '#334155' }, ticks: { color: '#94a3b8' } },
                    x: { grid: { display: false }, ticks: { color: '#94a3b8' } }
                }
            }
        });
    }

    // --- 4. FIREBASE & MODAL LOGIC ---
    const userModal = document.getElementById('addUserModal');
    if (userModal) {
        userModal.addEventListener('hidden.bs.modal', function () {
            const form = document.getElementById('addUserForm');
            if (form) form.reset();
        });
    }
});

/**
 * GLOBAL HELPER FUNCTIONS
 */
function filterByStatus(status) {
    const rows = document.querySelectorAll('.food-row, .workout-row, tr[data-status]');
    rows.forEach(row => {
        const rowStatus = row.getAttribute('data-status');
        row.style.display = (status === 'all' || rowStatus === status) ? '' : 'none';
    });
}

function filterTable(inputId, tableId) {
    const input = document.getElementById(inputId);
    if (!input) return;
    const filter = input.value.toUpperCase();
    const table = document.getElementById(tableId);
    if (!table) return;
    const tr = table.getElementsByTagName("tr");

    for (let i = 1; i < tr.length; i++) {
        let textContent = tr[i].textContent || tr[i].innerText;
        tr[i].style.display = textContent.toUpperCase().indexOf(filter) > -1 ? "" : "none";
    }
}

function confirmDelete(itemName) {
    return confirm(`Are you sure you want to delete "${itemName}"? This action cannot be undone.`);
}

function useGoogleProvider() {
    // Siguraduhing available ang Firebase Auth
    if (typeof firebase === 'undefined') {
        console.error('Firebase SDK not loaded');
        alert('Firebase is not initialized. Please check your scripts.');
        return;
    }

    const provider = new firebase.auth.GoogleAuthProvider();
    provider.addScope('email');
    provider.addScope('profile');

    firebase.auth().signInWithPopup(provider)
        .then((result) => {
            const user = result.user;
            const displayName = user.displayName || '';
            const email = user.email || '';
            const uid = user.uid;

            // Punan ang modal form fields (nasa loob ng addUserModal)
            const modalName = document.getElementById('modalName');
            const modalUsername = document.getElementById('modalUsername');
            const modalEmail = document.getElementById('modalEmail');
            const modalUid = document.getElementById('modalUid');
            const modalProvider = document.getElementById('modalProvider');

            if (modalName) modalName.value = displayName;
            if (modalEmail) modalEmail.value = email;
            if (modalUid) modalUid.value = uid;
            if (modalProvider) modalProvider.value = 'GOOGLE';

            // Awtomatikong mag-generate ng username base sa display name o email
            if (modalUsername) {
                if (displayName) {
                    // Hal. "Juan Dela Cruz" -> "juan_dela_cruz"
                    modalUsername.value = displayName.toLowerCase().replace(/\s+/g, '_');
                } else if (email) {
                    modalUsername.value = email.split('@')[0];
                } else {
                    modalUsername.value = 'user_' + uid.slice(0, 8);
                }
            }

            console.log('Google sign-in successful', user);
            // Hindi na kailangang i-show ulit ang modal dahil naka-open na ito
        })
        .catch((error) => {
            console.error('Google sign-in error:', error);
            alert('Failed to sign in with Google: ' + error.message);
        });
}