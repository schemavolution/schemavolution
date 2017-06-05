using FluentAssertions;
using Schemavolution.EF6;
using Schemavolution.Specification;
using Schemavolution.Specification.Implementation;
using System;
using System.Linq;
using Xunit;

namespace Mathematicians.UnitTests
{
    public class SqlGeneratorTests
    {
        [Fact]
        public void CanGenerateSql()
        {
            var genome = new Genome();
            var evolutionHistory = new EvolutionHistory();
            var sql = WhenGenerateSql(genome, evolutionHistory);
            sql.Should().Contain(@"CREATE TABLE [Mathematicians].[dbo].[Mathematician](
    [MathematicianId] INT IDENTITY (1,1) NOT NULL,
    CONSTRAINT [PK_Mathematician] PRIMARY KEY CLUSTERED ([MathematicianId]),
    [Name] NVARCHAR(100) NOT NULL,
    [BirthYear] INT NOT NULL,
    [DeathYear] INT NULL)");
            sql.Should().Contain(@"CREATE TABLE [Mathematicians].[dbo].[Contribution](
    [ContributionId] INT IDENTITY (1,1) NOT NULL,
    CONSTRAINT [PK_Contribution] PRIMARY KEY CLUSTERED ([ContributionId]),
    [MathematicianId] INT NOT NULL,
    [Description] NVARCHAR(500) NOT NULL,
    INDEX [IX_Contribution_MathematicianId] NONCLUSTERED ([MathematicianId]),
    CONSTRAINT [FK_Contribution_MathematicianId] FOREIGN KEY ([MathematicianId])
        REFERENCES [Mathematicians].[dbo].[Mathematician] ([MathematicianId]))");
        }

        [Fact]
        public void GeneratesNoSqlWhenUpToDate()
        {
            var genome = new Genome();
            var evolutionHistory = GivenCompleteEvolutionHistory(genome);
            var sql = WhenGenerateSql(genome, evolutionHistory);

            sql.Length.Should().Be(0);
        }

        [Fact]
        public void CanSaveMigrationHistory()
        {
            var mementos = GivenGeneMementos(new Genome());

            mementos[0].Type.Should().Be("UseSchemaGene");
            mementos[0].Attributes["SchemaName"].Should().Be("dbo");

            mementos[1].Type.Should().Be("CreateTableGene");
            mementos[1].Attributes["TableName"].Should().Be("Mathematician");
            mementos[1].Prerequisites["Parent"].Should().Contain(mementos[0].HashCode);
        }

        [Fact]
        public void CanUpgradeToANewVersion()
        {
            var previousVersion = GivenGeneMementos(new Genome());
            var evolutionHistory = WhenLoadEvolutionHistory(previousVersion);
            var sql = WhenGenerateSql(new GenomeV2(), evolutionHistory);

            sql.Should().Contain(@"CREATE TABLE [Mathematicians].[dbo].[Field](
    [FieldId] INT IDENTITY (1,1) NOT NULL,
    CONSTRAINT [PK_Field] PRIMARY KEY CLUSTERED ([FieldId]),
    [Name] NVARCHAR(20) NOT NULL)");
            sql.Should().Contain(@"ALTER TABLE [Mathematicians].[dbo].[Contribution]
    ADD [FieldId] INT NOT NULL
    CONSTRAINT [DF_Contribution_FieldId] DEFAULT (0)");
        }

        [Fact]
        public void ThrowsWhenDowngradingToAPreviousVersion()
        {
            var laterVersion = GivenGeneMementos(new GenomeV2());
            var evolutionHistory = WhenLoadEvolutionHistory(laterVersion);

            Action generateSql = () => WhenGenerateSql(new Genome(), evolutionHistory);
            generateSql.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void ThrowsWhenMovingSideways()
        {
            var laterVersion = GivenGeneMementos(new Genome());
            var evolutionHistory = WhenLoadEvolutionHistory(laterVersion);

            Action generateSql = () => WhenGenerateSql(new GenomeV3(), evolutionHistory);
            generateSql.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void CanGenerateRollbackScript()
        {
            var previousVersion = GivenGeneMementos(new GenomeV2());
            var evolutionHistory = WhenLoadEvolutionHistory(previousVersion);
            var sql = WhenGenerateRollbackSql(new Genome(), evolutionHistory);

            sql.Should().Contain(@"DROP TABLE [Mathematicians].[dbo].[Field]");
            sql.Should().Contain(@"ALTER TABLE [Mathematicians].[dbo].[Contribution]
    DROP COLUMN [FieldId]");
            sql.Should().NotContain(@"ALTER TABLE [Mathematicians].[dbo].[Field]
    DROP COLUMN [Name]");

            int dropColumn = Array.IndexOf(sql, @"ALTER TABLE [Mathematicians].[dbo].[Contribution]
    DROP COLUMN [FieldId]");
            int dropTable = Array.IndexOf(sql, @"DROP TABLE [Mathematicians].[dbo].[Field]");
            dropColumn.Should().BeLessThan(dropTable);
        }

        private GeneMemento[] GivenGeneMementos(IGenome genome)
        {
            var evolutionHistory = GivenCompleteEvolutionHistory(genome);
            return evolutionHistory.GetMementos().ToArray();
        }

        private EvolutionHistory GivenCompleteEvolutionHistory(IGenome genome)
        {
            var databaseSpecification = new DatabaseSpecification("Mathematicians");
            genome.AddGenes(databaseSpecification);
            return databaseSpecification.EvolutionHistory;
        }

        private EvolutionHistory WhenLoadEvolutionHistory(GeneMemento[] mementos)
        {
            return EvolutionHistory.LoadMementos(mementos);
        }

        private static string[] WhenGenerateSql(IGenome genome, EvolutionHistory evolutionHistory)
        {
            var sqlGenerator = new SqlGenerator(genome, evolutionHistory);
            return sqlGenerator.Generate("Mathematicians");
        }

        private static string[] WhenGenerateRollbackSql(IGenome genome, EvolutionHistory evolutionHistory)
        {
            var sqlGenerator = new SqlGenerator(genome, evolutionHistory);
            return sqlGenerator.GenerateRollbackSql("Mathematicians");
        }
    }
}
