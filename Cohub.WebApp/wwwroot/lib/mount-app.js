const elements = document.querySelectorAll("[mount-app]")
elements.forEach((element) => {
    const appName = element.getAttribute("mount-app")
    inject(appName).then(app => app.mount(element))
})