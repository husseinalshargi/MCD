
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url:'/home/getall'},
        "columns": [
            {
                data: 'fileName',
                render: function (data, type, row) {//the file name to put in the path of the files
                    //row to get other data from columns
                    let userId = row.applicationUserId; // in order to get the user id to get the path
                    let fileType = row.fileType;
                    return `<button onclick="openDocument('${userId}', '${data}', '${fileType}')" class="btn btn-primary">
                            <i class="bi bi-file-earmark"></i> Open Document
                        </button>`;
                },
                "width": "15%"
            },
            {
                data: 'fileName', "width": "15%", render: function (data) { // all data returned will be passed in this function
                    return data.replace(/\.[^/.]+$/, ""); //to remove file extension using Regular Expression
                }
            },
            {
                data: 'fileName', "width": "15%", render: function (data) { // all data returned will be passed in this function
                    return data.replace(/\.[^/.]+$/, ""); //to remove file extension using Regular Expression
                }
            },
            { data: 'uploadDate', "width": "15%" },
            { data: 'updateDate', "width": "15%" },
            { data: 'category.categoryName', "width": "10%" },
            {
                data: 'id',
                "render": function (data) { //the data is the id
                    return `<div class="w-75 btn-group" role="group">
                    <a href="" class="btn btn-info mx-2"> <i class="bi bi-info-circle"></i> Info </a>
                    </div>
                    `

                    //here it will return a button for the info
                },
                "width": "15%"
            },
        ]
    });
}

function openDocument(userId, fileName, fileType) { // a function to call inside render for getting html in the table
    let fileUrl = `/home/GetDocument?userId=${encodeURIComponent(userId)}&fileName=${encodeURIComponent(fileName)}&fileType=${encodeURIComponent(fileType)}`;
    window.open(fileUrl, '_blank'); // Open in new tab -> _self to open in the same page
}