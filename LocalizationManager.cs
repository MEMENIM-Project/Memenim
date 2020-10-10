using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;

namespace AnonymDesktopClient.Core
{
    public static class LocalizationManager
    {
        // TODO: Make something more elegant
        public static FrameworkElement MainWindow;

        /// <summary>  
        /// Get application name from an element  
        /// </summary>  
        /// <param name="element"></param>  
        /// <returns></returns>  
        private static string GetAppName(FrameworkElement element)
        {
            var elType = element.GetType().ToString();
            var elNames = elType.Split('.');
            return elNames[0];
        }

        /// <summary>  
        /// Generate a name from an element base on its class name  
        /// </summary>  
        /// <param name="element"></param>  
        /// <returns></returns>  
        private static string GetElementName(FrameworkElement element)
        {
            var elType = element.GetType().ToString();
            var elNames = elType.Split('.');
            var elName = "";
            if (elNames.Length >= 2) elName = elNames[elNames.Length - 1];
            return elName;
        }

        /// <summary>  
        /// Get current culture info name base on previously saved setting if any,  
        /// otherwise get from OS language  
        /// </summary>  
        /// <param name="element"></param>  
        /// <returns></returns>  
        public static string GetCurrentCultureName(FrameworkElement element)
        {
            // TODO: Add localization loading from settings
            var cultureName = CultureInfo.CurrentUICulture.Name;
            return cultureName;
        }

        /// <summary>  
        /// Set language based on previously save language setting,  
        /// otherwise set to OS lanaguage  
        /// </summary>  
        /// <param name="element"></param>  
        public static void SetDefaultLanguage(FrameworkElement element)
        {
            SetLanguageResourceDictionary(element, GetLocXAMLFilePath(GetElementName(element), GetCurrentCultureName(element)));
        }

        /// <summary>  
        /// Dynamically load a Localization ResourceDictionary from a file  
        /// </summary>  
        public static void SwitchLanguage(FrameworkElement element, string inFiveCharLang)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(inFiveCharLang);
            SetLanguageResourceDictionary(element, GetLocXAMLFilePath(GetElementName(element), inFiveCharLang));
            // TODO: Add localization saving
        }

        /// <summary>  
        /// Returns the path to the ResourceDictionary file based on the language character string.  
        /// </summary>  
        /// <param name="inFiveCharLang"></param>  
        /// <returns></returns>  
        public static string GetLocXAMLFilePath(string element, string inFiveCharLang)
        {
            string locXamlFile = element + "." + inFiveCharLang + ".xaml";
            string directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(directory, "Localization", locXamlFile);
        }

        /// <summary>  
        /// Sets or replaces the ResourceDictionary by dynamically loading  
        /// a Localization ResourceDictionary from the file path passed in.  
        /// </summary>  
        /// <param name="inFile"></param>  
        private static void SetLanguageResourceDictionary(FrameworkElement element, String inFile)
        {
            if (File.Exists(inFile))
            {
                // Read in ResourceDictionary File  
                var languageDictionary = new ResourceDictionary();
                languageDictionary.Source = new Uri(inFile);
                // Remove any previous Localization dictionaries loaded  
                int langDictId = -1;
                for (int i = 0; i < element.Resources.MergedDictionaries.Count; i++)
                {
                    var md = element.Resources.MergedDictionaries[i];
                    // Make sure your Localization ResourceDictionarys have the ResourceDictionaryName  
                    // key and that it is set to a value starting with "Loc-".  
                    if (md.Contains("ResourceDictionaryName"))
                    {
                        if (md["ResourceDictionaryName"].ToString().StartsWith("Loc-"))
                        {
                            langDictId = i;
                            break;
                        }
                    }
                }
                if (langDictId == -1)
                {
                    // Add in newly loaded Resource Dictionary  
                    element.Resources.MergedDictionaries.Add(languageDictionary);
                }
                else
                {
                    // Replace the current langage dictionary with the new one  
                    element.Resources.MergedDictionaries[langDictId] = languageDictionary;
                }
            }
            else
            {
                DialogManager.ShowDialog("F U C K","'" + inFile + "' not found.");
            }
        }
    }
}
