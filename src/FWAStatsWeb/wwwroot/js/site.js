/* eslint-disable */

/**
 * Initialize table sort functionality on all .table-sort tables
 */
function initTableSort() {
    const tables = document.querySelectorAll('.table-sort');
    tables.forEach(table => {
        new Tablesort(table);

        // Prevent click events on table header links from propagating
        table.querySelectorAll('thead tr th a').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                e.stopPropagation();
            });
        });

    });
}

/**
 * Initialize table search functionality
 * @param {string} searchInputId - The search input element id
 * @param {string} tableSelector - CSS selector for the table to search
 * @param {string} searchCellSelector - CSS selector for the cells to search within each row
 */
function initTableSearch(searchInputId = 'search', tableSelector = '#table', searchCellSelector = '.table-search') {

    const searchInput = document.getElementById(searchInputId);

    searchInput.addEventListener('keyup', function () {
        const searchValue = this.value.trim().replace(/ +/g, ' ').toLowerCase();
        const rows = document.querySelectorAll(tableSelector + ' > tbody > tr');

        rows.forEach(function (row) {
            const searchElements = row.querySelectorAll(searchCellSelector);
            let found = false;
            
            searchElements.forEach(function (searchElement) {
                let searchText = searchElement.textContent.replace(/\s+/g, ' ').toLowerCase();
                if (searchText.indexOf(searchValue) !== -1) {
                    found = true;
                }
            });

            if (found) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    });
}

// =============================================
// HOME CONTROLLER FUNCTIONS
// =============================================

/**
 * Initialize Home Index page functionality
 * @param {Object} config - Configuration object with chart data from server
 */
function initHomeIndex(config) {
    if (!config.chartData) return;

    google.charts.load('current', {'packages':['corechart']});
    google.charts.setOnLoadCallback(function() {
        
        // Initialize pie charts for war stats
        if (config.chartData.pieCharts) {
            config.chartData.pieCharts.forEach(function(pieChart) {
                const data = google.visualization.arrayToDataTable([
                    ['Wars', 'Count'],
                    ['Matches', pieChart.allianceMatches],
                    ['Mismatches', pieChart.warMatches]
                ]);

                const options = {
                    pieHole: 0.4,
                    chartArea: { left: 15, top: 15, width: '270', height: '170' }
                };

                new google.visualization.PieChart(document.getElementById(pieChart.chartId)).draw(data, options);
            });
        }

        // Initialize sync history chart
        if (config.chartData.syncHistoryChart) {
            let syncData = [['Date', 'Matches', 'Mismatches', "Didn't start"]];
            syncData = syncData.concat(config.chartData.syncHistoryChart.data);
            
            const data = google.visualization.arrayToDataTable(syncData);

            const options = {
                isStacked: 'relative',
                legend: {position: 'top', maxLines: 3},
                vAxis: {format: 'percent' },
                chartArea: { left: 35, top: 15, width: '250', height: '170' }
            };

            new google.visualization.AreaChart(document.getElementById(config.chartData.syncHistoryChart.chartId)).draw(data, options);
        }

        // Initialize townhall area charts
        if (config.chartData.areaCharts) {
            config.chartData.areaCharts.forEach(function(areaChart) {
                const data = google.visualization.arrayToDataTable(areaChart.data);

                const options = {
                    isStacked: 'absolute',
                    legend: {position: 'top', maxLines: 3},
                    chartArea: { left: 35, top: 15, width: '250', height: '170' },
                    vAxis: { gridlines: { count: areaChart.gridlines }, viewWindow: { min: 0, max: areaChart.teamSize }}
                };

                new google.visualization.AreaChart(document.getElementById(areaChart.chartId)).draw(data, options);
            });
        }

        // Initialize weight histogram charts
        if (config.chartData.histogramCharts) {
            config.chartData.histogramCharts.forEach(function(histogramChart) {
                const data = google.visualization.arrayToDataTable(histogramChart.data);

                const options = {
                    chartArea: { left: 35, top: 15, width: '250', height: '170' }
                };

                new google.visualization.ColumnChart(document.getElementById(histogramChart.chartId)).draw(data, options);
            });
        }
    });
}

// =============================================
// UPDATE CONTROLLER FUNCTIONS
// =============================================

/**
 * Initialize Update Index page functionality
 * @param {Object} config - Configuration object with data from server
 */
function initUpdateIndex(config) {
    let updates_remaining = config.updates_remaining;
    const total_updates = updates_remaining + 1;
    let updates_done = 0;
    const update_threads = 8;
    let update_finished = 0;
    const tasks = config.tasks.slice().reverse(); // copy and reverse

    async function updateTask() {
        if(tasks.length > 0) {
            try {
                const response = await fetch(config.updateTaskUrl + '/' + tasks.pop());
                const data = await response.json();
                updateTaskCallback(data);
            } catch (error) {
                console.error('Error:', error);
            }
        }
        else if(updates_remaining === 0 && update_finished === 0) {
            update_finished = 1;
            document.querySelector(".progress-bar").textContent = "Finishing up";
            try {
                const response = await fetch(config.updateFinishedUrl);
                const data = await response.json();
                updateFinishedCallback(data);
            } catch (error) {
                console.error('Error:', error);
            }
        }
    }

    function updateProgress(message) {
        if (message)
            document.querySelector(".progress-bar").textContent = message;

        const progress = Math.round(++updates_done * 100 / total_updates);
        document.querySelector(".progress-bar").setAttribute("aria-valuenow", progress);
        document.querySelector(".progress-bar").setAttribute("style", "width:" + progress + "%");
    }

    function updateTaskCallback(data) {
        if (data && data.id) {
            if(!data.status) {
                const alertDiv = document.createElement('div');
                alertDiv.className = 'alert alert-danger';
                alertDiv.innerHTML = '<strong>Error: </strong><span>' + data.message + '</span>';
                document.querySelector(".alerts").appendChild(alertDiv);
            }
            updateProgress(data.message);
            updates_remaining--;
            updateTask();
        }
    }

    function updateFinishedCallback(data) {
        updateProgress(data.message);
    }

    // Initialize confirm button click handler
    document.getElementById("confirm-btn").addEventListener('click', function() {
        this.classList.add('disabled');
        document.querySelector(".progress").classList.remove('d-none');
        for (let i = 0; i < update_threads; i++)
            updateTask();
    });
}

/**
 * Initialize Update Players page functionality
 * @param {Object} config - Configuration object with data from server
 */
function initUpdatePlayers(config) {
    let updates_remaining = config.updates_remaining;
    const total_updates = updates_remaining;
    let updates_done = 0;
    const update_threads = 16;
    const tasks = config.tasks.slice().reverse(); // copy and reverse

    async function updateTask() {
        if(tasks.length > 0) {
            try {
                const response = await fetch(config.updatePlayerTaskUrl + '/' + tasks.pop());
                const data = await response.json();
                updateTaskCallback(data);
            } catch (error) {
                console.error('Error:', error);
            }
        }
    }

    function updateProgress(message) {
        if (message)
            document.querySelector(".progress-bar").textContent = message;

        const progress = Math.round(++updates_done * 100 / total_updates);
        document.querySelector(".progress-bar").setAttribute("aria-valuenow", progress);
        document.querySelector(".progress-bar").setAttribute("style", "width:" + progress + "%");
    }

    function updateTaskCallback(data) {
        if (data && data.id) {
            if(!data.status) {
                const alertDiv = document.createElement('div');
                alertDiv.className = 'alert alert-danger';
                alertDiv.innerHTML = '<strong>Error: </strong><span>' + data.message + '</span>';
                document.querySelector(".alerts").appendChild(alertDiv);
            }
            updateProgress(data.message);
            updates_remaining--;
            if(updates_remaining > 0) {
                updateTask();
            }
        }
    }

    // Initialize confirm button click handler
    document.getElementById("confirm-btn").addEventListener('click', function() {
        this.classList.add('disabled');
        document.querySelector(".progress").classList.remove('d-none');
        for (let i = 0; i < update_threads; i++)
            updateTask();
    });
}

// =============================================
// CLANS CONTROLLER FUNCTIONS
// =============================================

/**
 * Initialize Clans Track page functionality
 * @param {Object} config - Configuration object with data from server
 */
function initClansTrack(config) {
    let track_progress = 0;
    let track_enabled = false;
    let prev_data = null;

    function findMember(members, tag) {
        return members.find(member => member.tag === tag) || null;
    }

    function getTime() {
        const d = new Date();
        const h = d.getHours();
        const m = d.getMinutes();
        const hh = (h > 9 ? '' : '0') + h;
        const mm = (m > 9 ? '' : '0') + m;
        return hh + ':' + mm;
    }

    function createMessage(style, msg) {
        const p = document.createElement('p');
        p.className = style;
        p.textContent = getTime() + ' ' + msg;
        document.querySelector('.messages').appendChild(p);
    }

    function dataCallback(data) {
        if (!track_enabled) {
            trackingStopped();
            return;
        }

        for (const old_mbr of prev_data) {
            const new_mbr = findMember(data, old_mbr.tag);
            if (new_mbr === null) {
                createMessage('text-danger', old_mbr.name + ' left clan');
            }
        }

        for (const new_mbr of data) {
            const old_mbr = findMember(prev_data, new_mbr.tag);
            if (old_mbr === null) {
                createMessage('text-success', new_mbr.name + ' joined clan');
                if (new_mbr.donated > 0) {
                    createMessage('text-warning', new_mbr.name + ' donated ' + new_mbr.donated);
                }
            } else {
                if (new_mbr.donated > old_mbr.donated) {
                    createMessage('text-warning', new_mbr.name + ' donated ' + (new_mbr.donated - old_mbr.donated));
                }
            }
        }

        for (const new_mbr of data) {
            const old_mbr = findMember(prev_data, new_mbr.tag);
            if (old_mbr === null) {
                if (new_mbr.received > 0) {
                    createMessage('text-primary', new_mbr.name + ' received ' + new_mbr.received);
                }
            }
            else {
                if (new_mbr.received > old_mbr.received) {
                    createMessage('text-primary', new_mbr.name + ' received ' + (new_mbr.received - old_mbr.received));
                }
            }
        }

        scheduleNext(data);
    }

    function failCallback() {
        createMessage("bg-danger", "Tracking failure");
        trackingStopped();
    }

    async function timerCallback() {
        if (!track_enabled) {
            trackingStopped();
            return;
        }

        if (track_progress >= 20) {
            track_progress = 0;
            try {
                const response = await fetch(config.donationsUrl);
                if (response.ok) {
                    const data = await response.json();
                    dataCallback(data);
                } else {
                    throw new Error('Network response was not ok');
                }
            } catch (error) {
                failCallback();
            }
        } else {
            setTimeout(timerCallback, 3000);
        }

        updateProgress();
    }

    function scheduleNext(data) {
        prev_data = data;
        setTimeout(timerCallback, 3000);
        updateProgress();
    }

    function updateProgress() {
        track_progress++;
        const progress_text = `<i class="fas fa-hourglass fa-spin"></i> Checking donations...`;
        document.querySelector('.status').innerHTML = progress_text;
    }

    function trackingStopped() {
        createMessage('bg-warning', 'Tracking stopped');
        document.querySelector('.btn-stop').classList.add('d-none');
        document.querySelector('.btn-start').classList.remove('d-none');
        document.querySelector('.status').innerHTML = '';
    }

    // Initialize button handlers
    document.querySelector('.btn-start').addEventListener('click', async function() {
        this.classList.add('d-none');
        document.querySelector('.btn-stop').classList.remove('d-none');
        createMessage('bg-success', 'Tracking started');
        track_enabled = true;
        track_progress = 0;
        try {
            const response = await fetch(config.donationsUrl);
            if (response.ok) {
                const data = await response.json();
                scheduleNext(data);
            } else {
                throw new Error('Network response was not ok');
            }
        } catch (error) {
            failCallback();
        }
    });
    
    document.querySelector('.btn-stop').addEventListener('click', function() {
        track_enabled = false;
        this.classList.add('d-none');
        createMessage('bg-info', 'Tracking stopping...');
        document.querySelector('.status').innerHTML = '';
    });
}

/**
 * Initialize Clans Weight page functionality
 * @param {Object} config - Configuration object with data from server
 */
function initClansWeight(config) {
    initTableSort();
    
    // Number input controls (+ and - buttons)
    document.querySelectorAll('.btn-number').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const fieldName = this.getAttribute('data-field');
            const type = this.getAttribute('data-type');
            const input = document.querySelector("input[name='" + fieldName + "']");
            let currentVal = parseInt(input.value);
            if (!isNaN(currentVal)) {
                let stepValue = parseInt(input.getAttribute('step'));
                if (!stepValue) stepValue = 1;
                currentVal = Math.round(currentVal / stepValue) * stepValue;
                if (type === 'minus') {
                    let minValue = parseInt(input.getAttribute('min'));
                    if (!minValue) minValue = 0;
                    if (currentVal - stepValue >= minValue) {
                        input.value = currentVal - stepValue;
                        input.dispatchEvent(new Event('change'));
                    }
                    if (parseInt(input.value) === minValue) {
                        this.setAttribute('disabled', true);
                    }
                } else if (type === 'plus') {
                    let maxValue = parseInt(input.getAttribute('max'));
                    if (!maxValue) maxValue = 999;
                    if (currentVal + stepValue <= maxValue) {
                        input.value = currentVal + stepValue;
                        input.dispatchEvent(new Event('change'));
                    }
                    if (parseInt(input.value) === maxValue) {
                        this.setAttribute('disabled', true);
                    }
                }
            } else {
                input.value = 0;
            }
            input.focus();
        });
    });

    // Number input validation and button state management
    document.querySelectorAll('.input-number').forEach(function(input) {
        input.addEventListener('focusin', function() {
            this.setAttribute('data-old-value', this.value);
        });

        input.addEventListener('change', function() {
            let minValue = parseInt(this.getAttribute('min'));
            let maxValue = parseInt(this.getAttribute('max'));
            let stepValue = parseInt(this.getAttribute('step'));
            if (!stepValue) stepValue = 1;
            if (!minValue) minValue = 0;
            if (!maxValue) maxValue = 999;
            const valueCurrent = parseInt(this.value);

            const name = this.getAttribute('name');
            if (valueCurrent > minValue) {
                const minusBtn = document.querySelector(".btn-number[data-type='minus'][data-field='" + name + "']");
                if (minusBtn) minusBtn.removeAttribute('disabled');
            } else {
                this.value = minValue;
            }
            if (valueCurrent < maxValue) {
                const plusBtn = document.querySelector(".btn-number[data-type='plus'][data-field='" + name + "']");
                if (plusBtn) plusBtn.removeAttribute('disabled');
            } else {
                this.value = maxValue;
            }
        });
    });

    // War member checkbox handling
    document.querySelectorAll('.member-in-war').forEach(function(checkbox) {
        checkbox.addEventListener('change', function(e) {
            const row = this.closest('tr');
            if (this.checked) {
                row.classList.add('table-success');
                row.classList.remove('table-danger');
            } else {
                row.classList.add('table-danger');
                row.classList.remove('table-success');
            }
            calculateWarWeight();
        });
    });

    // Member weight input handling with tooltips
    document.querySelectorAll('.member-weight').forEach(function(input) {
        // Initialize Bootstrap 5 tooltip
        let tooltip = new bootstrap.Tooltip(input, {
            title: '',
            trigger: 'manual',
            placement: 'top'
        });

        function updateTooltipText() {
            const valueCurrent = parseInt(input.value);
            let tooltipText = "";
            if (valueCurrent > 26000) {
                const div5 = parseInt(valueCurrent / 5);
                const div4 = parseInt(valueCurrent / 4);
                tooltipText = "5x" + div5 + ", 4x" + div4;
            } else if (valueCurrent > 0) {
                const mul5 = valueCurrent * 5;
                const mul4 = valueCurrent * 4;
                tooltipText = mul5 + "/5, " + mul4 + "/4";
            }
            
            if (tooltipText) {
                // Update tooltip content
                tooltip.dispose();
                tooltip = new bootstrap.Tooltip(input, {
                    title: tooltipText,
                    trigger: 'manual',
                    placement: 'top'
                });
            }
            return tooltipText;
        }

        input.addEventListener('focus', function() {
            const tooltipText = updateTooltipText();
            if (tooltipText) {
                tooltip.show();
            }
        });

        input.addEventListener('blur', function() {
            tooltip.hide();
        });

        input.addEventListener('change', function(e) {
            calculateWarWeight();
            const tooltipText = updateTooltipText();
            // If tooltip is currently shown, update it
            if (document.activeElement === input && tooltipText) {
                tooltip.show();
            }
        });

        input.addEventListener('input', function(e) {
            const tooltipText = updateTooltipText();
            // If tooltip is currently shown and input is focused, update it
            if (document.activeElement === input && tooltipText) {
                tooltip.show();
            }
        });
    });

    // Weight multiplier buttons
    document.querySelectorAll('.weight-multiply').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
            const fieldName = this.getAttribute('data-field');
            const input = document.querySelector("input[name='" + fieldName + "']");
            const multiplier = parseInt(this.getAttribute('multiplier'));
            const max = parseInt(this.getAttribute('max'));
            const currentVal = parseInt(input.value);
            const newVal = currentVal * multiplier;
            if (newVal <= max) {
                input.value = newVal;
                input.dispatchEvent(new Event('change'));
            }
        });
    });

    // Submit button handlers
    const submitBtn = document.getElementById('command-submit');
    if (submitBtn) {
        submitBtn.addEventListener('click', function() {
            return confirm("Submit weights to FWA Weight Sheet?");
        });
    }

    const submitDisabledBtn = document.getElementById('command-submit-disabled');
    if (submitDisabledBtn) {
        submitDisabledBtn.addEventListener('click', function() {
            alert("Please select war roster from top of the page.");
            return false;
        });
    }

    calculateWarWeight();
    resizeChart();

    // Handle queued submit modal
    if (config.weightSubmitQueued) {
        statusPoll();
        const submitModal = new bootstrap.Modal(document.getElementById('submitModal'));
        submitModal.show();
    }

    // Initialize Google Charts if comparison data is provided
    if (config.comparisonData) {
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(function () {
            const data = new google.visualization.DataTable();
            data.addColumn('number', 'Pos');
            data.addColumn('number', 'Weight');
            data.addColumn('number', 'Average');
            data.addRows(config.comparisonData);

            resizeChart();

            const options = {
                theme: 'maximized',
                legend: {
                    alignment: 'end'
                },
                series: {
                    3: { color: 'black' }
                }
            };

            new google.visualization.LineChart(document.getElementById('chart')).draw(data, options);
        });
    }
    
    async function statusPoll() {
        try {
            const response = await fetch(config.weightStatusUrl);
            const data = await response.json();
            document.getElementById("submit-details").textContent = data.text;
            if (data.final === true) {
                if (data.result) {
                    document.getElementById("submitModalTitle").textContent = "Submit done";
                    document.getElementById("submitModalHeader").classList.add("modal-header-success");
                } else {
                    document.getElementById("submitModalTitle").textContent = "Submit failed";
                    document.getElementById("submitModalHeader").classList.add("modal-header-danger");
                    if (data.text !== "Too few weight changes since last submit.")
                        document.getElementById("backup-submit-button").classList.remove("d-none");
                }
                document.getElementById("submitModalHeader").classList.remove("modal-header-primary");
                document.getElementById("submitModalAnimation").classList.remove("loader");
            }
            if(data.final === false)
                setTimeout(statusPoll, 1000);
        } catch (error) {
            document.getElementById("submitModalTitle").textContent = "Submit error";
            document.getElementById("submitModalHeader").classList.add("modal-header-danger");
            document.getElementById("submitModalHeader").classList.remove("modal-header-primary");
            document.getElementById("submitModalAnimation").classList.remove("loader");
            document.getElementById("submit-details").textContent = "Network error: " + error.message;
            document.getElementById("backup-submit-button").classList.remove("d-none");
        }
    }

    function calculateWarWeight() {
        let members_total = 0;
        let members_in_war = 0;

        document.querySelectorAll('.member-in-war').forEach(function(checkbox) {
            members_total++;
            if (checkbox.checked)
                members_in_war++;
        });

        document.getElementById('war-members').textContent = members_in_war + ' / ' + members_total;

        let th18_count = 0, th17_count = 0, th16_count = 0, th15_count = 0, th14_count = 0, th13_count = 0;
        let th12_count = 0, th11_count = 0, th10_count = 0, th9_count = 0, th8_count = 0, th7_count = 0;
        let war_weight = 0;

        document.querySelectorAll('.member-weight').forEach(function(input) {
            const member_weight = parseInt(input.value);
            const checkName = input.getAttribute('war-check');
            const warCheckbox = document.querySelector("input[name='" + checkName + "']");
            if (warCheckbox && warCheckbox.checked) {
                war_weight += member_weight;
                if (member_weight > config.maxWeights.TH17)
                    th18_count++;
                if (member_weight > config.maxWeights.TH16)
                    th17_count++;
                else if (member_weight > config.maxWeights.TH15)
                    th16_count++;
                else if (member_weight > config.maxWeights.TH14)
                    th15_count++;
                else if (member_weight > config.maxWeights.TH13)
                    th14_count++;
                else if (member_weight > config.maxWeights.TH12)
                    th13_count++;
                else if (member_weight > config.maxWeights.TH11)
                    th12_count++;
                else if (member_weight > config.maxWeights.TH10)
                    th11_count++;
                else if (member_weight > config.maxWeights.TH9)
                    th10_count++;
                else if (member_weight > config.maxWeights.TH8)
                    th9_count++;
                else if (member_weight > config.maxWeights.TH7)
                    th8_count++;
                else
                    th7_count++;
            }
        });

        document.getElementById('war-weight').textContent = war_weight;
        document.getElementById('war-composition').textContent = th18_count + ' / ' + th17_count + ' / ' + th16_count + ' / ' + th15_count + ' / ' + th14_count + ' / ' + th13_count + ' / ' + th12_count + ' / ' + th11_count + ' / ' + th10_count + ' / ' + th9_count + ' / ' + th8_count + ' / ' + th7_count;

        const saveButtons = document.querySelectorAll('.btn-save');
        const isValidSize = (members_in_war === config.warSizes.SIZE1 || members_in_war === config.warSizes.SIZE2 || members_in_war === config.warSizes.SIZE3);
        saveButtons.forEach(function(btn) {
            if (isValidSize) {
                btn.classList.add('btn-success');
                btn.classList.remove('btn-danger');
            } else {
                btn.classList.add('btn-danger');
                btn.classList.remove('btn-success');
            }
        });
    }

    function resizeChart() {
        const chart = document.getElementById('chart');
        if (chart) {
            const width = chart.offsetWidth;
            let height = Math.round(width * 0.5);
            if (height > 400) height = 400;
            chart.style.height = height + 'px';
        }
    }
}

