using System;
using System.Collections.Generic;
using L4p.Common.Extensions;

namespace L4p.WebApi
{
    public enum ELanguage
    {
    /*
        select count,user_language,language_name from (
        select u.user_language,count(*) as count from users u where partner_id=14
            and u.user_create_date>sysdate-180
            group by u.user_language) u,
            languages l
        where u.user_language=l.language_id
        order by 1 desc  
    */

        Default = 0, 
        En = 50,        // English
        Ar = 5,         // Arabic
        Ru = 98,        // Russian
        Fr = 58,        // France
        He = 71,        // Israel
        De = 65,        // Germany
        Es = 120,       // Spanish
        Cn = 31,        // Chinese
        Pt = 95         // Portuguese
    }

    public static class Language
    {
        private static readonly Dictionary<string, ELanguage> _languages =
        new Dictionary<string, ELanguage>(StringComparer.InvariantCultureIgnoreCase) {
            { "en", ELanguage.En },
            { "ar", ELanguage.Ar },
            { "ru", ELanguage.Ru },
            { "fr", ELanguage.Fr },
            { "he", ELanguage.He },
            { "de", ELanguage.De },
            { "es", ELanguage.Es },
            { "cn", ELanguage.Cn },
            { "pt", ELanguage.Pt },
        };

        public static int language_code_to_language_id(string countryCode)
        {
            int langId = (int) ELanguage.En;

            do
            {
                if (countryCode.IsEmpty())
                    break;

                ELanguage lang;

                if (!_languages.TryGetValue(countryCode, out lang))
                    break;

                langId = (int) lang;
            }
            while (false);

            return langId;
        }
    }
}