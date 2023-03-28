//if (parent && parent.location.pathname == location.pathname) {
//    window.close()
//}

document.addEventListener("readystatechange", () => {

    document.querySelectorAll(".iframe-panel").forEach(
        /** @param {HTMLElement} panel */
        (panel) => {

            const header = panel.querySelector(".iframe-panel-header")
            const title = panel.querySelector(".iframe-panel-title")
            const closeButton = panel.querySelector(".iframe-panel-close")
            const iframe = panel.getElementsByTagName("iframe")[0]

            iframe.addEventListener("load", (ev) => {
                console.log("iframe.contentWindow.location.href:", iframe.contentWindow.location.href)
                if (iframe.contentWindow.location.href && iframe.contentWindow.location.href != "about:blank") {
                    panel.classList.add("active")
                }
                else {
                    panel.classList.remove("active")
                }
                title.textContent = iframe.contentWindow.document.title
            })

            closeButton.addEventListener("click", (ev) => {
                iframe.src = "about:blank"
            })
        }
    )
})

//document.addEventListener("readystatechange", () => {

//    document.querySelectorAll("a[target^='.']").forEach(
//        /** @param {HTMLAnchorElement} element */
//        (element) => {

//            element.addEventListener("click", (ev) => {
//                /** @type {HTMLIFrameElement} */
//                let microModal = element.microModal;

//                if (!microModal) {
//                    const target = document.querySelector(element.target),
//                        href = element.getAttribute("href")

//                    microModal = document.createElement("iframe")
//                    microModal.src = href + (href.indexOf("?") > -1 ? "&" : "?") + "modal"
//                    target.appendChild(microModal)
//                    element.microModal = microModal;
//                    window.ope
//                    microModal.addEventListener("close", (ev) => {
//                        microModal.remove()
//                        delete element.microModal
//                    })
//                }
//                else {
//                    microModal.contentWindow.close()
//                }

//                ev.preventDefault()
//            })

//        }
//    )
//})