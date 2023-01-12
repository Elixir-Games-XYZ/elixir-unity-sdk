using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elixir{
    public class NFTs : BaseWS {
        [System.Serializable]
        public class Collection {
            public string collection;
            public string collectionName;
            [System.Serializable]
            public class NTF {
                public string tokenId;
                public string name;
                public string image;

                [System.Serializable]
                public class Attribute {
                    public string trait_type;
                    public string value;
                }
                public Attribute[] attributes;
            };
            public NTF[] nfts;
        };
        [System.Serializable]
        public class Data{
            public Collection[] data;
        }
        static Data responseCollections = new Data();
        public static Collection[] collections { get { return responseCollections.data;  } }
        public static IEnumerator Get() {
            if (User.userData.wallets.Length!=0)
                yield return Get($"/sdk/v2/nfts/user", responseCollections);
            else {
                error.code = -2000;
                error.message = "No wallet";
                lastError = true;
            }
        }
    }
}