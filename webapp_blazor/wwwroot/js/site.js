function renderEditableProgramChart(status, index) {
    var ctx = document.getElementById('programChart_' + index).getContext('2d');
    var dayData = status.Program[index];
    var times = Object.keys(dayData);
    var setpoints = Object.values(dayData);

    var chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: times,
            datasets: [{
                label: 'Setpoint',
                data: setpoints,
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1,
                fill: false
            }]
        },
        options: {
            scales: {
                x: {
                    type: 'category',
                    labels: times
                },
                y: {
                    beginAtZero: true
                }
            },
            plugins: {
                annotation: {
                    annotations: {
                        line1: {
                            type: 'line',
                            yMin: 0,
                            yMax: 0,
                            borderColor: 'red',
                            borderWidth: 2,
                            label: {
                                content: 'Drag me',
                                enabled: true,
                                position: 'center'
                            },
                            draggable: true,
                            onDrag: function(event) {
                                var newValue = event.y;
                                var index = event.element.index;
                                chart.data.datasets[0].data[index] = newValue;
                                chart.update();
                            }
                        }
                    }
                }
            }
        }
    });
}
