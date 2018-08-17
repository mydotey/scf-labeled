using System;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * Jun 19, 2018
     */
    public class TestDataCenterSetting : ICloneable
    {
        public const String DC_KEY = "dc";
        public const String APP_KEY = "app";

        private String key;
        private String value;

        private String dc;
        private String app;

        public TestDataCenterSetting(String key, String value, String dc, String app)
        {
            this.key = key;
            this.value = value;
            this.dc = dc;
            this.app = app;
        }

        public String getKey()
        {
            return key;
        }

        public void setKey(String key)
        {
            this.key = key;
        }

        public String getValue()
        {
            return value;
        }

        public void setValue(String value)
        {
            this.value = value;
        }

        public String getDc()
        {
            return dc;
        }

        public void setDc(String dc)
        {
            this.dc = dc;
        }

        public String getApp()
        {
            return app;
        }

        public void setApp(String app)
        {
            this.app = app;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((app == null) ? 0 : app.GetHashCode());
            result = prime * result + ((dc == null) ? 0 : dc.GetHashCode());
            result = prime * result + ((key == null) ? 0 : key.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            TestDataCenterSetting setting = (TestDataCenterSetting)obj;

            if (!object.Equals(key, setting.key))
                return false;

            if (!object.Equals(dc, setting.dc))
                return false;

            if (!object.Equals(app, setting.app))
                return false;

            return true;
        }

        public override string ToString()
        {
            return string.Format("{0} {{ key: {0}, value: {1}, dc: {2}, app: {3} }}", GetType().Name, key, value, dc,
                    app);
        }
    }
}