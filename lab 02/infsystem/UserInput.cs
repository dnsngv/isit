using System;
using System.Linq;

namespace infsystem
{
    static class UserInput
    {
        public static Value AskQuestion(Input input)
        {
            string question = input.Question.Trim();
            if (question[^1] != '?') question += ':';

            switch (input.Type)
            {
                case ValueType.String:
                    return new Value {StringValue = AskQuestionWithPossibleValues(question, input.Values)};
                default:
                    throw new ArgumentException("Question doesn't have a valid type");
            }
        }

        private static string AskQuestionWithPossibleValues(string question, string[] inputValues)
        {
            Console.WriteLine(question);
            for (int i = 0; i < inputValues.Length; i++)
            {
                Console.WriteLine($"{i + 1} - {inputValues[i]}");
            }

            string? value = null;
            while (value == null)
            {
                var input = Console.ReadLine();
                if (input == null) continue;

                if (ParseInt(input) is int index && index >= 1 && index <= inputValues.Length)
                {
                    value = inputValues[index - 1];
                }
                else
                {
                    input = input.ToLower();
                    var foundValue = inputValues.FirstOrDefault(value => value.ToLower() == input);
                    if (foundValue != null) value = foundValue;
                }
            }

            return value;
        }

        private static int? ParseInt(string input)
        {
            try
            {
                return int.Parse(input);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private static double? ParseDouble(string input)
        {
            try
            {
                return double.Parse(input);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}