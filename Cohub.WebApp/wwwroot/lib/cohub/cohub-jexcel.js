/**
 * @param {HTMLDivElement} div
 * @param {object} options See https://jexcel.net/v7/examples/custom-formulas
 */
const _jexcel = async (div, options) => {

    for (var col of options.columns) {
        if (col.options && col.options.remoteSearch && col.name && !col.source && !col.url) {
            col.source = await (
                await fetch(col.options.url + '?' + Array.from(options.data)
                    .filter(val => val[col.name])
                    .map(val => "id=" + encodeURIComponent(val[col.name]))
                    .join("&")
                )
            ).json()
        }
    }

    options.license = "OWRiN2Q5YjFjOTk5Nzc2ZDZjNDM3NjE5ODBiMzgyMWIzZjIyZTYxNjNhOTJiMDNkOWI2OGZiNGIxZmQ5Y2Q4NzczN2IyMTQ3NGY0NThjMmVhZmJhYjE3ZGVhNjZiNzVmZTI4Nzk4MjhiZjE1MGZkNDEzOGViMmE3ZjMxMGMwNmEsZXlKdVlXMWxJam9pU21WeVpXMTVJRU52YjJzaUxDSmtZWFJsSWpveE5qRXdOalk0T0RBd0xDSmtiMjFoYVc0aU9sc2liRzlqWVd4b2IzTjBJaXdpYkc5allXeG9iM04wSWwwc0luQnNZVzRpT2pCOQ=="

    let table = jexcel(div, options);
}

export {
    _jexcel as jexcel
}