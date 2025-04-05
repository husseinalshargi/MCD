
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/Customer/home/getall' },
        "columns": [
            {
                data: 'fileName',
                render: function (data, type, row) {//the file name to put in the path of the files
                    //row to get other data from columns
                    let userId = row.applicationUserId; // in order to get the user id to get the path
                    let documentId = row.id; // in order to get the document id to get the path
                    return `<div>
                                <button onclick="openDocument('${userId}', ${documentId}, '${data}')" class="btn btn-danger">
                                    <i class="bi bi-file-earmark"></i> Access File
                                </button>
                            </div>`;
                },
                "width": "15%"
            },
            { data: 'title', "width": "15%" },
            {
                data: 'fileName', "width": "15%", render: function (data) { // all data returned will be passed in this function
                    return data.replace(/\.[^/.]+$/, ""); //to remove file extension using Regular Expression
                }
            },
            {
                data: 'uploadDate', "width": "15%",
                render: function (data) {
                    return new Date(data).toLocaleString(); // Format timestamp to make it readable
                }
            },
            {
                data: 'updateDate', "width": "15%",
                render: function (data) {
                    return new Date(data).toLocaleString(); // Format timestamp to make it readable
                }
            },
            { data: 'categoryName', "width": "10%" },
            {
                data: 'id',
                "render": function (data) { //the data is the id
                    return `<a href="/Customer/document/moreinfo/${data}" class="btn btn-info mx-2"> <i class="bi bi-info-circle"></i> Info </a>`

                    //here it will return a button for the entering the info page
                },
                "width": "15%"
            },
            //to make the hidden column for entity text as it is only searchable but the user can not see it in the ui
            {
                data: 'entityText',   //searchable hidden column
                visible: false,       //hides the column from view
                searchable: true      
            }
        ]
    });
}

function openDocument(userId, documentId, fileName) { // a function to call inside render for getting html in the table
    $.ajax({
        url: `/Customer/home/GetDocument?userId=${userId}&documentId=${documentId}&fileName=${fileName}`,
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
            location.reload(); // Refresh the page
        }
    });
}