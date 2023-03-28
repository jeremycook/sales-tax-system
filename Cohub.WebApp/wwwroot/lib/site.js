/// <reference path="autosize/autosize.js" />
/// <reference path="apps/utils.js" />
x(() => {
    // Warn the user when potentially unsaved changes may be lost.
    const warnbeforeunload = (ev) => {
        ev.preventDefault()
        ev.returnValue = ''
    }
    const warnofdataloss = (ev) => {
        if (ev.target.form && ev.target.form.method.toLowerCase() == "post") {
            document.removeEventListener("change", warnofdataloss)
            window.addEventListener("beforeunload", warnbeforeunload)
        }
    }
    document.addEventListener("change", warnofdataloss)
    document.addEventListener("submit", (ev) => {
        if (ev.target.tagName == "FORM") {
            // Allow form submission without warning
            window.removeEventListener("beforeunload", warnbeforeunload)
        }
    })

    document.addEventListener("keyup", (ev) => {
        if (ev.ctrlKey && ev.key == "Enter") {
            /** @type {HTMLFormElement} */
            const form = ev.target.form
            if (form) {
                let buttons = form.getElementsByTagName("button")
                for (var button of buttons) {
                    if (button.type == "submit") {
                        window.removeEventListener("beforeunload", warnbeforeunload)
                        button.click()
                        ev.preventDefault()
                        return
                    }
                }
            }
        }
    });

    document.addEventListener("readystatechange", () => {
        /**
         * Autocomplete with input[list]->datalist
         */
        document.querySelectorAll("input[list]").forEach(autocompleteWithDatalist)
    })

    document.addEventListener("readystatechange", async () => {
        /**
         * Autosize textareas
         */
        const matches = document.querySelectorAll("textarea")
        if (matches.length > 0) {
            autosize(matches)
        }
    })
})

// Classically trained 🎮

var site = {
    // Example: <select select2='{ "select2": "options", "go": "here" }'></select>
    select2: (i, el) => {
        const $el = $(el),
            select2Attr = el.getAttribute("select2"),
            options = JSON.parse(select2Attr.startsWith("{") ? select2Attr : ("{" + select2Attr + "}"));

        $el.select2(options);

        if (options.prependSelection) {
            $el.on("select2:select", function (evt) {
                var element = evt.params.data.element;
                var $element = $(element);

                window.setTimeout(function () {
                    if ($el.find(":selected").length > 1) {
                        var $second = $el.find(":selected").eq(-2);

                        $element.detach();
                        $second.after($element);
                    } else {
                        $element.detach();
                        $el.prepend($element);
                    }

                    $el.trigger("change");
                }, 1);
            });

            $el.on("select2:unselect", function (evt) {
                if ($el.find(":selected").length) {
                    var element = evt.params.data.element;
                    var $element = $(element);
                    $el.find(":selected").after($element);
                }
            });
        }
    },
    /**
     * @param {number} index
     * @param {HTMLElement} element
     */
    "inject-module": function (index, element) {
        const injectModule = element.getAttribute("inject-module"),
            injectMember = element.getAttribute("inject-member");

        inject(injectModule).then((module) => {
            if (injectMember) {
                module[injectMember](element);
            }
            else {
                module(element);
            }
        });
    }
};

$(() => {
    $.each(site, function (attributeName, binder) {
        $("[" + $.escapeSelector(attributeName) + "]").each(binder);
    });
});
