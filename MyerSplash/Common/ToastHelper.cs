using System.Text;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Notifications;

namespace MyerSplash.Common
{
    public static class ToastHelper
    {
        public static ToastNotification CreateToastNotification(string title, string content)
        {
            string xml = $@"<toast activationType='foreground'>
                                            <visual>
                                                <binding template='ToastGeneric'>
                                                </binding>
                                            </visual>
                                        </toast>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            var binding = doc.SelectSingleNode("//binding");

            var el = doc.CreateElement("text");
            el.InnerText = title;

            binding.AppendChild(el);

            el = doc.CreateElement("text");
            el.InnerText = content;
            binding.AppendChild(el);

            return new ToastNotification(doc);
        }
    }
}
