using System.Text.RegularExpressions;
using C__Project.Data;
using Microsoft.EntityFrameworkCore;

namespace C__Project.Services;

public class ExpenseClassifierService
{
    private readonly ExpenseTrackerDbContext _context;

    public ExpenseClassifierService(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public class ClassificationResult
    {
        public int CategoryId { get; set; }
        public double Confidence { get; set; }
    }

    // Default static mapping dictionary for bootstrap suggest logic when DB data is small
    private static readonly Dictionary<string, int> PretrainedKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        // Food & Dining (Id = 1)
        { "food", 1 }, { "dining", 1 }, { "restaurant", 1 }, { "cafe", 1 }, { "coffee", 1 }, 
        { "starbucks", 1 }, { "mcdonalds", 1 }, { "pizza", 1 }, { "lunch", 1 }, { "dinner", 1 }, 
        { "breakfast", 1 }, { "burger", 1 }, { "groceries", 1 }, { "grocery", 1 }, { "sushi", 1 },
        { "bakery", 1 }, { "supermarket", 1 }, { "subway", 1 }, { "dunkin", 1 }, { "eat", 1 },

        // Transport & Fuel (Id = 2)
        { "transport", 2 }, { "fuel", 2 }, { "gas", 2 }, { "petrol", 2 }, { "diesel", 2 }, 
        { "uber", 2 }, { "lyft", 2 }, { "taxi", 2 }, { "cab", 2 }, { "metro", 2 }, 
        { "subway-ticket", 2 }, { "train", 2 }, { "bus", 2 }, { "flight", 2 }, { "airline", 2 }, 
        { "parking", 2 }, { "toll", 2 }, { "shell", 2 }, { "chevron", 2 }, { "exxon", 2 },

        // Utilities & Bills (Id = 3)
        { "utility", 3 }, { "utilities", 3 }, { "electricity", 3 }, { "power", 3 }, { "water", 3 }, 
        { "gas-bill", 3 }, { "internet", 3 }, { "wifi", 3 }, { "phone", 3 }, { "mobile", 3 }, 
        { "broadband", 3 }, { "cable", 3 }, { "sewer", 3 }, { "trash", 3 }, { "insurance", 3 },
        { "bill", 3 }, { "bills", 3 }, { "subscription", 3 },

        // Entertainment (Id = 4)
        { "entertainment", 4 }, { "movie", 4 }, { "movies", 4 }, { "cinema", 4 }, { "netflix", 4 }, 
        { "spotify", 4 }, { "hulu", 4 }, { "disney", 4 }, { "youtube", 4 }, { "concert", 4 }, 
        { "show", 4 }, { "theater", 4 }, { "ticket", 4 }, { "tickets", 4 }, { "game", 4 }, 
        { "gaming", 4 }, { "xbox", 4 }, { "playstation", 4 }, { "steam", 4 }, { "pub", 4 }, 
        { "bar", 4 }, { "club", 4 }, { "beer", 4 }, { "wine", 4 },

        // Housing & Rent (Id = 5)
        { "housing", 5 }, { "rent", 5 }, { "apartment", 5 }, { "house", 5 }, { "mortgage", 5 }, 
        { "lease", 5 }, { "landlord", 5 }, { "furniture", 5 }, { "decor", 5 }, { "home", 5 }, 
        { "repairs", 5 }, { "maintenance", 5 },

        // Shopping (Id = 6)
        { "shopping", 6 }, { "store", 6 }, { "mall", 6 }, { "clothes", 6 }, { "clothing", 6 }, 
        { "shoes", 6 }, { "apparel", 6 }, { "amazon", 6 }, { "walmart", 6 }, { "target", 6 }, 
        { "ebay", 6 }, { "bestbuy", 6 }, { "gadget", 6 }, { "electronics", 6 }, { "gift", 6 }, 
        { "cosmetics", 6 }, { "makeup", 6 }, { "shoes-weekly", 6 }
    };

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "and", "or", "but", "if", "because", "as", "until", "while",
        "of", "at", "by", "for", "with", "about", "against", "between", "into", "through",
        "during", "before", "after", "above", "below", "to", "from", "up", "down", "in",
        "out", "on", "off", "over", "under", "again", "further", "then", "once", "here",
        "there", "when", "where", "why", "how", "all", "any", "both", "each", "few",
        "more", "most", "other", "some", "such", "no", "nor", "not", "only", "own", "same",
        "so", "than", "too", "very", "s", "t", "can", "will", "just", "don", "should", "now"
    };

    public async Task<ClassificationResult?> PredictCategoryAsync(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return null;

        var tokens = Tokenize(description);
        if (!tokens.Any()) return null;

        // Fetch all transactions from the database
        var expenses = await _context.Expenses.AsNoTracking().ToListAsync();
        
        // Count totals
        var categoryCounts = new Dictionary<int, int>();
        var categoryWordCounts = new Dictionary<int, Dictionary<string, int>>();
        var uniqueWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Prepopulate dictionary structure for categories 1 to 6
        for (int i = 1; i <= 6; i++)
        {
            categoryCounts[i] = 0;
            categoryWordCounts[i] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        // Seed with Pretrained Keywords to give it static intelligence on first boot
        foreach (var keyword in PretrainedKeywords)
        {
            var word = keyword.Key;
            var catId = keyword.Value;
            if (categoryCounts.ContainsKey(catId))
            {
                // Give seed weight
                categoryCounts[catId] += 1;
                if (!categoryWordCounts[catId].ContainsKey(word))
                {
                    categoryWordCounts[catId][word] = 0;
                }
                categoryWordCounts[catId][word] += 3; // Seed weights count as multiple occurrences
                uniqueWords.Add(word);
            }
        }

        // Process database training data
        foreach (var exp in expenses)
        {
            var catId = exp.CategoryId;
            if (!categoryCounts.ContainsKey(catId))
            {
                categoryCounts[catId] = 0;
                categoryWordCounts[catId] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }

            categoryCounts[catId]++;

            var expTokens = Tokenize(exp.Description);
            foreach (var token in expTokens)
            {
                uniqueWords.Add(token);
                if (!categoryWordCounts[catId].ContainsKey(token))
                {
                    categoryWordCounts[catId][token] = 0;
                }
                categoryWordCounts[catId][token]++;
            }
        }

        int totalDocs = categoryCounts.Values.Sum();
        int vocabularySize = uniqueWords.Count;

        // If no training data exists, abort
        if (totalDocs == 0 || vocabularySize == 0) return null;

        int bestCategoryId = 1;
        double maxLogPosterior = double.MinValue;
        var categoryScores = new Dictionary<int, double>();

        // Naive Bayes probability estimation: Log P(c|d) = Log P(c) + Sum Log P(w|c)
        foreach (var cat in categoryCounts.Keys)
        {
            double prior = (double)categoryCounts[cat] / totalDocs;
            double logPosterior = Math.Log(prior);

            int totalWordsInCat = categoryWordCounts[cat].Values.Sum();

            foreach (var token in tokens)
            {
                int wordCountInCat = categoryWordCounts[cat].ContainsKey(token) ? categoryWordCounts[cat][token] : 0;
                // Laplace Smoothing
                double wordProbability = (double)(wordCountInCat + 1) / (totalWordsInCat + vocabularySize);
                logPosterior += Math.Log(wordProbability);
            }

            categoryScores[cat] = logPosterior;

            if (logPosterior > maxLogPosterior)
            {
                maxLogPosterior = logPosterior;
                bestCategoryId = cat;
            }
        }

        // Calculate confidence by normalizing scores
        double sumExp = 0.0;
        var relativeProbabilities = new Dictionary<int, double>();
        foreach (var cat in categoryScores.Keys)
        {
            double diff = categoryScores[cat] - maxLogPosterior;
            double exp = Math.Exp(diff);
            relativeProbabilities[cat] = exp;
            sumExp += exp;
        }

        double confidence = sumExp > 0 ? (relativeProbabilities[bestCategoryId] / sumExp) * 100 : 0.0;

        return new ClassificationResult
        {
            CategoryId = bestCategoryId,
            Confidence = confidence
        };
    }

    private List<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();

        // Clean punctuation, lowercase, split by space
        var clean = Regex.Replace(text.ToLower(), @"[^\w\s]", "");
        var words = clean.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        var tokens = new List<string>();
        foreach (var word in words)
        {
            if (word.Length >= 2 && !StopWords.Contains(word))
            {
                tokens.Add(word);
            }
        }
        return tokens;
    }
}
