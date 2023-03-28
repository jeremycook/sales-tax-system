/**
 * 
 * @param {HTMLFormElement} form
 */
export async function mount(form) {
    form.addEventListener("submit", (ev) => {
        ev.preventDefault();
        alert("Canceled submit")
    })
}