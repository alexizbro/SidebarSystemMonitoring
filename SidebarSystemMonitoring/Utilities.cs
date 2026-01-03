using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using SidebarSystemMonitoring.Framework;

namespace SidebarSystemMonitoring.Utilities
{
    public static class Culture
    {
        public const string DEFAULT = "Default";

        public static void SetDefault()
        {
            Default = Thread.CurrentThread.CurrentUICulture;
        }

        public static void SetCurrent(bool init)
        {
            Resources.Culture = CultureInfo;

            Thread.CurrentThread.CurrentCulture = CultureInfo;
            Thread.CurrentThread.CurrentUICulture = CultureInfo;

            if (init)
            {
                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));
            }
        }

        public static CultureItem[] GetAll()
        {
            return new CultureItem[1] { new CultureItem() { Value = DEFAULT, Text = Resources.SettingsLanguageDefault } }.Concat(CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(c => Languages.Contains(c.TwoLetterISOLanguageName)).OrderBy(c => c.DisplayName).Select(c => new CultureItem() { Value = c.Name, Text = c.DisplayName })).ToArray();
        }

        public static string[] Languages
        {
            get
            {
                return new string[11] { "en", "da", "de", "fr", "ja", "nl", "zh", "it", "ru", "fi", "es" };
            }
        }

        public static CultureInfo Default { get; private set; }

        public static CultureInfo CultureInfo
        {
            get
            {
                string culture = Framework.Settings.Instance.Culture;
                return string.Equals(culture, DEFAULT, StringComparison.Ordinal)
                    ? Default
                    : new CultureInfo(culture);
            }
        }
    }

    public class CultureItem
    {
        public string Value { get; set; }

        public string Text { get; set; }
    }
}
