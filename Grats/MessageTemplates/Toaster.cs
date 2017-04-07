using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Metadata;
using Windows.UI.Notifications;

namespace Grats.Notification
{
    class Toaster
    {
        public static ToastNotification PopToast(string title, string content)
        {
            return PopToast(title, content, null, null, null);
        }

        public static ToastNotification PopToast(string title, string content, string tag, string group, string img)
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
            var image = doc.CreateElement("image");
            image.SetAttribute("placement", "appLogoOverride");
            image.SetAttribute("hint-crop", "circle");
            image.SetAttribute("src", (img is null? "..\\..\\..\\Assets\\Logo.png" : img));
            var el = doc.CreateElement("text");
            el.InnerText = title;
            binding.AppendChild(el);
            el = doc.CreateElement("text");
            el.InnerText =content;
            binding.AppendChild(el);
            binding.AppendChild(image);
            return PopCustomToast(doc, tag, group);
        }

        public static ToastNotification PopCustomToast(string xml)
        {
            return PopCustomToast(xml, null, null);
        }

        public static ToastNotification PopCustomToast(string xml, string tag, string group)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);


            return PopCustomToast(doc, tag, group);
        }

        [DefaultOverloadAttribute]
        public static ToastNotification PopCustomToast(XmlDocument doc, string tag, string group)
        {
            var toast = new ToastNotification(doc);

            if (tag != null)
                toast.Tag = tag;

            if (group != null)
                toast.Group = group;

            ToastNotificationManager.CreateToastNotifier().Show(toast);

            return toast;
        }
    }
}
