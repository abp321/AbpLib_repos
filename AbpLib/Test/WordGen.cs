using System;

namespace AbpLib.Test
{
    public class WordGen
    {
        public string Word(int requestedLength)
        {
            Random rnd = new Random();
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
            string[] vowels = { "a", "e", "i", "o", "u" };

            string word = "";

            if (requestedLength == 1)
            {
                word = GetRandomLetter(rnd, vowels);
            }
            else
            {
                for (int i = 0; i < requestedLength; i += 2)
                {
                    int l = 2;

                    word += l.RN() == 0 ? GetRandomLetter(rnd, vowels) + GetRandomLetter(rnd, consonants) : GetRandomLetter(rnd, consonants) + GetRandomLetter(rnd, vowels);
                }
                word = word.Replace("q", "qu").Substring(0, requestedLength);
            }

            return word;
        }

        private static string GetRandomLetter(Random rnd, string[] letters)
        {
            return letters[rnd.Next(0, letters.Length - 1)];
        }
    }
}
