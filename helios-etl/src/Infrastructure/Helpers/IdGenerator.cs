using System.Text.RegularExpressions;

namespace SunnyRewards.Helios.Etl.Infrastructure.Helpers
{
    public class IdGenerator
    {
        private static int MAX_WORDS_USED = 20;
        private static int MAX_CHARS_FROM_WORD_USED = 3;
        private static string CONCAT_SEPARATOR = "_";

        private readonly int _maxWordsUsed;
        private readonly int _maxCharsFromWordUsed;

        public IdGenerator()
        {
            _maxWordsUsed = MAX_WORDS_USED;
            _maxCharsFromWordUsed = MAX_CHARS_FROM_WORD_USED;
        }

        public IdGenerator(int maxWordUsed, int maxCharsFromWordUsed)
        {
            _maxWordsUsed = maxWordUsed;
            _maxCharsFromWordUsed = maxCharsFromWordUsed;
        }

        public string GenerateIdentifier(string input)
        {
            // Remove punctuation from the input string
            string cleanedString = RemovePunctuation(input);

            // Split the cleaned string into words
            string[] words = cleanedString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Take the first 20 words (or less if the string has fewer than 20 words)
            int wordsToTake = Math.Min(words.Length, _maxWordsUsed);
            string[] selectedWords = new string[wordsToTake];
            Array.Copy(words, selectedWords, wordsToTake);

            // Take the first 3 characters from each word and concatenate with "_" separator
            string identifier = string.Join(CONCAT_SEPARATOR,
                Array.ConvertAll(selectedWords, w => w.Substring(0, Math.Min(_maxCharsFromWordUsed, w.Length))));

            return identifier;
        }
        public string GenerateTaskIdentifier(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Trim leading/trailing spaces
            input = input.Trim();

            // Replace all spaces with underscores
            string identifier = input.Replace(' ', '_');

            return identifier;
        }

        // Function to remove punctuation from the input string
        private string RemovePunctuation(string input)
        {
            return Regex.Replace(input.ToLower(), @"[^\w\s]", " ");
        }
    }
}
