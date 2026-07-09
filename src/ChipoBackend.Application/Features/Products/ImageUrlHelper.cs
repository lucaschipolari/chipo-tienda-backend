using System.Text.RegularExpressions;

namespace ChipoBackend.Application.Features.Products;

/// <summary>
/// Normaliza URLs de imágenes. Convierte links de Google Drive (incluido el de
/// "compartir") a una URL directa que se puede embeber en &lt;img src&gt;.
/// Cualquier otro link se devuelve tal cual.
/// </summary>
public static class ImageUrlHelper
{
    public static string Normalize(string url)
    {
        url = url.Trim();

        // Formatos soportados:
        //   https://drive.google.com/file/d/FILEID/view?usp=sharing
        //   https://drive.google.com/open?id=FILEID
        //   https://drive.google.com/uc?id=FILEID&export=...
        var m = Regex.Match(url, @"drive\.google\.com/file/d/([\w-]+)");
        if (!m.Success)
            m = Regex.Match(url, @"drive\.google\.com/(?:open|uc)\?(?:[^#]*&)?id=([\w-]+)");

        if (m.Success)
        {
            var fileId = m.Groups[1].Value;
            // El endpoint "thumbnail" es el más estable para embeber en <img>.
            return $"https://drive.google.com/thumbnail?id={fileId}&sz=w1200";
        }

        return url;
    }
}
