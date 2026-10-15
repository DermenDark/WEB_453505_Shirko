$(document).on(
    "click",
    "#product-list .page-link",
    function (event) {
        const link = $(this);
        const listItem = link.closest(".page-item");
        const url = link.attr("href");

        if (
            listItem.hasClass("disabled") ||
            listItem.hasClass("active") ||
            !url
        ) {
            event.preventDefault();
            return;
        }

        event.preventDefault();

        const container = $("#product-list");

        container
            .addClass("opacity-50")
            .attr("aria-busy", "true");

        $.ajax({
            url: url,
            method: "GET",
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        })
            .done(function (html) {
                container.html(html);
            })
            .fail(function () {
                window.location.href = url;
            })
            .always(function () {
                container
                    .removeClass("opacity-50")
                    .removeAttr("aria-busy");
            });
    }
);