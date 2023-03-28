const inject_options = {
    maps: {
        "autosize": "autosize/autosize.min.js",
        "jquery": "https://cdn.jsdelivr.net/npm/jquery@3.6.0/dist/jquery.min.js",
        "vue": "apps/vue.js",
        "bootstrap": async () => {
            await inject("jquery")
            await inject("bootstrap/js/bootstrap.bundle.min.js")
        }
    }
};