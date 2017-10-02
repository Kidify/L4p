using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.ConfigurationFiles
{
    interface IJson2Config<T>
        where T : class, new()
    {
        T ParseJson(string file, string json, ILogFile log = null);
    }

    class Json2Config<T> : IJson2Config<T> 
        where T : class, new()
    {
        #region configuration wrapper

        public class SingleConfiguration
        {
            public string[] PathKeys { get; set; }
            public T Configuration { get; set; }
        }

        class MultipleConfiguration
        {
            public SingleConfiguration[] Configurations { get; set; }
        }

        #endregion

        #region members

        private readonly string _path;
        private readonly string _hostname;

        #endregion

        #region construction

        public static IJson2Config<T> New(string path)
        {
            return
                new Json2Config<T>(path);
        }

        private Json2Config(string path)
        {
            _path = path;
            _hostname = Environment.MachineName;
        }

        #endregion

        #region private

        private static string remove_js_header(string json)
        {
            int firstBracketAt = json.IndexOf('{');
            Validate.That(firstBracketAt >= 0);

            if (firstBracketAt == 0)
                return json;

            return
                json.Substring(firstBracketAt);
        }

        private static string remove_js_comments(string json)
        {
            // from http://stackoverflow.com/a/3524689/675116

            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            var noComments = Regex.Replace(json,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                me => {
                    if (me.Value.StartsWith("//"))
                        return Environment.NewLine;
                    if (me.Value.StartsWith("/*"))
                        return "";
                    // Keep the literal strings
                    return me.Value;
                },
                RegexOptions.Singleline);

            return noComments;
        }

        private static TStruct parse_json<TStruct>(string json)
        {
            var timeSpanConverter = new TimeSpanFromJsonConverter();
            var @struct = JsonConvert.DeserializeObject<TStruct>(json, timeSpanConverter);

            return @struct;
        }

        private void fail_if(bool expr, string msg, params object[] args)
        {
            if (expr == false)
                return;

            string hdr = "Bad configuration file '{0}': ".Fmt(_path);
            string errorMsg = hdr + msg.Fmt(args);

            throw new ConfigFileException(errorMsg);
        }

        private void validate_mconfig(MultipleConfiguration mconfig)
        {
            fail_if(mconfig.Configurations == null, "No configurations are found");
            fail_if(mconfig.Configurations.Length == 0, "Configurations are empty");
        }

        private void validate_sconfig(SingleConfiguration sconfig)
        {
            fail_if(sconfig.Configuration == null, "Single configuration is empty");
        }

        private MultipleConfiguration parse_as_mconfig(string json)
        {
            var mconfig = parse_json<MultipleConfiguration>(json);

            if (mconfig == null)
                return null;

            if (mconfig.Configurations == null)
                return null;

            validate_mconfig(mconfig);

            return mconfig;
        }

        private bool has_key_in_path(SingleConfiguration sconfig, string path, out string matchedKey)
        {
            matchedKey = null;

            foreach (var key in sconfig.PathKeys)
            {
                bool containsKey =
                    path.IndexOf(key, StringComparison.InvariantCultureIgnoreCase) != -1;

                containsKey |=
                    0 == String.Compare(key, _hostname, StringComparison.InvariantCultureIgnoreCase);

                if (containsKey == false)
                    continue;

                matchedKey = key;
                return true;
            }

            return false;
        }

        private T choose_sconfig(MultipleConfiguration mconfig, out string matchedKey)
        {
            matchedKey = null;

            foreach (var sconfig in mconfig.Configurations)
            {
                validate_sconfig(sconfig);

                if (sconfig.PathKeys == null)
                    return sconfig.Configuration;

                if (sconfig.PathKeys.Length == 0)
                    return sconfig.Configuration;

                bool hasKeyInPath = has_key_in_path(sconfig, _path, out matchedKey);

                if (hasKeyInPath == false)
                    continue;

                return 
                    sconfig.Configuration;
            }

            throw
                new ConfigFileException("Configuration file '{0}': has no config for its path", _path);
        }

        private static T parse_as_sconfig(string json)
        {
            var sconfig = parse_json<T>(json);
            return sconfig;
        }

        #endregion

        #region IJson2Config

        T IJson2Config<T>.ParseJson(string file, string json, ILogFile log)
        {
            T sconfig;

            json = remove_js_header(json);
            json = remove_js_comments(json);

            file = Path.GetFileName(file);

            var mconfig = parse_as_mconfig(json);

            if (mconfig != null)
            {
                string matchedKey;

                sconfig = choose_sconfig(mconfig, out matchedKey);

                if (log != null)
                    log.Trace("PathKey '{0}' is matched ({1})", matchedKey, file);

                return sconfig;
            }

            sconfig = parse_as_sconfig(json);
            
            if (log != null)
                log.Trace("Single configuration file ({0})", file);

            return sconfig;
        }

        #endregion
    }
}