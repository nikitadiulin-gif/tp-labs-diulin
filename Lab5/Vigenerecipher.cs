namespace Lab5;

//Шифр Виженера над латинским алфавитом A-Z. Символы, не входящие в алфавит
//(пробелы, знаки препинания, цифры), не изменяются и не сдвигают позицию ключа.
public static class VigenereCipher
{
    public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    //Шифрует текст по ключу
    public static string Encrypt(string text, string key) => Process(text, key, shiftSign: 1);
    //Расшифровывает текст по ключу
    public static string Decrypt(string text, string key) => Process(text, key, shiftSign: -1);
    private static string Process(string text, string key, int shiftSign)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Ключ не может быть пустым.", nameof(key));

        key = key.ToUpperInvariant();
        foreach (char k in key)
        {
            if (!Alphabet.Contains(k))
                throw new ArgumentException($"Ключ может содержать только латинские буквы A-Z: '{k}' недопустим.", nameof(key));
        }
        var result = new char[text.Length];
        int keyIndex = 0;

        for (int i = 0; i < text.Length; i++)
        {
            char c = char.ToUpperInvariant(text[i]);
            int letterPos = Alphabet.IndexOf(c);

            if (letterPos < 0)
            {
                result[i] = text[i];
                continue;
            }
            int keyShift = Alphabet.IndexOf(key[keyIndex % key.Length]);
            int shifted = ((letterPos + shiftSign * keyShift) % Alphabet.Length + Alphabet.Length) % Alphabet.Length;

            result[i] = Alphabet[shifted];
            keyIndex++;
        }
        return new string(result);
    }
}