using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    internal static class XmlUtilities {
        /// <summary>
        /// Writes the document to file using UTF-8 encoding with no BOM.
        /// </summary>
        /// <param name="xmlDocument">
        /// The <see cref="XmlDocument"/> to write.
        /// </param>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        /// <param name="reindent">
        /// Whether to re-indent the XML before writing.
        /// </param>
        public static void SaveAsUtf8(this XmlDocument xmlDocument, string filePath, bool reindent = false) {
            UTF8Encoding utf8EncodingNoBom = new UTF8Encoding(false);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = utf8EncodingNoBom; // Do not emit the BOM

            if (!reindent) {
                using (XmlWriter xmlWriter = XmlWriter.Create(filePath, settings)) {
                    xmlDocument.Save(xmlWriter);
                }
            } else {
                XElement element = XElement.Parse(xmlDocument.InnerXml);
                settings.Indent = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(filePath, settings)) {
                    element.Save(xmlWriter);
                }
            }
        }
    }
}
