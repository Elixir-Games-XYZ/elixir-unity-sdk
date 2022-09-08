using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Elixir {
    [CreateAssetMenu(fileName = "Data", menuName = "Elixir/ElixirDescriptor", order = 1)]
    public class ElixirDescriptor : ScriptableObject {
        [System.Serializable]
        public class Environment {
            public string APIKey;
            public string GameID;
        }
        public bool         useconsole;
        public Environment  Sanbox;
        public Environment  Production;

        public enum Environments {
            Sanbox,
            Production
        }

        public Environments InEditor = Environments.Sanbox;
        public Environments InBuild = Environments.Production;
    }
}