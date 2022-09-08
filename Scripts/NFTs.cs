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
        class NFTsResponse{
            public Collection[] data;
        }
        static NFTsResponse responseCollections = new NFTsResponse();
        public static Collection[] collections { get { return responseCollections.data;  } }
        public static IEnumerator Get() {
            if (!string.IsNullOrEmpty(User.userData.wallet))
                yield return Get($"/nfts/user/{GameID}", responseCollections);
            else {
                error.code = -2000;
                error.message = "No wallet";
                lastError = true;
            }
        }
    }
}