using UnityEngine;
using System.Collections.Generic;

public class Local_Manager
{
    public class String_Data
    {
        public string ko;
        public string en;
        public string ja;

        public string Get_Data(string lan= "")
        {
            if(lan != "")
            {
                switch(lan)
                {
                    case "ko": return ko;
                    case "en": return en;
                    case "ja": return ja;
                }
            }
            switch(LocalAccess)
            {
                case "ko": return ko;
                case "en": return en;
                case "ja": return ja;
            }
            return "";
        }
    }

    public static string LocalAccess = "ko";

    public static Dictionary<string, String_Data> local_Data = new Dictionary<string, String_Data>();

    public void Init() => local_Data = dictionary();


    private static Dictionary<string, String_Data> dictionary()
    {
        var localiztion = CSV_Importer.Localization;
        Dictionary<string, String_Data> value = new Dictionary<string, String_Data>();
        for(int i =0; i< localiztion.Count; i++)
        {
            String_Data data = new String_Data();
            data.ko = localiztion[i]["ko"].ToString();
            data.en = localiztion[i]["en"].ToString();
            data.ja = localiztion[i]["ja"].ToString();

            string temp = localiztion[i]["Key"].ToString();

            value.Add(temp, data);
        }

        string localCheck = PlayerPrefs.GetString("LOCAL");


        if(localCheck == "" || localCheck == null)
        {
            SystemLanguage lang = Application.systemLanguage;
            switch(lang)
            {
                case SystemLanguage.Korean: LocalAccess ="ko"; break;
                case SystemLanguage.Japanese: LocalAccess = "ja"; break;
                default: LocalAccess = "en"; break;
            }
        }
        else
        {
            LocalAccess = localCheck;
        }


        return value;
    }

}
