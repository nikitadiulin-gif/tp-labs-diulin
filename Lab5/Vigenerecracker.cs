namespace Lab5;
//Результат перебора: найденный ключ (или null, если не найден) и число проверенных вариантов
public record CrackResult(string? Key, long CombinationsChecked);
//Взлом шифра Виженера перебором всех ключей заданной длины: для каждого
//варианта ключа текст расшифровывается, и проверяется, содержит ли результат
//известное слово. Последовательная и параллельная версии возвращают одинаковый
//результат — отличается только скорость работы.
public static class VigenereCracker
{
    //Общее количество возможных ключей длины <paramref name="keyLength"/>
    public static long TotalCombinations(int keyLength) => (long)Math.Pow(VigenereCipher.Alphabet.Length, keyLength);
    //Последовательный перебор ключей
    public static CrackResult CrackSequential(string cipherText, int keyLength, string knownWord)
    {
        ValidateArguments(keyLength, knownWord);
        long total = TotalCombinations(keyLength);
        long checkedCount = 0;
        for (long i = 0; i < total; i++)
        {
            checkedCount++;
            string key = IndexToKey(i, keyLength);
            string decrypted = VigenereCipher.Decrypt(cipherText, key);

            if (decrypted.Contains(knownWord, StringComparison.OrdinalIgnoreCase))
                return new CrackResult(key, checkedCount);
        }

        return new CrackResult(null, checkedCount);
    }
    //Параллельный перебор ключей со степенью параллелизма, заданной пользователем
    public static CrackResult CrackParallel(string cipherText, int keyLength, string knownWord, int degreeOfParallelism)
    {
        ValidateArguments(keyLength, knownWord);
        if (degreeOfParallelism <= 0)
            throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism), "Степень параллелизма должна быть положительной.");

        long total = TotalCombinations(keyLength);
        string? foundKey = null;
        long checkedCount = 0;
        object syncRoot = new();

        var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
        Parallel.For(0L, total, options, (i, state) =>
        {
            Interlocked.Increment(ref checkedCount);

            string key = IndexToKey(i, keyLength);
            string decrypted = VigenereCipher.Decrypt(cipherText, key);

            if (decrypted.Contains(knownWord, StringComparison.OrdinalIgnoreCase))
            {
                lock (syncRoot)
                {
                    foundKey ??= key;
                }
                state.Stop();
            }
        });

        return new CrackResult(foundKey, Interlocked.Read(ref checkedCount));
    }
    private static void ValidateArguments(int keyLength, string knownWord)
    {
        if (keyLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(keyLength), "Длина ключа должна быть положительной.");
        if (string.IsNullOrWhiteSpace(knownWord))
            throw new ArgumentException("Известное слово не может быть пустым.", nameof(knownWord));
    }
    //Переводит порядковый номер комбинации в ключ фиксированной длины (аналог счёта в системе счисления с основанием 26)
    private static string IndexToKey(long index, int keyLength)
    {
        var chars = new char[keyLength];
        for (int pos = keyLength - 1; pos >= 0; pos--)
        {
            chars[pos] = VigenereCipher.Alphabet[(int)(index % VigenereCipher.Alphabet.Length)];
            index /= VigenereCipher.Alphabet.Length;
        }
        return new string(chars);
    }
}