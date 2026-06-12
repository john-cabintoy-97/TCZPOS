namespace TCZPOS.Components.Services
{
    public class TranslationService
    {
        public string CurrentLanguage { get; set; } = "English";

        private readonly Dictionary<string, Dictionary<string, string>> _data = new()
        {
            ["English"] = [], // Default, returns the key itself

            ["Bisaya"] = new()
            {
                // Primary Actions
                ["Dashboard"] = "Main Board",
                ["POS Terminal"] = "Bayranan",
                ["Sales Operations"] = "Dagan sa Sales",
                ["Customer Ledger"] = "Listahan sa Utang",
                ["Stock Control"] = "Kontrol sa Produkto",
                ["Procurement"] = "Pagpalit og Supply",
                ["Wireless Scanner"] = "Wireless Scanner",
                ["QRCode Pairing"] = "Pag-pares sa QR",
                ["Data & Insights"] = "Report og Detalye",
                ["Configuration"] = "Set-up sa System",

                // Sub-items
                ["Void & Transaction Logs"] = "Listahan sa Void/Agi",
                ["Hold List"] = "Naka-Hold",
                ["Sales History"] = "Kaagi sa Halin",
                ["Credit (Utang) Records"] = "Talaan sa Utang",
                ["Payment Collection"] = "Kolekta sa Bayad",
                ["Customer List"] = "Listahan sa Suki",
                ["Products"] = "Mga Produkto",
                ["Stock Adjustment"] = "Usab sa Stocks",
                ["Categories & Brands"] = "Kategorya og Brands",
                ["Price Batch Update"] = "Usab og Presyo",
                ["Suppliers (Vendors)"] = "Mga Supplier",
                ["Purchase Orders"] = "Order sa Supply",
                ["Receiving Log"] = "Agi sa Nadawat",
                ["Sales Reports"] = "Report sa Halin",
                ["Expiry Watchlist"] = "Monitor sa Ma-expire",
                ["Profit/Loss Analysis"] = "Ihap sa Ganansya",
                ["Staff & Permissions"] = "Staff og Permiso",
                ["Hardware Settings"] = "Set-up sa Hardware",
                ["System AI"] = "AI sa System"
            },

            ["Tagalog"] = new()
            {
                ["Dashboard"] = "Pangunahing Board",
                ["POS Terminal"] = "Bayaran",
                ["Sales Operations"] = "Operasyon ng Benta",
                ["Customer Ledger"] = "Talaan ng Utang",
                ["Stock Control"] = "Kontrol sa Stocks",
                ["Procurement"] = "Pagbili ng Supply",
                ["Wireless Scanner"] = "Wireless Scanner",
                ["QRCode Pairing"] = "Pag-pares ng QR",
                ["Data & Insights"] = "Ulat at Detalye",
                ["Configuration"] = "Ayos ng System",

                // Sub-items
                ["Void & Transaction Logs"] = "Listahan ng Void",
                ["Hold List"] = "Naka-Hold",
                ["Sales History"] = "Kasaysayan ng Benta",
                ["Credit (Utang) Records"] = "Talaan ng Utang",
                ["Payment Collection"] = "Koleksyon ng Bayad",
                ["Customer List"] = "Listahan ng Suki",
                ["Products"] = "Mga Produkto",
                ["Stock Adjustment"] = "Pagsasaayos ng Stocks",
                ["Categories & Brands"] = "Kategorya at Brands",
                ["Price Batch Update"] = "Palit-Presyo",
                ["Suppliers (Vendors)"] = "Mga Supplier",
                ["Purchase Orders"] = "Order ng Supply",
                ["Receiving Log"] = "Talaan ng Natanggap",
                ["Sales Reports"] = "Ulat ng Benta",
                ["Expiry Watchlist"] = "Monitor ng Expiry",
                ["Profit/Loss Analysis"] = "Suri ng Kita",
                ["Staff & Permissions"] = "Staff at Permiso",
                ["Hardware Settings"] = "Settings ng Hardware",
                ["System AI"] = "AI ng System"
            }
        };

        public string T(string key)
        {
            if (CurrentLanguage == "English") return key;

            if (_data.TryGetValue(CurrentLanguage, out var languageDict) &&
                    languageDict.TryGetValue(key, out var value))
            {
                return value;
            }

            return key; // Fallback to English
        }
    }
}
