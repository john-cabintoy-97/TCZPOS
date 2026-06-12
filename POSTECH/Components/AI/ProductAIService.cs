using Microsoft.ML;
using Microsoft.ML.Data;
using TCZPOS.Components.Models;
using TCZPOS.Components.Repositories.Interfaces;
using System.Text.RegularExpressions;

namespace TCZPOS.Components.AI;

public class MLModels
{
    public class ProductTrainingData
    {
        [LoadColumn(0)]
        public string FullName { get; set; } = string.Empty;

        [LoadColumn(1)]
        public string ShortName { get; set; } = string.Empty;
    }

    public class ShortNamePrediction
    {
        [ColumnName("PredictedLabel")]
        public string SuggestedShortName { get; set; } = string.Empty;

        [ColumnName("Score")]
        public float[] Score { get; set; } = [];
    }
}

public partial class ProductAIService : IDisposable
{
    private readonly IAIProductRepositories _productRepo;
    private readonly MLContext _mlContext = new();
    private ITransformer? _model;
    private PredictionEngine<MLModels.ProductTrainingData, MLModels.ShortNamePrediction>? _predictionEngine;
    private List<AIProductModels> _productCache = [];
    private readonly Lock _trainLock = new();
    private bool _isTraining = false;
    private readonly Guid _sessionId = Guid.NewGuid();

    public int ProductCount => _productCache.Count;
    public bool IsModelTrained => _model is not null;

    public ProductAIService(IAIProductRepositories productRepo)
    {
        _productRepo = productRepo;
        Task.Run(InitializeAsync).ConfigureAwait(false);
    }

    private async Task InitializeAsync()
    {
        await LoadProductsFromDatabase();

        if (_productCache.Count >= 5)
        {
            await Task.Run(RetrainModel);
        }
    }

    public async Task ForceRetrainAsync() => await Task.Run(RetrainModel);

    public async Task<string> GetDebugInfoAsync() =>
        await Task.Run(() => $"Products: {_productCache.Count}, Model Ready: {_model is not null}, IsTraining: {_isTraining}");

    private async Task LoadProductsFromDatabase()
    {
        _productCache = await _productRepo.GetAllDataAsync();
        Console.WriteLine($"📁 Loaded {_productCache.Count} products from database");
    }

    public async Task<string> SuggestShortNameAsync(string fullProductName)
    {
        if (string.IsNullOrWhiteSpace(fullProductName))
            return string.Empty;

        var normalizedFull = NormalizeProductName(fullProductName);

        // 1. Check existing products
        var existing = _productCache.FirstOrDefault(p =>
            NormalizeProductName(p.FullName) == normalizedFull);

        if (existing is not null)
        {
            await _productRepo.IncrementUsageCountAsync(existing.Id);
            return existing.ShortName;
        }

        var suggestion = string.Empty;

        // 2. Try ML prediction
        if (_predictionEngine is not null && _productCache.Count >= 5)
        {
            try
            {
                var prediction = _predictionEngine.Predict(new MLModels.ProductTrainingData
                {
                    FullName = normalizedFull
                });

                if (!string.IsNullOrEmpty(prediction.SuggestedShortName) &&
                    prediction.Score is not null &&
                    prediction.Score.Max() > 0.3f)
                {
                    suggestion = prediction.SuggestedShortName;
                }
            }
            catch { }
        }

        // 3. Try pattern matching
        if (string.IsNullOrEmpty(suggestion))
        {
            var similar = FindSimilarProducts(normalizedFull);
            if (similar.Count != 0)
            {
                suggestion = GenerateShortNameFromPattern(normalizedFull, similar);
            }
        }

        // 4. Rule-based fallback
        if (string.IsNullOrEmpty(suggestion))
        {
            suggestion = GenerateRuleBasedShortName(fullProductName);
        }

        // Log suggestion for learning
        await LogSuggestionAsync(fullProductName, suggestion);

        return suggestion;
    }