// =============================================
// SIMPLE INIT FUNCTIONS FOR TABLE SEARCH PAGES
// =============================================

/**
 * Initialize Clans Departed page
 */
function initClansDeparted() {
    initTableSort();
    initTableSearch();
}

/**
 * Initialize Clans Following page
 */
function initClansFollowing() {
    initTableSort();
    initTableSearch();
}

/**
 * Initialize Clans Index page
 */
function initClansIndex() {
    initTableSort();
    initTableSearch();
}

/**
 * Initialize Clans My page
 */
function initClansMy() {
    initTableSort();
    initTableSearch();
}

/**
 * Initialize Syncs Index page
 */
function initSyncsIndex() {
    initTableSort();
    initTableSearch();
}

/**
 * Initialize Syncs Details page
 */
function initSyncsDetails() {
    initTableSort();
    initTableSearch();
}

/**
 * Initialize Players My page
 */
function initPlayersMy() {
    initTableSort();
}

/**
 * Initialize Players Index page
 */
function initPlayersIndex() {
    initTableSort();
}

/**
 * Initialize Players Details page
 */
function initPlayersDetails() {
    initTableSort();
}

/**
 * Initialize Clans Details page
 */
function initClansDetails() {
    initTableSort();
}

/**
 * Initialize Clans WarDetails page
 */
function initClansWarDetails() {
    initTableSort();
}

/**
 * Initialize Clans Attacks page
 */
function initClansAttacks() {
    initTableSort();
}