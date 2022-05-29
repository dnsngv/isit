using System.Collections.Generic;
using System.Linq;

namespace infsystem
{
    class InferenceComponent
    {
        private MemoryComponent Memory;

        public InferenceComponent(MemoryComponent memory)
        {
            Memory = memory;
        }

        // Алгоритм вывода выбирает одно из правил, готовых к активации (условия которых выполнены),
        // добавляет правую часть этого правила к списку действующих фактов
        // (при необходимости - задает вопрос пользователю),
        // если один из добавленных фактов является результатом - возвращает его.
        public Fact? GetResult()
        {
            UpdateAgenda();
            while (Memory.Agenda.Any())
            {
                var (ruleToActivate, requirements) = GetOrderedAgenda().First();

                Memory.ActivateRule(ruleToActivate, requirements);

                foreach (var fact in ruleToActivate.Assertions)
                {
                    if (fact.Input is Input input)
                    {
                        fact.Value = UserInput.AskQuestion(input);
                    }

                    Memory.AddFact(fact, ruleToActivate);

                    if (fact.IsResult) return fact;
                }
                UpdateAgenda();
            }

            return null;
        }

        // Алгоритм разрешения конфликтов
        // Правила отсортированы сначала по заданному приоритету,
        // затем по конкретности (сначала с наибольшим количеством условий),
        // затем по порядку выполнения их условия (сначала самые новые)
        private Dictionary<Rule, List<Fact>> GetOrderedAgenda() =>
            Memory.Agenda
                .Select((rule, i) => new
                {
                    Rule = rule.Key,
                    OrderInAgenda = i,
                    rule.Key.Salience,
                    rule.Key.Condition.Specificity,
                    Requirements = rule.Value,
                })
                .OrderByDescending(ruleWithOrdering => ruleWithOrdering.Salience)
                .ThenByDescending(ruleWithOrdering => ruleWithOrdering.Specificity)
                .ThenByDescending(ruleWithOrdering => ruleWithOrdering.OrderInAgenda)
                .ToDictionary(
                    ruleWithOrdering => ruleWithOrdering.Rule, 
                    ruleWithOrdering => ruleWithOrdering.Requirements
                );

        private void UpdateAgenda()
        {
            foreach (var (rule, _) in Memory.Agenda)
            {
                var result = rule.Condition.Evaluate(Memory.Facts.Keys);
                if (!result.CanActivate)
                {
                    Memory.RemoveFromAgenda(rule);
                }
            }

            foreach (var rule in Memory.NotActivatedRules)
            {
                var result = rule.Condition.Evaluate(Memory.Facts.Keys);
                if (result.CanActivate)
                {
                    Memory.AddToAgenda(rule, result.Requirements);
                }
            }
        }
    }
}