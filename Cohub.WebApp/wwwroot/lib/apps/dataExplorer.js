/**
 * @param {HTMLButtonElement} button
 * @returns {Promise}
 */
export async function addFilter(button) {
    /** @type {HTMLTextAreaElement} */
    const whereFilter = document.getElementById(button.getAttribute('data-where-filter-id'));
    const columnTemplate = button.getAttribute('data-column-template');
    button.addEventListener("click", (ev) => {
        if (whereFilter.value.trim()) {
            whereFilter.value += "\nAND ";
        }
        whereFilter.value += columnTemplate;
        autosize.update(whereFilter);
    });
}