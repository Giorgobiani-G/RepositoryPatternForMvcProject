namespace MvcRepository.Repository
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RepositoryAttribute : Attribute
    {
        public bool CreateGenericRepository { get; set; } = true;
        public bool CreateQueryRepository { get; set; } = true;
    }
}
