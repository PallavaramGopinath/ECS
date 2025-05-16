namespace Infin8.Coapp.Repository
{
    public interface IRepository<in TEntity> where TEntity :class
    {
        void Add(TEntity entity);
        Task AddAsync(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);
        Task DeleteAsync(TEntity entity);
        void Edit(TEntity entity);
        Task EditAsync(TEntity entity);
    }
    
}
