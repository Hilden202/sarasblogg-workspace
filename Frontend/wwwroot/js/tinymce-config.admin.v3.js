console.log("🔥 TINYMCE CONFIG – BEARER TOKEN VERSION 🔥", new Date().toISOString());
function initTinyMCE(selector, options = {}) {
    const baseOptions = {
        menubar: true,
        branding: false,
        promotion: false,
        license_key: 'gpl',
        height: 550,
        plugins:
            "lists link image table code advlist autolink charmap preview anchor " +
            "searchreplace visualblocks fullscreen insertdatetime media wordcount",
        toolbar:
            "undo redo | fontfamily fontsize | forecolor backcolor | highlight | " +
            "bold italic underline strikethrough | alignleft aligncenter alignright alignjustify | " +
            "bullist numlist outdent indent | blockquote | link image code removeformat",
        // 📱 Tvinga alltid fullständig desktop-toolbar – förhindra mobilläge
        toolbar_mode: 'wrap',
        mobile: { toolbar_mode: 'wrap' },
        block_formats: "Paragraph=p; Heading 1=h1; Heading 2=h2; Heading 3=h3",
        convert_urls: false,

        // 📚 Typsnitt
        font_family_formats:
            "Cormorant Garamond=Cormorant Garamond,serif;" +
            "Roboto=Roboto,Arial,sans-serif;" +
            "Lato=Lato,Arial,sans-serif;" +
            "Merriweather=Merriweather,Georgia,serif;" +
            "Montserrat=Montserrat,Arial,sans-serif;" +
            "Arial=arial,helvetica,sans-serif;" +
            "Georgia=georgia,serif;" +
            "Times New Roman='Times New Roman',times,serif;" +
            "Verdana=verdana,geneva,sans-serif;" +
            "Courier New='Courier New',courier,monospace;" +
            "Lucida Handwriting='Lucida Handwriting',cursive;",

        // 🌐 Ladda Google Fonts inuti TinyMCE-editorn
        content_css: "https://fonts.googleapis.com/css2?family=Roboto:wght@400;700&family=Lato:wght@400;700&family=Merriweather:wght@400;700&family=Montserrat:wght@400;700&display=swap",

        // 🔠 Storlekar
        font_size_formats:
            "8pt 9pt 10pt 11pt 12pt 14pt 16pt 18pt 24pt 36pt 48pt 72pt 96pt 120pt",

        // 🎨 Utökad färgpalett + highlights
        color_map: [
            "7e6655", "Brun (Hjärtat)",
            "a87363", "Koppar",
            "c48a7d", "Rosa",
            "b87333", "Bronze",
            "d4af37", "Guld",
            "fceea7", "Highlight – Ljusgul",
            "ffe5ec", "Highlight – Rosa",
            "cde8d5", "Highlight – Ljusgrön",
            "e0f0ff", "Highlight – Ljusblå",
            "ffffff", "Vit",
            "000000", "Svart",
            "708090", "Slate Gray"
        ],

        color_picker_callback: function (callback, value) {
            const input = document.createElement('input');
            input.setAttribute('type', 'color');
            input.setAttribute('value', value || '#fceea7');
            input.addEventListener('change', () => callback(input.value));
            input.click();
        },

        // ✨ Setup – lägger till Highlight-menyn
        setup: (editor) => {
            editor.ui.registry.addMenuButton('highlight', {
                text: 'Highlight',
                icon: 'highlight-bg-color',
                fetch: (callback) => {
                    const items = [
                        { type: 'menuitem', text: 'Gul', onAction: () => editor.execCommand('mceApplyTextcolor', false, '#fceea7', 'hilitecolor') },
                        { type: 'menuitem', text: 'Rosa', onAction: () => editor.execCommand('mceApplyTextcolor', false, '#ffe5ec', 'hilitecolor') },
                        { type: 'menuitem', text: 'Grön', onAction: () => editor.execCommand('mceApplyTextcolor', false, '#cde8d5', 'hilitecolor') },
                        { type: 'menuitem', text: 'Blå', onAction: () => editor.execCommand('mceApplyTextcolor', false, '#e0f0ff', 'hilitecolor') },
                        { type: 'menuitem', text: 'Koppar', onAction: () => editor.execCommand('mceApplyTextcolor', false, '#a87363', 'hilitecolor') },
                        { type: 'menuitem', text: 'Ta bort', onAction: () => editor.execCommand('RemoveTextcolor', false, 'hilitecolor') }
                    ];
                    callback(items);
                }
            });

            editor.on("change input undo redo", () => editor.save());
        },

        // 🪶 Stil för bloggtexten i editorn
        content_style: `
          body { font-family: 'Cormorant Garamond', serif; color: #7e6655; line-height: 1.6; font-size: 16px; }
          h1, h2, h3 { font-family: 'Cormorant SC', serif; color: #7e6655; text-transform: uppercase; letter-spacing: 0.05em; }
          a { color: #a87363; text-decoration: underline; }
          blockquote { border-left: 4px solid #c48a7d; margin: 1.5rem 0; padding: .75rem 1.25rem; background: #fdf3eb; }
          ul, ol { padding-left: 1.5rem; }
        `,

        // 📐 Tvinga inline-stilar för justering (text-align) istället för CSS-klasser
        formats: {
            alignleft:    { block: 'p', styles: { 'text-align': 'left' } },
            aligncenter:  { block: 'p', styles: { 'text-align': 'center' } },
            alignright:   { block: 'p', styles: { 'text-align': 'right' } },
            alignjustify: { block: 'p', styles: { 'text-align': 'justify' } }
        },

        // ✅ Tillåt text-align i sparad HTML
        valid_styles: {
            '*': 'text-align,font-family,font-size,color,background-color,font-weight,font-style,text-decoration'
        },

        ...options
    };

    tinymce.init({ selector, ...baseOptions });
}

