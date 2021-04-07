using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using RIS;

namespace Memenim.Localization.Entities
{
    public class LocalizationXamlModule
    {
        private readonly List<LocalizationXamlFile> _files;
        public ReadOnlyCollection<LocalizationXamlFile> Files
        {
            get
            {
                return new ReadOnlyCollection<LocalizationXamlFile>(
                    _files);
            }
        }

        public string ElementName { get; private set; }

        public ResourceDictionary Dictionary { get; }

        public CultureInfo Culture { get; private set; }
        public string CultureName { get; private set; }
        private string _cultureNativeName;
        public string CultureNativeName
        {
            get
            {
                return _cultureNativeName;
            }
            private set
            {
                if (value.Length > 1)
                {
                    value = char.ToUpper(value[0], Culture)
                            + value.Remove(0, 1);
                }

                _cultureNativeName = value;
            }
        }

        public LocalizationXamlModule(IEnumerable<string> filesPaths,
            string elementName)
        {
            _files = new List<LocalizationXamlFile>();
            Dictionary = new ResourceDictionary();

            Load(filesPaths.ToArray(),
                elementName);
        }

        private void ValidateFile(LocalizationXamlFile file)
        {
            if (_files.Count == 0)
            {
                ElementName = file.ElementName;
                Culture = file.Culture;
                CultureName = file.CultureName;
                CultureNativeName = file.CultureNativeName;

                return;
            }

            if (ElementName != file.ElementName)
            {
                var exception = new Exception(
                    $"Element name '{ElementName}' in base dictionary file['{_files[0].Path}'] is not equal to '{file.ElementName}' in dictionary file['{file.Path}']");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }
            if (!Equals(Culture, file.Culture))
            {
                var exception = new Exception(
                    $"Culture '{Culture}' in base dictionary file['{_files[0].Path}'] is not equal to '{file.Culture}' in dictionary file['{file.Path}']");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }
            if (CultureName != file.CultureName)
            {
                var exception = new Exception(
                    $"Culture name '{CultureName}' in base dictionary file['{_files[0].Path}'] is not equal to '{file.CultureName}' in dictionary file['{file.Path}']");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }
            if (CultureNativeName != file.CultureNativeName)
            {
                var exception = new Exception(
                    $"Culture native name '{CultureNativeName}' in base dictionary file['{_files[0].Path}'] is not equal to '{file.CultureNativeName}' in dictionary file['{file.Path}']");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
                throw exception;
            }
        }

        private void Load(string[] filesPaths,
            string elementName)
        {
            foreach (var filePath in filesPaths)
            {
                var file = new LocalizationXamlFile(
                    filePath, elementName);

                ValidateFile(file);

                _files.Add(file);
                Dictionary.MergedDictionaries.Add(file.Dictionary);
            }
        }

        public void Merge(string filePath)
        {
            Merge(new[] { filePath });
        }
        public void Merge(IEnumerable<string> filesPaths)
        {
            Load(filesPaths.ToArray(),
                ElementName);
        }

        public void Remove(string filePath)
        {
            Remove(new[] { filePath });
        }
        public void Remove(IEnumerable<string> filesPaths)
        {
            var filePaths = filesPaths.ToArray();

            foreach (var file in Files)
            {
                foreach (var filePath in filePaths)
                {
                    if (file == null || file.Path != filePath)
                        continue;

                    _files.Remove(file);
                }
            }
        }
    }
}
