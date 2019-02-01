namespace Schemavolution.Specification
{
    public interface IGenome
    {
        RdbmsIdentifier Rdbms { get; }
        void AddGenes(DatabaseSpecification databaseSpecification);
    }
}
