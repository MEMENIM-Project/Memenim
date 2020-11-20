using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Memenim.Converters
{
    public sealed class ProfileStatSex
    {
        private static readonly List<ProfileStatSex> Elements;
        private static byte _nextValue;
        private static byte NextValue
        {
            get
            {
                return _nextValue++;
            }
            set
            {
                if (value < _nextValue && _nextValue != 0)
                    value = _nextValue;

                _nextValue = value;
            }
        }

        public static ProfileStatSex Unknown { get; }
        public static ProfileStatSex Male { get; }
        public static ProfileStatSex Female { get; }

        public string Name { get; }
        public byte Value { get; }

        static ProfileStatSex()
        {
            Elements = new List<ProfileStatSex>(3);
            NextValue = 0;

            Unknown = new ProfileStatSex(nameof(Unknown));
            Male = new ProfileStatSex(nameof(Male));
            Female = new ProfileStatSex(nameof(Female));
        }

        private ProfileStatSex(string name)
            : this(name, _nextValue)
        {

        }
        private ProfileStatSex(string name, byte value)
        {
            NextValue = value;

            Name = name;
            Value = NextValue;

            Elements.Add(this);
        }

        public static ReadOnlyCollection<ProfileStatSex> GetElements()
        {
            return new ReadOnlyCollection<ProfileStatSex>(Elements);
        }

        public static ReadOnlyCollection<string> GetNames(bool localize = false)
        {
            List<string> names = new List<string>(Elements.Count);

            foreach (var element in Elements)
            {
                if (!localize)
                {
                    names.Add(element.Name);
                    continue;
                }

                string localizeName = (string)MainWindow.Instance
                    .FindResource(GetResourceKey(element.Name));

                if (localizeName == null)
                    continue;

                names.Add(localizeName);
            }

            return new ReadOnlyCollection<string>(names);
        }

        public static ReadOnlyCollection<byte> GetValues()
        {
            List<byte> values = new List<byte>(Elements.Count);

            foreach (var element in Elements)
            {
                values.Add(element.Value);
            }

            return new ReadOnlyCollection<byte>(values);
        }

        public static string GetResourceKey(string name)
        {
            if (name == null)
                return null;

            return nameof(ProfileStatSex) + name;
        }
        public static string GetResourceKey(byte value)
        {
            string name = ParseValue(value);

            if (name == null)
                return null;

            return nameof(ProfileStatSex) + name;
        }

        public static byte ParseName(string name, bool localize = false)
        {
            foreach (var element in Elements)
            {
                if (!localize)
                {
                    if (element.Name == name)
                        return element.Value;

                    continue;
                }

                string localizeName = (string)MainWindow.Instance
                    .FindResource(GetResourceKey(element.Name));

                if (localizeName == null)
                    continue;

                if (localizeName == name)
                    return element.Value;
            }

            return 0;
        }

        public static string ParseValue(byte value, bool localize = false)
        {
            foreach (var element in Elements)
            {
                if (element.Value == value)
                {
                    return !localize
                        ? element.Name
                        : (string)MainWindow.Instance.FindResource(GetResourceKey(element.Name));
                }
            }

            return null;
        }
    }
}
