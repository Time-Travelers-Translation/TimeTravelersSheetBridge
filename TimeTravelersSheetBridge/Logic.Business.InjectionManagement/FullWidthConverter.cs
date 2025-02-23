using System.Text;
using Logic.Business.InjectionManagement.InternalContract;
using Logic.Business.TimeTravelersManagement.Contract;

namespace Logic.Business.InjectionManagement
{
    internal class FullWidthConverter : IFullWidthConverter
    {
        private readonly ICharacterProvider _characterProvider;

        private readonly StringBuilder _sb = new();

        public FullWidthConverter(ICharacterProvider characterProvider)
        {
            _characterProvider = characterProvider;
        }

        public string? Convert(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            _sb.Clear();

            var isControlCode = false;
            foreach (char character in text)
            {
                switch (character)
                {
                    case '<':
                        isControlCode = true;

                        _sb.Append(character);
                        continue;

                    case '>':
                        isControlCode = false;

                        _sb.Append(character);
                        continue;

                    default:
                        if (isControlCode)
                            _sb.Append(character);
                        else if (_characterProvider.TryGet(character, out char mappedChar))
                            _sb.Append(mappedChar);
                        else
                            _sb.Append((char)ConvertCharFullWidth(character));

                        break;
                }
            }

            return _sb.ToString();
        }

        private static uint ConvertCharFullWidth(uint character)
        {
            // Convert space specially
            if (character == 0x20)
                return 0x3000;

            // Don't convert single apostrophe
            if (character == 0x27)
                return '\'';

            // Don't convert double apostrophe
            if (character == 0x22)
                return '"';

            // Convert all ASCII characters to wide Unicode
            if (character is >= 0x21 and <= 0x7E)
                return character + 0xFEE0;

            return character;
        }
    }
}
