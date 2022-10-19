using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Elixir {
    [CreateAssetMenu(fileName = "Data", menuName = "Elixir/ElixirDescriptor", order = 1)]
    public class ElixirDescriptor : ScriptableObject {
        public bool         useconsole;
        public string       GameID; // 076258f0-49c1-4f0a-a832-ede4ff7628f4
        public string       DevAPIKey; // c29893d3-f582-4c91-91b9-4aa5fe6b3622
        public string       ProdAPIKey; // 2e904bd4-197d-4d37-94a2-da2642395571

        public enum Environments {
            Dev,
            Prod
        }

        public Environments EditorEnv = Environments.Dev;
        public Environments BuildEnv = Environments.Prod;
    }
}