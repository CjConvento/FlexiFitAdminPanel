// Example for the Pie Chart
const distCtx = document.getElementById('distPieChart').getContext('2d');
new Chart(distCtx, {
    type: 'doughnut',
    data: {
        labels: ['Users', 'Workouts', 'Foods'],
        datasets: [{
            data: [totalUsers, totalWorkouts, totalFoods],
            backgroundColor: ['#3b82f6', '#10b981', '#f59e0b'],
            hoverOffset: 10,
            borderWidth: 0,
            borderRadius: 5
        }]
    },
    options: {
        maintainAspectRatio: false,
        cutout: '80%',
        plugins: {
            legend: {
                position: 'bottom',
                labels: { color: '#94a3b8', usePointStyle: true, padding: 20 }
            }
        }
    }
});