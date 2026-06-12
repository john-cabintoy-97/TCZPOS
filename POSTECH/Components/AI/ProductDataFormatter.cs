using System.Text.RegularExpressions;
using System.Text;

namespace TCZPOS.Components.AI
{
    public class ProductDataFormatter
    {
        public class ProductEntry
        {
            public string FullName { get; set; } = string.Empty;
            public string ShortName { get; set; } = string.Empty;
        }

        /// <summary>
        /// Converts your raw product list to properly formatted CSV for ML training
        /// </summary>
        public static string FormatToTrainingCSV(List<ProductEntry> products)
        {
            var sb = new StringBuilder();
            sb.AppendLine("FullName,ShortName");

            foreach (var product in products)
            {
                // Clean and normalize text
                string cleanFullName = NormalizeText(product.FullName);
                string cleanShortName = NormalizeText(product.ShortName);

                // Escape quotes if needed
                cleanFullName = EscapeCsvField(cleanFullName);
                cleanShortName = EscapeCsvField(cleanShortName);

                sb.AppendLine($"{cleanFullName},{cleanShortName}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates augmented training data (variations of same product)
        /// Helps ML model learn patterns better
        /// </summary>
        public static List<ProductEntry> AugmentTrainingData(List<ProductEntry> originalProducts)
        {
            var augmented = new List<ProductEntry>(originalProducts);

            foreach (var product in originalProducts)
            {
                // Add variations with common Philippine retail variations
                augmented.Add(new ProductEntry
                {
                    FullName = product.FullName.Replace("SACHET", "PACK"),
                    ShortName = product.ShortName
                });

                augmented.Add(new ProductEntry
                {
                    FullName = product.FullName.Replace("REFILL", "BOTTLE"),
                    ShortName = product.ShortName
                });

                // Remove size variations
                string withoutSize = Regex.Replace(product.FullName, @"\s\d+(G|ML|L|KG|S)", "");
                if (withoutSize != product.FullName)
                {
                    augmented.Add(new ProductEntry
                    {
                        FullName = withoutSize,
                        ShortName = product.ShortName
                    });
                }
            }

            return augmented;
        }

        private static string NormalizeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Convert to UPPERCASE (Philippine retail standard)
            string result = input.ToUpperInvariant();

            // Fix common variations
            result = result.Replace(" PCS ", "S ");
            result = result.Replace(" PIECES ", "S ");
            result = result.Replace(" GRAM ", "G ");
            result = result.Replace(" MILLILITER ", "ML ");

            // Remove special characters
            result = Regex.Replace(result, @"[^\w\s\d/]", " ");

            // Remove extra spaces
            result = Regex.Replace(result, @"\s+", " ").Trim();

            return result;
        }

        private static string EscapeCsvField(string field)
        {
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }
            return field;
        }
    }
}
