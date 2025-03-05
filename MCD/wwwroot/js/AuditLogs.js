
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#auditLogsTable').DataTable({
        "ajax": { url: '/logs/getall' },
        "columns": [
            { data: 'userEmailAddress', "width": "25%" },
            { data: 'action', "width": "25%" },
            { data: 'fileName', "width": "25%" },
            {
                data: 'actionDate',
                width: "25%",
                render: function (data) {
                    return new Date(data).toLocaleString(); // Format timestamp to make it readable
                }
            }
        ]
    });
}