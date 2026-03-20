/* wwwroot/js/admin-logic.js */

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
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('expanded');
            const currentState = sidebar.classList.contains('collapsed') ? 'collapsed' : 'expanded';
            localStorage.setItem('sidebar-state', currentState);
        });
    }


    // --- DOUGHNUT CHART (Data Split) ---
    const pieCanvas = document.getElementById('distPieChart');
    if (pieCanvas) {
        new Chart(pieCanvas.getContext('2d'), {
            type: 'doughnut', // Mula 'pie', ginawa nating 'doughnut'
            data: {
                labels: ['Workouts', 'Foods', 'Users'],
                datasets: [{
                    data: [workoutCount, foodCount, userCount],
                    backgroundColor: ['#10b981', '#fbbf24', '#3b82f6'],
                    borderWidth: 0,
                    hoverOffset: 20,
                    borderRadius: 8 // Ginagawa nitong rounded ang kanto ng bawat slice
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '75%', // Ito ang nagpapanipis sa ring (Modern Look)
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            color: '#94a3b8',
                            usePointStyle: true, // Ginagawang bilog ang icons sa legend
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

    // Growth Chart (Line Chart) Initialization
    if (growthCanvas) {
        const ctxL = growthCanvas.getContext('2d');
        new Chart(ctxL, {
            type: 'line',
            data: {
                labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
                datasets: [{
                    label: 'Activity',
                    data: [12, 19, 3, 5, 2, 3, 15], // Dummy data muna
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

    // --- 3. FIREBASE & MODAL LOGIC ---
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
    const filter = input.value.toUpperCase();
    const table = document.getElementById(tableId);
    const tr = table.getElementsByTagName("tr");

    for (let i = 1; i < tr.length; i++) {
        let textContent = tr[i].textContent || tr[i].innerText;
        tr[i].style.display = textContent.toUpperCase().indexOf(filter) > -1 ? "" : "none";
    }
}