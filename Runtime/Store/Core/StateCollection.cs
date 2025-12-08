using System.Collections.Generic;

namespace Shared.Foundation
{
    public abstract class StateCollection : Dictionary<string, IStoreData>
    {
        public void SetState(string name, IStoreData data)
        {
            this[name] = data;
        }

        public IStoreData GetState(string name)
        {
            if (this.ContainsKey(name))
            {
                return this[name];
            }

            return null;
        }

        public abstract StateCollection CreateNew();
    }
}