    public async Task<AIProductModels> RegisterNewProductAsync(string fullName, string acceptedShortName)
    {
        var normalizedFull = NormalizeProductName(fullName);
        var normalizedShort = acceptedShortName.ToUpperInvariant().Trim();

        // Check if exists
        var existing = await _productRepo.GetDataByFullNameAsync(normalizedFull);
        if (existing is not null)
            return existing;

        var product = new AIProductModels
        {
            FullName = normalizedFull,
            ShortName = normalizedShort,
            IsActive = true,
            CreatedAt = DateTime.Now,
            SecureId = Guid.NewGuid(),
            UsageCount = 1
        };

        // Save to database
        await _productRepo.SaveDataAsync(product);

        // Update cache
        _productCache.Add(product);

        // Update learning history
        await UpdateAcceptedSuggestionAsync(fullName, acceptedShortName, product.Id);

        // Retrain every 5 new products
        if (_productCache.Count % 5 == 0)
        {
            await Task.Run(RetrainModel);
        }

        Console.WriteLine($"✅ Registered: '{fullName}' → '{acceptedShortName}'");
        return product;
    }

    private async Task LogSuggestionAsync(string originalInput, string suggestedName)
    {
        var history = new ProductLearningHistoryModels
        {
            OriginalInput = originalInput,
            SuggestedName = suggestedName,
            AcceptedName = string.Empty,
            WasAccepted = false,
            CreatedAt = DateTime.Now,
            SessionId = _sessionId
        };

        await _productRepo.SaveLearningHistoryAsync(history);
    }

    private async Task UpdateAcceptedSuggestionAsync(string originalInput, string acceptedName, int productId)
    {
        var histories = await _productRepo.GetUnacceptedSuggestionsAsync();
        var match = histories.LastOrDefault(h => h.OriginalInput == originalInput);

        if (match is not null)
        {
            match.AcceptedName = acceptedName;
            match.WasAccepted = true;
            match.ProductId = productId;
            await _productRepo.UpdateLearningHistoryAsync(match);
        }
    }

    private List<AIProductModels> FindSimilarProducts(string fullName)
    {
        var similar = new List<AIProductModels>();

        foreach (var product in _productCache)
        {
            var similarity = CalculateSimilarity(fullName, product.FullName);
            if (similarity > 0.5)
            {
                similar.Add(product);
            }
        }

        return [.. similar.OrderByDescending(p => CalculateSimilarity(fullName, p.FullName)).Take(3)];
    }

    private static double CalculateSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0;

        var words1 = s1.Split(' ');
        var words2 = s2.Split(' ');

        if (words1.Length > 0 && words2.Length > 0 && words1[0] == words2[0])
            return 0.8;

        var common = words1.Intersect(words2).Count();
        var total = Math.Max(words1.Length, words2.Length);

