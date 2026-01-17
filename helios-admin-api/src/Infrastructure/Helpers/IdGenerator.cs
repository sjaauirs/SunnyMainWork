using System.Text.RegularExpressions;

namespace SunnyRewards.Helios.Admin.Infrastructure.Helpers
{
    public class IdGenerator
    {
        private static int MAX_WORDS_USED = 10;
        private static int MAX_CHARS_FROM_WORD_USED = 4;
        private static string CONCAT_SEPARATOR = "_";

        public string GenerateIdentifier(string input)
        {
            // Remove punctuation from the input string
            string cleanedString = RemovePunctuation(input);

            // Split the cleaned string into words
            string[] words = cleanedString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Take the first 20 words (or less if the string has fewer than 20 words)
            int wordsToTake = Math.Min(words.Length, MAX_WORDS_USED);
            string[] selectedWords = new string[wordsToTake];
            Array.Copy(words, selectedWords, wordsToTake);

            // Take the first 3 characters from each word and concatenate with "_" separator
            string identifier = string.Join(CONCAT_SEPARATOR,
                Array.ConvertAll(selectedWords, w => w.Substring(0, Math.Min(MAX_CHARS_FROM_WORD_USED, w.Length))));

            return identifier;
        }

        // Function to remove punctuation from the input string
        private string RemovePunctuation(string input)
        {
            return Regex.Replace(input.ToLower(), @"[^\w\s]", " ");
        }
    }
}
