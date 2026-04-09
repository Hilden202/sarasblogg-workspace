// Shared blog editor functions – used on both Blogg.cshtml and Admin/Index.cshtml.
// Pages must define bloggListData before this file is loaded.
// currentBloggData is optional (null if not defined).

function setEditorContent(content) {
    var editor = (typeof tinymce !== 'undefined') ? tinymce.get('ContentEditor') : null;
    if (editor) {
        editor.setContent(content || '');
    } else {
        var ta = document.getElementById('ContentEditor');
        if (ta) ta.value = content || '';
        // TinyMCE not yet initialised — apply content once it is ready
        if (typeof tinymce !== 'undefined') {
            tinymce.on('AddEditor', function onAdd(e) {
                if (e.editor.id === 'ContentEditor') {
                    e.editor.setContent(content || '');
                    tinymce.off('AddEditor', onAdd);
                }
            });
        }
    }
}

function openEditor(blogId) {
    var modalEl    = document.getElementById('blogEditorModal');
    var form       = modalEl.querySelector('#blogForm');
    var titleEl    = modalEl.querySelector('#blogEditorModalLabel');
    var imgSection = modalEl.querySelector('#editImagesSection');

    if (blogId) {
        var current = (typeof currentBloggData !== 'undefined' && currentBloggData && currentBloggData.id === blogId)
            ? currentBloggData
            : null;
        var data = current || ((typeof bloggListData !== 'undefined') ? bloggListData.find(function (b) { return b.id === blogId; }) : null) || null;

        if (!data) return;

        titleEl.textContent = 'Redigera blogg';
        form.querySelector('[name="NewBlogg.Id"]').value         = data.id;
        form.querySelector('[name="NewBlogg.UserId"]').value     = data.userId || '';
        form.querySelector('[name="NewBlogg.Title"]').value      = data.title || '';
        form.querySelector('[name="NewBlogg.Author"]').value     = data.author || '';
        form.querySelector('[name="NewBlogg.LaunchDate"]').value = data.launchDate || '';
        setEditorContent(data.content);
        if (imgSection) imgSection.style.display = '';

    } else {
        titleEl.textContent = 'Skapa nytt inlägg';
        form.querySelector('[name="NewBlogg.Id"]').value         = '0';
        form.querySelector('[name="NewBlogg.UserId"]').value     = '';
        form.querySelector('[name="NewBlogg.Title"]').value      = '';
        form.querySelector('[name="NewBlogg.Author"]').value     = '';
        form.querySelector('[name="NewBlogg.LaunchDate"]').value = new Date().toISOString().slice(0, 10);
        setEditorContent('');
        if (imgSection) imgSection.style.display = 'none';
    }

    bootstrap.Modal.getOrCreateInstance(modalEl).show();
}

function closeEditor() {
    var inst = bootstrap.Modal.getInstance(document.getElementById('blogEditorModal'));
    if (inst) inst.hide();
}
