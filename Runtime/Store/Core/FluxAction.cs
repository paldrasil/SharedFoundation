using UnityEngine;
using System.Collections.Generic;

namespace Shared.Foundation
{
    public class FluxAction : Dictionary<string, object>
    {
        public enum RequestContentType
        {
            Default,
            Json,
            Put,
            Delete
        };

        public enum AccessTokenType
        {
            None,
            Account,
            Game,
        }

        private int _actionType;
        public string apiUrl;
        public int timeout = 15;
        //public AccessTokenType accessTokenType = AccessTokenType.None;
        public bool includeAccessToken;
        public bool isPostRequest;
        public RequestContentType requestContentType;
        public WWWForm apiForm;
        public string jsonPayload;

        public bool isDone = false;
        public long errorCode;
        public string errorMsg;
        public string response;
        public FluxAction next;

        public FluxAction(int type, Dictionary<string, object> payload)
        {
            _actionType = type;
            requestContentType = RequestContentType.Default;
            foreach (KeyValuePair<string, object> kvp in payload)
            {
                this.Add(kvp.Key, kvp.Value);
            }
        }

        public FluxAction(int type)
        {
            _actionType = type;
            requestContentType = RequestContentType.Default;
        }

        public int GetActionType()
        {
            return _actionType;
        }
    }
}
