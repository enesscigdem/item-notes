using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace ItemNotes.UI.Controls
{
    // WebView.Avalonia: Navigate/Source yok; HTML'i JS ile document.write ederek yükleyeceğiz.
    public partial class CkEditorView : UserControl
    {
        private bool _editorReady;

        /// <summary> Editör hazır olmadan set edilirse buffer'lanır. </summary>
        public string? InitialHtml { get; set; }

        public CkEditorView()
        {
            InitializeComponent();
            AttachedToVisualTree += OnAttachedToVisualTree;
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            // 1) CKEditor HTML'ini oluştur
            var html = BuildCkEditorHtml();

            // 2) about:blank içine document.write ile bas. Hazır olana kadar birkaç kez dene.
            for (int i = 0; i < 50; i++)
            {
                try
                {
                    await WriteHtmlAsync(html);
                    // 3) CKEditor hazır mı?
                    var ready = await EvalAsync("window.editorReady === true ? '1' : '0'");
                    if (ready == "1")
                    {
                        _editorReady = true;
                        if (!string.IsNullOrWhiteSpace(InitialHtml))
                            await SetHtmlAsync(InitialHtml!);
                        return;
                    }
                }
                catch
                {
                    // İlk saniyelerde WebView henüz hazır olmayabilir; bekleyip tekrar dene.
                }
                await Task.Delay(100);
            }

            // Yine de hazır olmadıysa en azından boş kur.
            await WriteHtmlAsync(html);
        }

        /// <summary> Editöre HTML set et. </summary>
        public async Task SetHtmlAsync(string html)
        {
            if (!_editorReady)
            {
                InitialHtml = html;
                return;
            }
            var json = JsonSerializer.Serialize(html);
            await EvalAsync($"window.editorApi && window.editorApi.setData({json});");
        }

        /// <summary> Editörden HTML al. </summary>
        public async Task<string> GetHtmlAsync()
        {
            if (!_editorReady) return InitialHtml ?? string.Empty;
            var result = await EvalAsync("window.editorApi && window.editorApi.getData()");
            return result ?? string.Empty;
        }

        /// <summary> JS çalıştır. </summary>
        private Task<string?> EvalAsync(string js) => EditorHost.ExecuteScriptAsync(js);

        /// <summary>
        /// Mevcut belgeyi base64 HTML ile değiştirir (navigate gerektirmez).
        /// </summary>
        private Task<string?> WriteHtmlAsync(string html)
        {
            var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(html));
            var js = $"(function(){{document.open();document.write(atob('{b64}'));document.close();}})();";
            return EditorHost.ExecuteScriptAsync(js);
        }

        /// <summary> CKEditor 5 (super-build) host HTML. </summary>
        private static string BuildCkEditorHtml() => @"
<!DOCTYPE html>
<html lang=""tr"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<title>Editor</title>
<style>
  html,body,#app{height:100%;margin:0;}
  .ck-editor__editable_inline{ min-height: 400px; }
  body { background:#fff; font-family: system-ui, -apple-system, Segoe UI, Roboto, Inter, sans-serif; }
</style>
<script src=""https://cdn.ckeditor.com/ckeditor5/43.2.0/super-build/ckeditor.js""></script>
<script src=""https://cdn.ckeditor.com/ckeditor5/43.2.0/super-build/translations/tr.js""></script>
</head>
<body>
  <div id=""app""><div id=""editor""><p></p></div></div>

<script>
  window.editorReady = false;

  window.editorApi = {
    setData: function(html){ window.editor && window.editor.setData(html || ''); },
    getData: function(){ return window.editor ? window.editor.getData() : ''; },
    focus:   function(){ if(window.editor){ window.editor.editing.view.focus(); } }
  };

  (function(){
    CKEDITOR.ClassicEditor.create(document.getElementById('editor'), {
      language: 'tr',
      toolbar: {
        items: [
          'undo','redo','|',
          'selectAll','|',
          'heading','fontSize','|',
          'bold','italic','underline','strikethrough','|',
          'link','|',
          'bulletedList','numberedList','outdent','indent','|',
          'insertTable','|',
          'imageUpload','blockQuote','removeFormat'
        ],
        shouldNotGroupWhenFull: true
      },
      fontSize: { options: [ '10','12','14','16','18','20','24','28' ], supportAllValues: true },
      link: { addTargetToExternalLinks: true, defaultProtocol: 'https://' },
      list: { properties: { styles: true, startIndex: true, reversed: true } },
      image: {
        toolbar: [ 'imageTextAlternative','toggleImageCaption','|','imageStyle:inline','imageStyle:wrapText','imageStyle:breakText','|','linkImage' ],
        resizeUnit: '%'
      },
      table: { contentToolbar: [ 'tableColumn','tableRow','mergeTableCells' ], defaultHeadings: { rows:0, columns:0 } },
      removePlugins: [
        'CKBox','CKFinder','EasyImage','RealTimeCollaborativeComments','RealTimeCollaborativeTrackChanges',
        'RealTimeCollaborativeRevisionHistory','PresenceList','Comments','TrackChanges','TrackChangesData',
        'RevisionHistory','Pagination','WProofreader','SlashCommand','Template','DocumentOutline',
        'FormatPainter','MathType','CaseChange'
      ]
    }).then(function(ed){
      window.editor = ed;
      window.editorReady = true;
    }).catch(function(err){ console.error(err); });
  })();
</script>
</body>
</html>";
    }
}
