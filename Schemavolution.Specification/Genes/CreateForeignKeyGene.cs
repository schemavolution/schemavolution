using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Schemavolution.Evolve.Providers;
using Schemavolution.Specification.Implementation;

namespace Schemavolution.Specification.Genes
{
    class CreateForeignKeyGene : TableDefinitionGene
    {
        private readonly IndexGene _parent;
        private readonly CreatePrimaryKeyGene _referencing;
        private readonly bool _cascadeDelete;
        private readonly bool _cascadeUpdate;

        public string DatabaseName => _parent.DatabaseName;
        public string SchemaName => _parent.SchemaName;
        public string TableName => _parent.TableName;
        public IEnumerable<CreateColumnGene> Columns => _parent.Columns;
        internal override CreateTableGene CreateTableGene => _parent.CreateTableGene;

        public CreateForeignKeyGene(IndexGene parent, CreatePrimaryKeyGene referencing, bool cascadeDelete, bool cascadeUpdate, ImmutableList<Gene> prerequsites) :
            base(prerequsites)
        {
            _parent = parent;
            _referencing = referencing;
            _cascadeDelete = cascadeDelete;
            _cascadeUpdate = cascadeUpdate;
        }

        public override IEnumerable<Gene> AllPrerequisites => Prerequisites
            .Concat(new[] { CreateTableGene });

        public override string[] GenerateSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph, IDatabaseProvider provider)
        {
            string indexTail = string.Join("_", Columns.Select(c => $"{c.ColumnName}").ToArray());
            string columnList = string.Join(", ", Columns.Select(c => $"[{c.ColumnName}]").ToArray());
            string referenceColumnList = string.Join(", ", _referencing.Columns.Select(c => $"[{c.ColumnName}]").ToArray());
            string onDelete = _cascadeDelete ? " ON DELETE CASCADE" : "";
            string onUpdate = _cascadeUpdate ? " ON UPDATE CASCADE" : "";

            string[] sql =
            {
                $@"ALTER TABLE [{DatabaseName}].[{SchemaName}].[{TableName}]
    ADD CONSTRAINT [FK_{TableName}_{indexTail}] FOREIGN KEY ({columnList})
        REFERENCES [{_referencing.DatabaseName}].[{_referencing.SchemaName}].[{_referencing.TableName}] ({referenceColumnList}){onDelete}{onUpdate}"
            };

            return sql;
        }

        public override string[] GenerateRollbackSql(EvolutionHistoryBuilder genesAffected, IGraphVisitor graph, IDatabaseProvider provider)
        {
            string indexTail = string.Join("_", Columns.Select(c => $"{c.ColumnName}").ToArray());

            string[] sql =
            {
                $"ALTER TABLE [{DatabaseName}].[{SchemaName}].[{TableName}]\r\n    DROP CONSTRAINT [FK_{TableName}_{indexTail}]"
            };

            return sql;
        }

        internal override string GenerateDefinitionSql()
        {
            string indexTail = string.Join("_", Columns.Select(c => $"{c.ColumnName}").ToArray());
            string columnList = string.Join(", ", Columns.Select(c => $"[{c.ColumnName}]").ToArray());
            string referenceColumnList = string.Join(", ", _referencing.Columns.Select(c => $"[{c.ColumnName}]").ToArray());
            string onDelete = _cascadeDelete ? " ON DELETE CASCADE" : "";
            string onUpdate = _cascadeUpdate ? " ON UPDATE CASCADE" : "";

            return $@"
    CONSTRAINT [FK_{TableName}_{indexTail}] FOREIGN KEY ({columnList})
        REFERENCES [{_referencing.DatabaseName}].[{_referencing.SchemaName}].[{_referencing.TableName}] ({referenceColumnList}){onDelete}{onUpdate}";
        }

        protected override BigInteger ComputeSha256Hash()
        {
            return nameof(CreateForeignKeyGene).Sha256Hash().Concatenate(
                _parent.Sha256Hash,
                _referencing.Sha256Hash,
                new BigInteger((_cascadeDelete ? 2 : 0) + (_cascadeUpdate ? 1 : 0)));
        }

        internal override GeneMemento GetMemento()
        {
            return new GeneMemento(
                nameof(CreateForeignKeyGene),
                new Dictionary<string, string>
                {
                    ["CascadeDelete"] = _cascadeDelete ? "true" : "false",
                    ["CascaseUpdate"] = _cascadeUpdate ? "true" : "false"
                },
                Sha256Hash,
                new Dictionary<string, IEnumerable<BigInteger>>
                {
                    ["Prerequisites"] = Prerequisites.Select(x => x.Sha256Hash),
                    ["Parent"] = new[] { _parent.Sha256Hash },
                    ["Referencing"] = new[] { _referencing.Sha256Hash }
                });
        }

        public static CreateForeignKeyGene FromMemento(GeneMemento memento, IImmutableDictionary<BigInteger, Gene> genesByHashCode)
        {
            return new CreateForeignKeyGene(
                (CreateIndexGene)genesByHashCode[memento.Prerequisites["Parent"].Single()],
                (CreatePrimaryKeyGene)genesByHashCode[memento.Prerequisites["Referencing"].Single()],
                memento.Attributes["CascadeDelete"] == "true",
                memento.Attributes["CascaseUpdate"] == "true",
                memento.Prerequisites["Prerequisites"].Select(x => genesByHashCode[x]).ToImmutableList());
        }
    }
}