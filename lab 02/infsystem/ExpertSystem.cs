using System;
using System.IO;
using System.Text.Json;

namespace infsystem
{
    static class ExpertSystem
    {
        static void Main(string[] args)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                AllowTrailingCommas = true,
                IncludeFields = true,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
                IgnoreNullValues = true,
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new ConditionConverter());

            var jsonString = File.ReadAllText("../../../rules.json");
            var rules = JsonSerializer.Deserialize<Rule[]>(jsonString, options);

            var memory = new MemoryComponent(rules);
            var inferenceComponent = new InferenceComponent(memory);

            var result = inferenceComponent.GetResult();

            if (result is Fact resultFact)
            {
                Console.WriteLine($"Результат:\n{resultFact.Name}");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Результат не определен");
            }
        }
    }
}
