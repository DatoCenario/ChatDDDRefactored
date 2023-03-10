using System.Text.RegularExpressions;

namespace Chatiks.Core.Extensions;

public static class ImageTools
{
    private static Regex ReplaceImageHeaderReg = new Regex(@"^data:image\/(png|jpg);base64,");
    
    public static string ReplaceImageHeader(string base64imageText)
    {
        return ReplaceImageHeaderReg.Replace(base64imageText, "");
    }
}