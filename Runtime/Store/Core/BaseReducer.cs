namespace Shared.Foundation
{
    public abstract class BaseReducer
    {
        private string _name;
        IStoreData _defaultValues;

        public BaseReducer(string name, IStoreData defaultValues)
        {
            this._name = name;
            this._defaultValues = defaultValues;
        }

        public string GetName()
        {
            return _name;
        }

        public IStoreData GetDefault()
        {
            return _defaultValues;
        }

        public abstract IStoreData Reduce(IStoreData state, FluxAction action);
    }
}
