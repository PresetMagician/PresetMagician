namespace PresetMagician.Core.Data
{
    public interface IWrappedModelBase
    {
        object OriginalItem { get; set; }
    }

    public abstract class WrappedModel<T> where T : class
    {
        public T OriginalItem { get; set; }

        public WrappedModel(T item)
        {
            OriginalItem = item;
        }

        public WrappedModel()
        {
        }
    }
}