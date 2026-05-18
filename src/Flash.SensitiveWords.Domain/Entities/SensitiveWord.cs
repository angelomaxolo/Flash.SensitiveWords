namespace Flash.SensitiveWords.Domain.Entities;

public sealed class SensitiveWord
{
    public Guid Id { get; private set; }
    public string Word { get; private set; } = null!;
    public DateTime CreatedDate { get; private set; }

    private SensitiveWord() { }

    public SensitiveWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            throw new ArgumentException("Sensitive word must be provided.", nameof(word));
        }

        Id = Guid.NewGuid();
        Word = word.Trim();
        CreatedDate = DateTime.UtcNow;
    }

    public void UpdateWord(string newWord)
    {
        if (string.IsNullOrWhiteSpace(newWord))
        {
            throw new ArgumentException("Sensitive word must be provided.", nameof(newWord));
        }

        Word = newWord.Trim();
    }
}
