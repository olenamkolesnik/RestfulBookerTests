using Reqnroll;

namespace RestfulBookerTests.Helpers
{
    public static class ScenarioContextExtensions
    {
        public static void SetData<T>(this ScenarioContext context, string key, T value)
        {
            context[key] = value!;
        }

        public static T GetData<T>(this ScenarioContext context, string key)
        {
            if (!context.ContainsKey(key))
                throw new KeyNotFoundException($"ScenarioContext key '{key}' not found.");

            return (T)context[key]!;
        }

        public static bool TryGetData<T>(this ScenarioContext context, string key, out T value)
        {
            if (context.ContainsKey(key) && context[key] is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default!;
            return false;
        }
    }
}
