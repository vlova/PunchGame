using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PunchGame.Client.Ui
{
    class Utils
    {
        public static List<string> ReadManifestData<TSource>(string embeddedFileName) where TSource : class
        {
            embeddedFileName = embeddedFileName.Replace("\\", ".");
            var assembly = typeof(TSource).GetTypeInfo().Assembly;
            var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(embeddedFileName, StringComparison.CurrentCultureIgnoreCase));

            if (resourceName == null)
            {
                throw new InvalidOperationException($"Failed to find {embeddedFileName}");
            }

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Could not load manifest resource stream.");
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd().Replace("\r\n", "\n").Split('\n').ToList();
                }
            }
        }
    }
}