window.addEventListener("load", () => {
    // 🔄 Synka TinyMCE-innehåll till textarea innan formulär skickas
    document.querySelectorAll("form").forEach(form => {
        form.addEventListener("submit", () => {
            if (window.tinymce) {
                tinymce.triggerSave();
            }
        });
    });

    // 🧭 Admin blog editor modal – initiera TinyMCE när modalen öppnas
    const blogModalEl = document.getElementById("blogEditorModal");
    if (blogModalEl && document.querySelector("#ContentEditor")) {
        blogModalEl.addEventListener("shown.bs.modal", () => {
            if (tinymce.get("ContentEditor")) return; // redan initierad

            console.log("✅ Initierar TinyMCE för Admin (modal shown)...");

            initTinyMCE("#ContentEditor", {
                plugins: "lists link image table code",
                toolbar:
                    "undo redo | fontfamily fontsize | forecolor backcolor | highlight | bold italic underline strikethrough | " +
                    "alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | blockquote | link image code removeformat",
                images_upload_handler: async (blobInfo) => {
                    const form = blogModalEl.querySelector('#blogForm');
                    const apiBase = (document.documentElement.dataset.apiBaseUrl || "").replace(/\/+$/, "");
                    const editorToken = form?.dataset?.editorToken?.trim();
                    const bloggId = form.querySelector("input[name='NewBlogg.Id']")?.value || 0;

                    const uploadUrl = `${apiBase}/api/editor/upload-image?bloggId=${bloggId}`;
                    const formData = new FormData();
                    formData.append("file", blobInfo.blob(), blobInfo.filename());

                    const isLocal =
                        location.hostname === "localhost" ||
                        location.hostname === "127.0.0.1";

                    const fetchOptions = {
                        method: "POST",
                        body: formData
                    };

                    if (isLocal) {
                        // 🔧 Lokal dev: cookie-auth
                        fetchOptions.credentials = "include";
                    } else {
                        // 🔐 Prod: Bearer token
                        fetchOptions.headers = {
                            "Authorization": `Bearer ${editorToken}`
                        };
                    }

                    const response = await fetch(uploadUrl, fetchOptions);

                    if (!response.ok) {
                        const err = await response.text();
                        throw new Error(`Uppladdning misslyckades (${response.status}): ${err}`);
                    }

                    const json = await response.json();
                    return json.location;
                },
            });
        });

        blogModalEl.addEventListener("hidden.bs.modal", () => {
            const editor = tinymce.get("ContentEditor");
            if (editor) editor.remove();
        });
    }

    // 🌸 About Me editor modal – initiera TinyMCE när modalen öppnas
    const aboutModalEl = document.getElementById("aboutMeFormModal");
    if (aboutModalEl && document.querySelector("#AboutMeEditor")) {
        aboutModalEl.addEventListener("shown.bs.modal", () => {
            if (tinymce.get("AboutMeEditor")) return; // redan initierad

            console.log("✅ Initierar TinyMCE för About Me (modal shown)...");
            initTinyMCE("#AboutMeEditor", {
                height: 400 // lägre höjd i modalen
            });
        });

        aboutModalEl.addEventListener("hidden.bs.modal", () => {
            const editor = tinymce.get("AboutMeEditor");
            if (editor) editor.remove();
        });
    }
});
