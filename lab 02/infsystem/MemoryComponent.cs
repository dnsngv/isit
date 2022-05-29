using System;
using System.Collections.Generic;
using System.Linq;

namespace infsystem
{
    class MemoryComponent
    {
        // Список действующих фактов. Каждому факту в соответствие поставлено правило,
        // в результате активации которого получен данный факт.
        public readonly Dictionary<Fact, Rule?> Facts;

        // Все правила базы знаний
        public readonly Rule[] Rules;

        // Список правил, условия которых уже выполнены, и которые могут быть активированы в данный момент
        // (вместе со списком фактов, наличие которых обращает условие данного правила в истину)
        public readonly Dictionary<Rule, List<Fact>> Agenda = new ();

        public Rule[] NotActivatedRules => 
            Rules.Where(rule => !ActivatedRules.ContainsKey(rule)).ToArray();
        // Активированные правила, правая часть которых уже внесена в список действующих фактов
        public Dictionary<Rule, List<Fact>> ActivatedRules = new ();

        public MemoryComponent(IEnumerable<Rule> rules)
        {
            Rules = rules.ToArray();
            Facts = new();
        }
        public MemoryComponent(IEnumerable<Rule> rules, IEnumerable<Fact> initialFacts)
        {
            Rules = rules.ToArray();
            Facts = initialFacts.ToDictionary<Fact, Fact, Rule?>(
                fact => fact, 
                _ => null
            );
        }

        public void AddFact(Fact fact, Rule reason) => Facts.Add(fact, reason);

        public void ActivateRule(Rule rule, List<Fact> requirements) 
        {
            if (ActivatedRules.ContainsKey(rule))
                throw new InvalidOperationException($"Rule already activated:\n{rule}");

            ActivatedRules.Add(rule, requirements);
            RemoveFromAgenda(rule);
        }

        public void AddToAgenda(Rule rule, List<Fact> requirements)
        {
            if (Agenda.ContainsKey(rule)) RemoveFromAgenda(rule);

            Agenda.Add(rule, requirements);
        }

        public void RemoveFromAgenda(Rule rule)
        {
            if (!Agenda.Remove(rule))
                throw new InvalidOperationException($"Rule was not on agenda:\n{rule}");
        }
    }
}