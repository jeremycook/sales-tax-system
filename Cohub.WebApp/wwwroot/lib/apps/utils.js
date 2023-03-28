function x(func) {
    func()
}

/**
 * Returns a string in the format "yyyy-mm-dd".
 */
Date.prototype.toISODateString = function () {
    return this.toISOString().split('T')[0];
}

/**
 * Returns a string in the format "yyyy-mm-dd" or null if text is not a string.
 * @param {string | any} text
 */
Date.toISODateString = function (text) {
    return typeof text == "string" ? new Date(text).toISODateString() : null;
}

/**
 * Returns a function, that, as long as it continues to be invoked, will not
 * be triggered. The function will be called after it stops being called for
 * N milliseconds. If `immediate` is passed, trigger the function on the
 * leading edge, instead of the trailing.
 * @param {Function} func
 * @param {number | 350} waitms
 * @param {boolean | false} immediate
 */
function debounce(func, waitms, immediate) {
    if (typeof waitms != "number") waitms = 350
    if (typeof immediate != "boolean") immediate = false

    let timeout;
    return function () {
        let context = this, args = arguments;
        let later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args)
        };
        let callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, waitms);
        if (callNow) func.apply(context, args);
    };
};

/**
 * 
 * @param {string} path
 * @returns {string}
 */
function normalizePath(path) {
    path = Array.prototype.join.apply(arguments, ['/'])
    let sPath;
    while (sPath !== path) {
        sPath = n(path);
        path = n(sPath);
    }
    function n(s) { return s.replace(/\/+/g, '/').replace(/\w+\/+\.\./g, '') }
    return path.replace(/^\//, '').replace(/\/$/, '');
}

/**
 * Request all the HTTP things.
 * @param {{ url: string, method: string | "GET", headers: {}? }} obj
 */
function request(obj) {
    return new Promise((resolve, reject) => {
        let xhr = new XMLHttpRequest();
        xhr.open(obj.method || "GET", obj.url);
        if (obj.headers) {
            Object.keys(obj.headers).forEach(key => {
                xhr.setRequestHeader(key, obj.headers[key]);
            });
        }
        xhr.onload = () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(xhr.response);
            } else {
                reject(xhr.statusText);
            }
        };
        xhr.onerror = () => reject(xhr.statusText);
        xhr.send(obj.body);
    });
};

/**
 * TODO: Document autocompleteWithDatalist(input)
 * @param {HTMLInputElement} input
 */
function autocompleteWithDatalist(input) {

    if (input.hasAttribute(autocompleteWithDatalist.initialized)) {
        return
    }
    else {
        input.setAttribute(autocompleteWithDatalist.initialized, 1)
    }

    //input.setAttribute("autocomplete", "off")

    const datalistId = input.getAttribute("list")

    /** @type {HTMLDataListElement} */
    const datalist = document.getElementById(datalistId)

    const autocomplete =
        /** @param {KeyboardEvent} ev */
        (ev) => {
            if (input.value && ev.keyCode == 9) {

                const lowerCaseValue = input.value.toLowerCase()
                const options = Array.from(datalist.querySelectorAll("option"))

                // First look for exact match on value
                for (let i in options) {
                    let option = options[i]
                    if (option.value == input.value) {
                        // Do nothing if an exact match is found
                        return
                    }
                    else if (option.value.toLowerCase() == lowerCaseValue) {
                        // Fix casing if that is the only difference
                        input.value = option.value
                        input.dispatchEvent(new Event("input"))
                        input.dispatchEvent(new Event("change"))
                        return
                    }
                }

                // Next try a looser match
                for (let i in options) {
                    let option = options[i]
                    if (option.label.toLowerCase().includes(lowerCaseValue) ||
                        option.value.toLowerCase().includes(lowerCaseValue)) {

                        input.value = option.value
                        input.dispatchEvent(new Event("input"))
                        input.dispatchEvent(new Event("change"))
                        return
                    }
                }
            }
        }

    // Autocomplete the top result when the user presses TAB
    input.addEventListener("keydown", autocomplete);

    const src = datalist.getAttribute("src")
    if (src) {
        const minlength = parseInt(
            input.getAttribute("value-minlength") ||
            datalist.getAttribute("value-minlength") ||
            "1"
        )
        const fetchOptions = () => {

            if (input.value.length < minlength) {
                return
            }

            const lowerCaseValue = input.value.toLowerCase()
            const options = Array.from(datalist.querySelectorAll("option"))

            // First look for exact match on value
            for (let i in options) {
                let option = options[i]
                if (option.value == input.value) {
                    // Do nothing if an exact match is found
                    return
                }
                else if (option.value.toLowerCase() == lowerCaseValue) {
                    // Fix casing if that is the only difference
                    input.value = option.value
                    input.dispatchEvent(new Event("input"))
                    input.dispatchEvent(new Event("change"))
                    return
                }
            }

            request({ url: src + input.value })
                .then(data => {
                    /** @type {[{value: string, label: string | null, title: string | null}]} */
                    const dataOptions = JSON.parse(data);
                    dataOptions.forEach(dataItem => {

                        const options = Array.from(datalist.querySelectorAll("option"))
                        const matches = options.filter((opt) => opt.value == dataItem.value && opt.label == (dataItem.label || dataItem.value))

                        if (matches.length <= 0) {
                            const option = document.createElement("option")
                            option.value = dataItem.value
                            option.label = dataItem.label || dataItem.value
                            option.title = dataItem.title
                            datalist.append(option)
                        }
                    })
                })
                .catch(error => { console.error(error) })
        }

        const debouncedFetchOptions = debounce(fetchOptions);
        input.addEventListener("input", debouncedFetchOptions)
        input.addEventListener("focus", debouncedFetchOptions)
        debouncedFetchOptions()
    }
}
autocompleteWithDatalist.initialized = "autocompleteWithDatalist.initialized"
