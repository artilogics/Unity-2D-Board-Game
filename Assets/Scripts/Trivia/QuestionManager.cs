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
                // Normalize Category: Title Case to match Enum.ToString()
                // Assumes categories in CSV are like "science" or "SCIENCE" -> "Science"
                // For now, let's just Trim. Ideally, we map to the Enum.
                category = fields[6].Trim() 
            };
            
            // Try to match Enum if possible to ensure consistency
            if (System.Enum.TryParse(question.category, true, out SpecialTile.QuestionCategory catEnum))
            {
                question.category = catEnum.ToString(); // Forces "Science" instead of "science"
            }
            else
            {
                Debug.LogWarning($"QuestionManager: Category '{question.category}' in CSV does not match any QuestionCategory Enum! defaulting to string.");
                // We keep the string, but warn.
            }

            // Add to category dictionary
            if (!questionsByCategory.ContainsKey(question.category))
            {
                questionsByCategory[question.category] = new List<TriviaQuestion>();
            }
            questionsByCategory[question.category].Add(question);
        }

        Debug.Log($"QuestionManager: Loaded {id} questions across {questionsByCategory.Keys.Count} categories: {string.Join(", ", questionsByCategory.Keys)}");
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
            Debug.LogWarning($"QuestionManager: Category '{category}' not found! Checking fallback pool.");
            // If the category completely doesn't exist, we skip straight to the global fallback
        }
        else
        {
            List<TriviaQuestion> pool = questionsByCategory[category];

            // Priority 1: Unanswered questions in requested category
            var unansweredTarget = pool.Where(q => !askedQuestionIds.Contains(q.id)).ToList();
            if (unansweredTarget.Count > 0)
            {
                return GetRandomQuestion(unansweredTarget);
            }
        }

        // --- GLOBAL FALLBACK START ---
        List<TriviaQuestion> allPool = new List<TriviaQuestion>();
        foreach (var p in questionsByCategory.Values) allPool.AddRange(p);

        // Priority 2: Unanswered questions ANY category
        var unansweredGlobal = allPool.Where(q => !askedQuestionIds.Contains(q.id)).ToList();
        if (unansweredGlobal.Count > 0)
        {
            Debug.Log($"QuestionManager: Category '{category}' exhausted fresh questions. Giving global fresh question!");
            return GetRandomQuestion(unansweredGlobal);
        }

        // At this point, EVERY single question in the game has been asked at least once.
        // We now start recycling old questions.

        // Priority 3: Incorrectly answered questions in requested category
        if (questionsByCategory.ContainsKey(category))
        {
            var incorrectTarget = questionsByCategory[category].Where(q => answeredCorrectly.ContainsKey(q.id) && !answeredCorrectly[q.id]).ToList();
            if (incorrectTarget.Count > 0)
            {
                Debug.Log($"QuestionManager: All global questions asked. Giving incorrect '{category}' question.");
                return GetRandomQuestion(incorrectTarget);
            }
        }

        // Priority 4: Incorrectly answered questions ANY category
        var incorrectGlobal = allPool.Where(q => answeredCorrectly.ContainsKey(q.id) && !answeredCorrectly[q.id]).ToList();
        if (incorrectGlobal.Count > 0)
        {
            Debug.Log($"QuestionManager: All global questions asked. Giving incorrect global question.");
            return GetRandomQuestion(incorrectGlobal);
        }

        // Priority 5: Everything has been answered correctly! Just give a random question from requested category if possible
        if (questionsByCategory.ContainsKey(category))
        {
            var correctTarget = questionsByCategory[category].Where(q => answeredCorrectly.ContainsKey(q.id) && answeredCorrectly[q.id]).ToList();
            if (correctTarget.Count > 0)
            {
                Debug.Log($"QuestionManager: Every question in the game answered correctly. Re-asking '{category}' question.");
                return GetRandomQuestion(correctTarget);
            }
        }

        // Priority 6: Absolute fallback (Should never reach here unless CSV is empty)
        Debug.LogWarning("QuestionManager: Absolute fallback reached. Randomly picking any question.");
        return GetRandomQuestion(allPool);
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
