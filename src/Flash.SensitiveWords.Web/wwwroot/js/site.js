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
            showAlert("danger", "Please enter a word before adding.");
            return;
        }

        $("#wordError").addClass("d-none");

        $.ajax({
            url: "/SensitiveWords/Create",
            type: "POST",
            data: { word: word },
            success: function (res) {
                if (!res || !res.id) {
                    showAlert("danger", "Unable to add the sensitive word. Please try again.");
                    return;
                }

                table.row.add([
                    res.id,
                    `<span class="badge bg-secondary">${escapeHtml(res.word)}</span>`,
                    `<button class="btn btn-warning btn-sm edit-btn me-1" data-id="${res.id}" data-word="${escapeHtml(res.word)}">Edit</button><button class="btn btn-danger btn-sm delete-btn" data-id="${res.id}">Delete</button>`
                ]).draw(false);

                $("#wordInput").val("");
                showAlert("success", "Sensitive word added successfully.");
            },
            error: function () {
                showAlert("danger", "Unable to add the sensitive word. Please try again.");
            }
        });
    });

    // DELETE (EVENT DELEGATION - SAFE)
    $(document).on("click", ".delete-btn", function () {

        const id = $(this).data("id");
        const row = $(this).closest("tr");
        const dtRow = table.row(row);

        $.ajax({
            url: "/SensitiveWords/Delete/" + id,
            type: "GET",
            success: function () {
                row.css("background-color", "#ffdddd");
                row.fadeOut(300, function () {
                    dtRow.remove().draw(false);
                    showAlert("success", "Sensitive word deleted successfully.");
                });
            },
            error: function () {
                showAlert("warning", "Unable to delete the sensitive word. Please try again.");
            }
        });
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
            showAlert("success", "Sensitive word updated successfully.");
        },
        error: function () {
            showAlert("danger", "Unable to update the sensitive word. Please try again.");
        }
    });

});

// ALERT HELPERS
function showAlert(type, message) {
    const alertId = `alert-${Date.now()}`;
    const alertMarkup = `
        <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${escapeHtml(message)}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    const container = $("#notificationContainer");

    if (container.length === 0) {
        return;
    }

    container.append(alertMarkup);

    setTimeout(function () {
        const alertElement = $(`#${alertId}`);
        if (alertElement.length) {
            alertElement.fadeOut(200, function () {
                $(this).remove();
            });
        }
    }, 4500);
}

// SAFE HTML ESCAPE (IMPORTANT)
function escapeHtml(text) {
    return $('<div>').text(text).html();
}
