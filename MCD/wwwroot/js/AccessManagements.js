
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#accessManagementsTable').DataTable({
        "ajax": { url: '/home/getallaccessmanagements' },
        "columns": [
            {
                data: 'document.fileName',
                render: function (data, type, row) {
                    //row to get other data from columns
                    let userId = row.sharedFromId; // in order to get the user id to get the path
                    return `<button onclick="openDocument('${userId}', '${data}')" class="btn btn-primary">
                            <i class="bi bi-file-earmark"></i> Open Document
                        </button>`;
                },
                "width": "15%"
            },
            { data: 'document.title', "width": "15%" },
            { data: 'sharedToEmail', "width": "15%" },
            {
                data: 'sharedAt', "width": "15%",
                render: function (data) {
                    return new Date(data).toLocaleString(); // Format timestamp to make it readable
                }

            },
            { data: 'sharedToEmail', "width": "20%" },
            {
                data: 'id',
                "render": function (data) { //the data is the id
                    return `<a href="" class="btn btn-secondary mx-2"> Change Access </a>`

                },
                "width": "10%"
            },
            {
                data: 'id',
                "render": function (data, type, row) { //the data is the id
                    return `<a href="/Home/RemoveAccess?SharedAccessId=${data}&FileName=${row.document.fileName}" class="btn btn-warning mx-2"> <i class="bi bi-x-lg"></i> Remove </a>`

                },
                "width": "10%"
            },
        ]
    });
}

function openDocument(userId, fileName) { // a function to call inside render for getting html in the table
    $.ajax({
        url: `/home/GetDocument?userId=${userId}&fileName=${fileName}`, //avoid redundant code by using the same method
        type: 'GET',
        success: function (response) {
            if (response && response.fileUrl) {
                window.open(response.fileUrl, '_blank'); // Open in new tab -> _self to open in the same page
            } else {
                alert("Error: Unable to fetch the document.");
            }
        },
        error: function (xhr) {
            console.log("AJAX Error:", xhr.responseText);
            alert("Error: Could not open the document.");
        }
    });
}