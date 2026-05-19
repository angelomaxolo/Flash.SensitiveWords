let table;

$(document).ready(function () {

    // INIT DATATABLE
    table = $('#wordsTable').DataTable({
        pageLength: 10,
        responsive: true,
        autoWidth: false
    });

    // CREATE (AJAX)
    $("#createForm").submit(function (e) {
        e.preventDefault();

        const word = $("#wordInput").val().trim();

        if (!word) {
            $("#wordError").removeClass("d-none");
            return;
        }

        $("#wordError").addClass("d-none");

        $.ajax({
            url: "/SensitiveWords/Create",
            type: "POST",
            data: { word: word },
            success: function (res) {

                table.row.add([
                    res.id,
                    `<span class="badge bg-secondary">${escapeHtml(res.word)}</span>`,
                    `<button class="btn btn-warning btn-sm edit-btn" data-id="${res.id}" data-word="${escapeHtml(res.word)}">Edit</button>`
                    `<button class="btn btn-danger btn-sm delete-btn" data-id="${res.id}">Delete</button>`
                ]).draw(false);

                $("#wordInput").val("");
            }
        });
    });

    // DELETE (EVENT DELEGATION - SAFE)
    $(document).on("click", ".delete-btn", function () {

        const id = $(this).data("id");
        const row = $(this).closest("tr");

        $.ajax({
            url: "/SensitiveWords/Delete/" + id,
            type: "GET",
            success: function () {

                row.css("background-color", "#ffdddd");

                row.fadeOut(300, function () {
                    table.row(row).remove().draw(false);
                });
            }
        });
    });

});

$(document).on("click", ".delete-btn", function () {

    const id = $(this).data("id");

    const row = $(this).closest("tr");

    $.ajax({
        url: "/SensitiveWords/Delete/" + id,
        type: "GET",
        success: function () {

            row.fadeOut(200, function () {
                $('#wordsTable').DataTable().row(row).remove().draw(false);
            });

        }
    });

});


let editModal = new bootstrap.Modal(document.getElementById('editModal'));

// OPEN EDIT MODAL
$(document).on("click", ".edit-btn", function () {

    const id = $(this).data("id");
    const word = $(this).data("word");

    $("#editId").val(id);
    $("#editWord").val(word);

    $("#editError").addClass("d-none");

    editModal.show();
});

$("#saveEditBtn").click(function () {

    const id = $("#editId").val();
    const word = $("#editWord").val().trim();
    const sensitiveWordDto = {
        id: id,
        word: word
    };

    if (!word) {
        $("#editError").removeClass("d-none");
        return;
    }

    $.ajax({
        url: "/SensitiveWords/Update",
    type: "PUT",
    contentType: "application/json",
        data: JSON.stringify(sensitiveWordDto),
        success: function () {

            // update table row live
            let row = $(".edit-btn[data-id='" + id + "']").closest("tr");

            // update UI
            row.find("td:eq(1)").html(
                `<span class="badge bg-secondary">${escapeHtml(word)}</span>`
            );

            // close modal
            editModal.hide();
        }
    });

});

// SAFE HTML ESCAPE (IMPORTANT)
function escapeHtml(text) {
    return $('<div>').text(text).html();
}
