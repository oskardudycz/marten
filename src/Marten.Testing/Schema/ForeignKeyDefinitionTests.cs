﻿using System;
using Marten.Schema;
using Marten.Testing.Documents;
using Shouldly;
using Xunit;

namespace Marten.Testing.Schema
{
    public class ForeignKeyDefinitionTests
    {
        private readonly DocumentMapping _userMapping = DocumentMapping.For<User>();
        private readonly DocumentMapping _issueMapping = DocumentMapping.For<Issue>();

        [Fact]
        public void default_key_name()
        {
            new ForeignKeyDefinition("user_id", _issueMapping, _userMapping).KeyName.ShouldBe("mt_doc_issue_user_id_fkey");
        }

        [Fact]
        public void generate_ddl()
        {
            var expected = string.Join(Environment.NewLine,
                "ALTER TABLE public.mt_doc_issue",
                "ADD CONSTRAINT mt_doc_issue_user_id_fkey FOREIGN KEY (user_id)",
                "REFERENCES public.mt_doc_user (id);");

            new ForeignKeyDefinition("user_id", _issueMapping, _userMapping).ToDDL()
                                                                            .ShouldBe(expected);
        }

        [Fact]
        public void generate_ddl_on_other_schema()
        {
            var issueMappingOtherSchema = DocumentMapping.For<Issue>("schema1");
            var userMappingOtherSchema = DocumentMapping.For<User>("schema2");

            var expected = string.Join(Environment.NewLine,
                "ALTER TABLE schema1.mt_doc_issue",
                "ADD CONSTRAINT mt_doc_issue_user_id_fkey FOREIGN KEY (user_id)",
                "REFERENCES schema2.mt_doc_user (id);");

            new ForeignKeyDefinition("user_id", issueMappingOtherSchema, userMappingOtherSchema).ToDDL()
                                                                                                .ShouldBe(expected);
        }
    }

    public class ExternalForeignKeyDefinitionTests
    {
        private readonly DocumentMapping _userMapping = DocumentMapping.For<User>();

        [Fact]
        public void generate_ddl()
        {
            var expected = string.Join(Environment.NewLine,
                "ALTER TABLE public.mt_doc_user",
                "ADD CONSTRAINT mt_doc_user_user_id_fkey FOREIGN KEY (user_id)",
                "REFERENCES external_schema.external_table (external_id);");

            new ExternalForeignKeyDefinition("user_id", _userMapping,
                    "external_schema", "external_table", "external_id")
                .ToDDL()
                .ShouldBe(expected);
        }
    }
}