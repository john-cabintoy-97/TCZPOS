using PdfSharpCore.Fonts;
using PdfSharpCore.Internal;
using System.Reflection;

namespace TCZPOS.Components.Extension
{
    public class MyFontResolver : IFontResolver
    {
        public string DefaultFontName => "OpenSans";

        public byte[] GetFont(string faceName)
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("OpenSans-Regular.ttf").GetAwaiter().GetResult();
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo("OpenSans-Regular.ttf");
        }
    }
}
