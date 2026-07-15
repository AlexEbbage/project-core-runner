namespace CoreRacer.Services.Save
{
    /// <summary>Optional capability for storage backends that can batch mutations before one durable flush.</summary>
    public interface IFlushableSaveStorage
    {
        void Flush();
    }
}
