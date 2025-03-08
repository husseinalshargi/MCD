
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#accessManagementsTable').DataTable({
        "ajax": { url: '/Customer/home/getallaccessmanagements' },
        "columns": [
            {
                data: 'document.fileName',
                render: function (data, type, row) {
                    //row to get other data from columns
                    let userId = row.sharedFromId; // in order to get the user id to get the path
                    let documentId = row.documentId; // in order to get the document id to get the path
                    return `<div>
                                <button onclick="openDocument('${userId}', '${documentId}', '${data}')" class="btn btn-danger">
                                    <i class="bi bi-file-earmark"></i> Access File
                                </button>
                            </div>`;
                },
                "width": "15%"
            },
            { data: 'document.title', "width": "25%" },
            { data: 'sharedToEmail', "width": "25%" },
            {
                data: 'sharedAt', "width": "20%",
                render: function (data) {
                    return new Date(data).toLocaleString(); // Format timestamp to make it readable
                }

            },
            {
                data: 'id',
                "render": function (data, type, row) { //the data is the id
                    return `<a href="/Customer/Home/RemoveAccess?SharedAccessId=${data}&FileName=${row.document.fileName}" class="btn btn-warning mx-2"> <i class="bi bi-x-lg"></i> Remove </a>`

                },
                "width": "15%"
            },
        ]
    });
}

function openDocument(userId, documentId, fileName) { // a function to call inside render for getting html in the table
    $.ajax({
        url: `/Customer/home/GetDocument?userId=${userId}&documentId=${documentId}&fileName=${fileName}`, //avoid redundant code by using the same method
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