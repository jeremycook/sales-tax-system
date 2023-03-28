const doc = document.documentElement,
    libpath = inject_options.libpath || ((doc.getAttribute("libpath") || "/lib").trimEnd("/") + "/"),
    buster = inject_options.buster || (doc.getAttribute("buster") ? "?" + doc.getAttribute("buster") : ""),
    maps = inject_options.maps || {}
/**
 * Import a JavaScript module like a bundler.
 * @param {string|string[]} modules
 * @returns {Promise}
 */
const inject = async (modulesNames) => {
    /** @type {string[]} */
    const stringInput = typeof modulesNames == "string",
        modules = stringInput ? [modulesNames] : modulesNames,
        results = [modules.length];

    let resolvedModules = 0;

    const promise = new Promise((resolved, rejectionFunc) => {
        modules.forEach((module, i) => {
            for (const key in maps) {
                if (module == key) {
                    const value = maps[key];
                    if (typeof value == "string") {
                        const importPath =
                            value.startsWith("/") || value.startsWith("https:") ? value :
                                module.indexOf(".") > -1 ? libpath + value + module.substring(prefix.length) + buster :
                                    libpath + value + buster;
                        import(importPath).then((returnVal) => {
                            results[i] = returnVal
                            resolvedModules++;
                            if (resolvedModules == results.length)
                                resolved(stringInput ? results[0] : results)
                        })
                    }
                    else if (typeof value == "function") {
                        value().then((returnVal) => {
                            results[i] = returnVal
                            resolvedModules++;
                            if (resolvedModules == results.length)
                                resolved(stringInput ? results[0] : results)
                        })
                    }
                    else {
                        throw new Error(`Unsupported typeof value '${typeof value}' of key '${key}'.`)
                    }
                    return
                }
            }

            // Fallback to URL relative to `libpath`
            import(libpath + module + buster).then((returnVal) => {
                results[i] = returnVal
                resolvedModules++;
                if (resolvedModules == results.length)
                    resolved(stringInput ? results[0] : results)
            })
        })
    });


    return promise;
}
if (window) window.inject = inject