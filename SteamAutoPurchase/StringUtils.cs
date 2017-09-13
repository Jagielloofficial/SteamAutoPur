namespace TestBed
{
    public static class StringUtils
    {
        public static string ChangeDescriptionForSite(this string str)
        {
            if (str.StartsWith(" "))
            {
                str = str.TrimStart(' ');
            }

            if (str.EndsWith(" "))
            {
                str = str.TrimEnd(' ');
            }

            if (str.Contains(" "))
            {
                str = str.Replace(" ", "+");
            }

            if (str.Contains("("))
            {
                str = str.Replace("(", "%28");
            }

            if (str.Contains(")"))
            {
                str = str.Replace(")", "%29");
            }

            if (str.Contains("'"))
            {
                str = str.Replace("'", "%27");
            }

            return str;
        }

        public static string ChangeItemNameForSite(this string str)
        {
            return str.Replace(" ", "%20");
        }
    }
}