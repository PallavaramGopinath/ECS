using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Dto
{
    public class MenuIcons
    {
        public const string DefaultMain = "bi bi-collection-fill";
        public const string DefaultSub = "bi bi-folder-fill";
        public const string DefaultForm = "bi bi-file-earmark-text";
        public const string DefaultLink = "bi bi-link-45deg";

        public static string GetIcon(string? customIcon, string defaultIcon)
        {
            return string.IsNullOrEmpty(customIcon) ? defaultIcon : customIcon;
        }
    }
}
