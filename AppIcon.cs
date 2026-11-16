using System.Drawing;
using System.IO;
using System.Reflection;

namespace InterfaceTeste
{
    internal static class AppIcon
    {
        public static Icon Carregar()
        {
            Assembly assembly = typeof(AppIcon).Assembly;

            using (Stream stream = assembly.GetManifestResourceStream("InterfaceTeste.Gestao.ico"))
            {
                return stream == null ? null : new Icon(stream);
            }
        }
    }
}
