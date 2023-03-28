$("[data-autocomplete-source]").each(function () {
    let input = $(this),
        source = input.data("autocomplete-source");

    input.autocomplete({
        source: source,
        minLength: 0
    }).focus(function () { if (!$(this).val()) $(this).autocomplete("search") })
})