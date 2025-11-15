

namespace BankingApp.Core.Application.Helpers
{
    public static class EnumMapper<TEnum> where TEnum : struct, Enum
    {
        private static readonly Dictionary<string, TEnum> _aliases = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<TEnum, List<string>> _reverseAliases = new();

        public static void AddAlias(string alias, TEnum value)
        {
            if (string.IsNullOrWhiteSpace(alias))
                throw new ArgumentException("El alias no puede estar vacío.", nameof(alias));

            alias = alias.ToLower();
            _aliases[alias] = value;

            if (!_reverseAliases.ContainsKey(value))
                _reverseAliases[value] = new List<string>();

            if (!_reverseAliases[value].Contains(alias))
                _reverseAliases[value].Add(alias);
        }

        public static IEnumerable<string> GetAllAliases() => _aliases.Keys;

        public static void AddAliases(Dictionary<string, TEnum> aliases)
        {
            foreach (var pair in aliases)
                AddAlias(pair.Key, pair.Value);
        }

        public static TEnum FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El valor no puede estar vacío.", nameof(value));

            if (_aliases.TryGetValue(value.ToLower(), out var result))
                return result;

            if (Enum.TryParse<TEnum>(value, true, out var parsed))
                return parsed;

            throw new ArgumentException($"Valor no reconocido para {typeof(TEnum).Name}: {value}");
        }

        public static string ToString(TEnum value)
        {
            if (_reverseAliases.TryGetValue(value, out var aliases) && aliases.Any())
                return aliases.First();

            return value.ToString();
        }

        public static IEnumerable<string> GetAliasesFor(TEnum value)
        {
            if (_reverseAliases.TryGetValue(value, out var aliases))
                return aliases;

            return Enumerable.Empty<string>();
        }

        public static IEnumerable<(string Alias, TEnum Value)> GetAliasEnumPairs()
        {
            return _reverseAliases.Select(pair => (pair.Value.First(), pair.Key));
        }
    }
}
