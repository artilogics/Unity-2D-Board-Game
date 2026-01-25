using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriviaQuestion
{
    public int id;
    public string question;
    public string optionA;
    public string optionB;
    public string optionC;
    public string optionD;
    public string correctAnswer; // "A", "B", "C", or "D"
    public string category;
    public bool wasAnsweredCorrectly = false;
    public bool wasAsked = false;
}

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager Instance { get; private set; }

    [Header("Question Database")]
    public TextAsset questionCSV; // Assign in Inspector

    private Dictionary<string, List<TriviaQuestion>> questionsByCategory;
    private HashSet<int> askedQuestionIds;
    private Dictionary<int, bool> answeredCorrectly;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadQuestions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadQuestions()
    {
        questionsByCategory = new Dictionary<string, List<TriviaQuestion>>();
        askedQuestionIds = new HashSet<int>();
        answeredCorrectly = new Dictionary<int, bool>();

        if (questionCSV == null)
        {
            Debug.LogError("QuestionManager: No CSV file assigned!");
            return;
        }

        string[] lines = questionCSV.text.Split('\n');
        int id = 0;

        // Skip header line
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] fields = ParseCSVLine(line);
            if (fields.Length < 7) continue;

            TriviaQuestion question = new TriviaQuestion
            {
                id = id++,
                question = fields[0],
                optionA = fields[1],
                optionB = fields[2],
                optionC = fields[3],
                optionD = fields[4],
                correctAnswer = fields[5].ToUpper(),
                category = fields[6]
            };

            // Add to category dictionary
            if (!questionsByCategory.ContainsKey(question.category))
            {
                questionsByCategory[question.category] = new List<TriviaQuestion>();
            }
            questionsByCategory[question.category].Add(question);
        }

        Debug.Log($"QuestionManager: Loaded {id} questions across {questionsByCategory.Count} categories");
    }

    // Parse CSV line handling commas in quotes
    private string[] ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.Trim());
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        fields.Add(currentField.Trim());

        return fields.ToArray();
    }

    public TriviaQuestion GetQuestion(string category)
    {
        if (!questionsByCategory.ContainsKey(category))
        {
            Debug.LogWarning($"QuestionManager: Category '{category}' not found!");
            return null;
        }

        List<TriviaQuestion> pool = questionsByCategory[category];

        // Priority 1: Unanswered questions
        var unanswered = pool.Where(q => !askedQuestionIds.Contains(q.id)).ToList();
        if (unanswered.Count > 0)
        {
            return GetRandomQuestion(unanswered);
        }

        // Priority 2: Incorrectly answered questions
        var incorrect = pool.Where(q => answeredCorrectly.ContainsKey(q.id) && !answeredCorrectly[q.id]).ToList();
        if (incorrect.Count > 0)
        {
            return GetRandomQuestion(incorrect);
        }

        // Priority 3: Correctly answered (all exhausted)
        var correct = pool.Where(q => answeredCorrectly.ContainsKey(q.id) && answeredCorrectly[q.id]).ToList();
        if (correct.Count > 0)
        {
            return GetRandomQuestion(correct);
        }

        Debug.LogWarning($"QuestionManager: No questions available for category '{category}'");
        return null;
    }

    private TriviaQuestion GetRandomQuestion(List<TriviaQuestion> pool)
    {
        int randomIndex = Random.Range(0, pool.Count);
        TriviaQuestion question = pool[randomIndex];
        
        // If this question was asked before, we're recycling it - remove from asked set
        if (askedQuestionIds.Contains(question.id))
        {
            Debug.Log($"QuestionManager: Recycling question {question.id}");
            askedQuestionIds.Remove(question.id);
        }
        
        askedQuestionIds.Add(question.id);
        return question;
    }

    public void MarkAnswered(int questionId, bool correct)
    {
        answeredCorrectly[questionId] = correct;
        Debug.Log($"QuestionManager: Question {questionId} marked as {(correct ? "correct" : "incorrect")}");
    }

    public void ResetQuestions()
    {
        askedQuestionIds.Clear();
        answeredCorrectly.Clear();
        Debug.Log("QuestionManager: Question pool reset");
    }

    public List<string> GetAllCategories()
    {
        return new List<string>(questionsByCategory.Keys);
    }

    public int GetQuestionCount(string category)
    {
        if (questionsByCategory.ContainsKey(category))
        {
            return questionsByCategory[category].Count;
        }
        return 0;
    }
}