        return total > 0 ? (double)common / total : 0;
    }

    private static string GenerateShortNameFromPattern(string fullName, List<AIProductModels> similarProducts)
    {
        if (similarProducts.Count == 0)
            return GenerateRuleBasedShortName(fullName);

        var pattern = similarProducts.First();
        var fullWords = fullName.Split(' ');
        var patternShortWords = pattern.ShortName.Split(' ');

        var keptWords = new List<string>();

        foreach (var shortWord in patternShortWords)
        {
            var matchingFullWord = fullWords.FirstOrDefault(fw =>
                fw.Contains(shortWord) || shortWord.Contains(fw) ||
                fw.StartsWith(shortWord) || shortWord.StartsWith(fw));

            if (matchingFullWord is not null)
            {
                keptWords.Add(matchingFullWord);
            }
        }

        var size = ExtractSize(fullName);
        if (!string.IsNullOrEmpty(size) && !keptWords.Any(w => w.Contains(size)))
        {
            keptWords.Add(size);
        }

        var suggestion = string.Join(" ", keptWords);
        return string.IsNullOrEmpty(suggestion) ? GenerateRuleBasedShortName(fullName) : suggestion;
    }

    private void RetrainModel()
    {
        lock (_trainLock)
        {
            if (_isTraining || _productCache.Count < 5) return;

            _isTraining = true;

            try
            {
                Console.WriteLine($"🔄 Training ML model with {_productCache.Count} products...");

                var trainingData = _productCache.Select(p => new MLModels.ProductTrainingData
                {
                    FullName = p.FullName,
                    ShortName = p.ShortName
                }).ToList();

                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

                var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(MLModels.ProductTrainingData.ShortName))
                    .Append(_mlContext.Transforms.Text.FeaturizeText("Features", nameof(MLModels.ProductTrainingData.FullName)))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                        maximumNumberOfIterations: 100,
                        l2Regularization: 0.1f))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                _model = pipeline.Fit(dataView);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<MLModels.ProductTrainingData, MLModels.ShortNamePrediction>(_model);

                Console.WriteLine($"✅ Model trained on {_productCache.Count} products");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Training error: {ex.Message}");
            }
            finally
            {
                _isTraining = false;
            }
        }
    }

    private static string GenerateRuleBasedShortName(string fullName)
    {
        var upper = fullName.ToUpperInvariant();
        var words = upper.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) return string.Empty;

        var brand = words[0];
        var size = ExtractSize(upper);

        // Brand mappings
        var brandMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "AJINOMOTO", "AJI" }, { "NESTLE", "NESTLE" }, { "LUCKY", "LUCKY" },
            { "MAGGI", "MAGGI" }, { "SAN", "SML" }, { "SILVER", "S-SWAN" },
            { "DATU", "DATU" }, { "PUREFOODS", "PF" }, { "COCA", "COKE" },
            { "SAFEGUARD", "SAFE" }, { "CREAMSILK", "CS" }, { "SKYFLAKES", "SKY" }
        };

        foreach (var map in brandMap)
        {
            if (upper.Contains(map.Key))
            {
                brand = map.Value;
                break;
            }
        }

        var suggestion = new List<string> { brand };
        if (words.Length > 1 && !IsFillerWord(words[1]))
            suggestion.Add(AbbreviateWord(words[1]));
        if (!string.IsNullOrEmpty(size))
            suggestion.Add(size);

        return string.Join(" ", suggestion);
    }

    [GeneratedRegex(@"(\d+(?:\.\d+)?)\s*(G|ML|L|KG|S|PC)", RegexOptions.None, "en-US")]
    private static partial Regex SizeRegex();

    private static string ExtractSize(string text)
    {
        var match = SizeRegex().Match(text);
        return match.Success ? match.Value.Replace(" ", "") : "";
    }

    [GeneratedRegex(@"\bPOWDERED\b", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex PowderedRegex();

    [GeneratedRegex(@"\bINSTANT\b", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex InstantRegex();

    [GeneratedRegex(@"\bORIGINAL\b", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex OriginalRegex();

    [GeneratedRegex(@"\bREGULAR\b", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RegularRegex();

    [GeneratedRegex(@"\bWITH\b", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex WithRegex();

    [GeneratedRegex(@"\bFLAVOR\b", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex FlavorRegex();

    [GeneratedRegex(@"\bREFILL\b", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RefillRegex();

    [GeneratedRegex(@"\bSACHET\b", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex SachetRegex();

    [GeneratedRegex(@"\s+", RegexOptions.None, "en-US")]
    private static partial Regex WhitespaceRegex();

    private static string NormalizeProductName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        var result = name.ToUpperInvariant().Trim();

        result = PowderedRegex().Replace(result, "");
        result = InstantRegex().Replace(result, "");
        result = OriginalRegex().Replace(result, "");
        result = RegularRegex().Replace(result, "");
        result = WithRegex().Replace(result, "");
        result = FlavorRegex().Replace(result, "");
        result = RefillRegex().Replace(result, "");
        result = SachetRegex().Replace(result, "");
        result = WhitespaceRegex().Replace(result, " ").Trim();

        return result;
    }

    private static bool IsFillerWord(string word)
    {
        string[] fillers = ["OF", "THE", "AND", "FOR", "WITH", "BRAND"];
        return fillers.Contains(word);
    }

    private static string AbbreviateWord(string word)
    {
        if (word.Length <= 5) return word;
        var abbreviations = new Dictionary<string, string>
        {
            { "CHOCOLATE", "CHOC" }, { "VINEGAR", "VIN" }, { "CONDITIONER", "COND" },
            { "DETERGENT", "DET" }, { "SHAMPOO", "SHAM" }, { "EXTRA", "EX" },
            { "BARBECUE", "BBQ" }, { "WHITENING", "WHT" }, { "CHILIMANSI", "CHLIM" }
        };
        foreach (var abbr in abbreviations)
            if (word.Contains(abbr.Key)) return abbr.Value;
        return word.Length > 4 ? word[..4] : word;
    }

    // Public methods for UI
    public async Task<List<AIProductModels>> SearchProductsAsync(string query) =>
        await _productRepo.SearchProductsAsync(query);

    public async Task<List<AIProductModels>> GetPopularProductsAsync(int limit = 10) =>
        await _productRepo.GetPopularProductsAsync(limit);

    public async Task<bool> LoadCSVIntoDatabaseAsync(string csvContent)
    {
        try
        {
            var lines = csvContent.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
            var processedNames = new HashSet<string>();

            for (int i = 1; i < lines.Length; i++)
            {
                var parts = SplitCsvLine(lines[i]);
                if (parts.Length >= 2)
                {
                    var fullName = NormalizeProductName(parts[0]);

                    // Check Repository AND our local HashSet
                    if (!processedNames.Contains(fullName) && !await _productRepo.DataExistsAsync(fullName))
                    {
                        var product = new AIProductModels
                        {
                            FullName = fullName,
                            ShortName = parts[1].Trim().ToUpperInvariant()
                        };
                        await _productRepo.SaveDataAsync(product);
                        processedNames.Add(fullName);
                    }
                }
            }
            await LoadProductsFromDatabase();
            await Task.Run(RetrainModel);

            Console.WriteLine($"📦 Loaded {processedNames.Count} products from CSV");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading CSV: {ex.Message}");
            return false;
        }
    }

    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '"') inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else current.Append(c);
        }
        result.Add(current.ToString());
        return [.. result];
    }

    public async Task<List<string>> SuggestShortNamesAsync(string fullProductName, int count = 3)
    {
        if (string.IsNullOrWhiteSpace(fullProductName))
            return [];

        var suggestions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedFull = NormalizeProductName(fullProductName);

        // 1. Exact Match Check (Highest Priority)
        var existing = _productCache.FirstOrDefault(p =>
            NormalizeProductName(p.FullName) == normalizedFull);

        if (existing is not null)
        {
            suggestions.Add(existing.ShortName);
        }

        // 2. ML Prediction (Only if trained)
        if (_predictionEngine is not null && _productCache.Count >= 5)
        {
            try
            {
                var prediction = _predictionEngine.Predict(new MLModels.ProductTrainingData { FullName = normalizedFull });
                if (!string.IsNullOrEmpty(prediction.SuggestedShortName))
                {
                    suggestions.Add(prediction.SuggestedShortName);
                }
            }
            catch { /* Prediction fail, move on */ }
        }

        // 3. Pattern Matching (Find up to 3 similar)
        var similar = FindSimilarProducts(normalizedFull);
        if (similar.Count != 0)
        {
            foreach (var product in similar)
            {
                var patternResult = GenerateShortNameFromPattern(normalizedFull, [product]);
                if (!string.IsNullOrEmpty(patternResult)) suggestions.Add(patternResult);
                if (suggestions.Count >= count) break;
            }
        }

        // 4. Rule-Based Fallback (Always generates a name)
        if (suggestions.Count < count)
        {
            var ruleBased = GenerateRuleBasedShortName(fullProductName);
            suggestions.Add(ruleBased);
        }

        // 5. Variations (If still short on suggestions, add slight variations like removing sizes)
        if (suggestions.Count < count)
        {
            suggestions.Add(normalizedFull.Split(' ')[0]); // Just the brand
        }

        // Log the primary suggestion (first one) for history
        if (suggestions.Count != 0)
        {
            await LogSuggestionAsync(fullProductName, suggestions.First());
        }

        return [.. suggestions.Take(count)];
    }

    public void Dispose()
    {
        _predictionEngine?.Dispose();
        GC.SuppressFinalize(this);
    }
}