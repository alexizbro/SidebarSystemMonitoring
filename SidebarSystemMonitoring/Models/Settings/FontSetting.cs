using Newtonsoft.Json;

namespace SidebarSystemMonitoring.Models.Settings;

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class FontSetting
    {
        internal FontSetting() { }

        private FontSetting(int fontSize)
        {
            FontSize = fontSize;
        }

        public override bool Equals(object obj)
        {
            FontSetting _that = obj as FontSetting;

            if (_that == null)
            {
                return false;
            }

            return this.FontSize == _that.FontSize;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static FontSetting x10
        {
            get
            {
                return new FontSetting(10);
            }
        }

        public static FontSetting x12
        {
            get
            {
                return new FontSetting(12);
            }
        }

        public static FontSetting x14
        {
            get
            {
                return new FontSetting(14);
            }
        }

        public static FontSetting x16
        {
            get
            {
                return new FontSetting(16);
            }
        }

        public static FontSetting x18
        {
            get
            {
                return new FontSetting(18);
            }
        }

        [JsonProperty]
        public int FontSize { get; set; }

        public int TitleFontSize
        {
            get
            {
                return FontSize + 2;
            }
        }

        public int SmallFontSize
        {
            get
            {
                return FontSize - 2;
            }
        }

        public int IconSize
        {
            get
            {
                switch (FontSize)
                {
                    case 10:
                        return 18;

                    case 12:
                        return 22;

                    case 14:
                    default:
                        return 24;

                    case 16:
                        return 28;

                    case 18:
                        return 32;
                }
            }
        }

        public int BarHeight
        {
            get
            {
                return FontSize - 3;
            }
        }

        public int BarWidth
        {
            get
            {
                return BarHeight * 6;
            }
        }

        public int BarWidthWide
        {
            get
            {
                return BarHeight * 8;
            }
        }
    }