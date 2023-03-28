
// Constants
const modalElementName = "site-panel"

document.addEventListener("keyup", (ev) => {
    if (ev.key == "Escape") {
        if (top != self) {
            location.href = "about:blank"
        }
        else {
            for (var button of document.getElementsByClassName("iframe-panel-close")) {
                button.click()
            }
        }
    }
})

// Iframe behaviors
if (top != self) {

    let url = new URL(location.href)

    const sk_intent = url.searchParams.get("sk_intent")
    if (sk_intent == "dismiss") {
        // Dismiss the modal
        location.href = "about:blank"
    }
    else if (sk_intent == "zoom") {
        // Zoom into the modal
        url.searchParams.delete("sk_intent")
        top.location.href = location.href
    }

    // Redirect the top window based on sk_top parameter
    // so that the top window refreshes and the modal URL stays the same.
    // Avoid redirecting to another site by comparing origins.
    const sk_top = url.searchParams.get("sk_top")
    if (sk_top) {
        let topUrl = new URL(sk_top, url)
        if (topUrl.origin == top.location.origin) {
            // The modal URL will be this iframe's URL without the sk_top parameter
            url.searchParams.delete("sk_top")
            topUrl.searchParams.append("sk_modal", url.pathname + url.search + url.hash)

            top.location.href = topUrl.href
        }
    }

    window.addEventListener("click", (ev) => {
        if (ev.target.tagName == "A") {
            /** @type HTMLAnchorElement */
            const a = ev.target
            if (a.getAttribute("data-intent") == "dismiss") {
                location.href = "about:blank"
                ev.preventDefault()
            }
        }
    })
}

document.addEventListener("readystatechange", () => {

    // Main window behaviors
    if (top == self) {

        const url = new URL(location.href),
            modalContainer = document.querySelector(".iframe-panel-container"),
            modalName = modalContainer.getAttribute("iframe-name")

        let modal = document.createElement("iframe")
        modal.name = modalName
        modalContainer.append(modal)

        // If an sk_modal search parameter is present redirect the modal to it 
        const sk_modal = url.searchParams.get("sk_modal")
        if (sk_modal) {
            let modalUrl = new URL(sk_modal, url)
            if (modalUrl.origin == location.origin) {
                /** @type HTMLIFrameElement */
                modal.contentWindow.location.href = modalUrl.href
            }
        }

        window.addEventListener("click", (ev) => {
            if (ev.target.tagName == "A") {
                /** @type HTMLAnchorElement */
                const a = ev.target

                // Redirect modal links to the modal
                // TODO: Change data-target=modal to data-intent=modal or data-intent=popup
                if (a.getAttribute("data-target") == "modal") {
                    /** @type HTMLIFrameElement */
                    modal.contentWindow.location.href = a.href
                    ev.preventDefault()
                }
            }
        })

        document.querySelectorAll(".iframe-panel").forEach(
            /** @param {HTMLElement} panel */
            (panel) => {

                const header = panel.querySelector(".iframe-panel-header")
                const title = panel.querySelector(".iframe-panel-title")
                const closeButton = panel.querySelector(".iframe-panel-close")
                const minimizeButton = panel.querySelector(".iframe-panel-minimize")
                const maximizeButton = panel.querySelector(".iframe-panel-maximize")
                const iframe = panel.getElementsByTagName("iframe")[0]

                let lastHref = iframe.contentWindow.location.href

                iframe.addEventListener("load", (ev) => {
                    const iframeHref = iframe.contentWindow.location.href

                    if (iframeHref && iframeHref != "about:blank") {
                        panel.classList.add("open")
                        panel.classList.remove("closed")
                    }
                    else {
                        panel.classList.add("closed")
                        panel.classList.remove("open")
                    }

                    title.textContent = iframe.contentWindow.document.title

                    if (lastHref != iframeHref) {
                        const topUrl = new URL(location.href)
                        if (iframeHref != "about:blank") {
                            topUrl.searchParams.set("sk_modal", iframeHref)
                            history.pushState(null, null, topUrl.href)
                        }
                        else if (topUrl.searchParams.get("sk_modal")) {
                            topUrl.searchParams.delete("sk_modal")
                            history.pushState(null, null, topUrl.href)
                        }
                    }

                    lastHref = iframeHref;
                })

                minimizeButton.addEventListener("click", (ev) => {
                    panel.classList.toggle("minimized")
                })

                maximizeButton.addEventListener("click", (ev) => {
                    top.location.href = iframe.contentWindow.location.href
                })

                closeButton.addEventListener("click", (ev) => {
                    iframe.src = "about:blank"
                })
            }
        )
    }

    // Iframe behavior
    else {
        /** @type {HTMLLabelElement} */
        const autofocus = document.querySelector("label[autofocus]")
        if (autofocus && autofocus.htmlFor)
            setTimeout(() => document.getElementById(autofocus.htmlFor).focus(), 1)
    }
})
