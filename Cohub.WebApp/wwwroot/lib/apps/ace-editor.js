
ace.config.set('basePath', '/lib/ace');

document.querySelectorAll('[ace-editor]').forEach(function (element) {

    let editorDiv = document.createElement('div');
    element.parentElement.appendChild(editorDiv);

    let editor = ace.edit(editorDiv, {
        minLines: 2
    });

    editor.session.setValue(element.value);

    editor.session.on('change', function () {
        element.value = editor.session.getValue();
    });

    element.setAttribute('style', 'display: none');

    editor.setTheme('ace/theme/eclipse');

    let mode = element.getAttribute('ace-mode');
    editor.session.setMode(mode || 'ace/mode/text');

    editor.resize(true)
});
