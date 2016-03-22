﻿using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Core.Context;
using Engine.Core.DataTypes;
using LanguageExt;
using NUnit.Framework;

namespace Engine.Core.Tests
{
    [TestFixture]
    public class EngineCoreTests
    {

        public static GetLoadedContextByIdentity CreateContext(Identity identity, params Tuple<string, string>[] tuples)
        {
            var data = tuples.ToDictionary(x => x.Item1, x => x.Item2);
            return fnIdentity => key => fnIdentity.Equals(identity) && data.ContainsKey(key) ? data[key] : Option<string>.None;
        }

        [Test]
        public void PathWithSimpleRule()
        {
            var identity = new Identity("device", "1");
            var identities = new HashSet<Identity> {identity};
            var context = CreateContext(identity);
            var rulesRepo = RulesRepositoryHelpers.With("path/to/key", FakeRule.Create(ctx => new ConfigurationValue("SomeValue")));
                
            var value = EngineCore.CalculateKey(identities, context, "path/to/key", rulesRepo).Map(x => x.Value);
            Assert.AreEqual("SomeValue", value);
        }

        [Test]
        public void PathWithNoMatchingRule()
        {
            var identity = new Identity("device", "1");
            var context = CreateContext(identity);
            var rulesRepo = RulesRepositoryHelpers.With("path/to/key", FakeRule.Create(ctx => new ConfigurationValue("SomeValue")));

            var missingValue = EngineCore.CalculateKey(new HashSet<Identity> {identity}, context, "path/to/key2",rulesRepo);

            Assert.IsTrue(missingValue.IsNone);
        }

        [Test]
        public void FixedValueInContext()
        {
            var identity = new Identity("device", "1");
            var context = CreateContext(identity, new Tuple<string, string>("@fixed_path/to/key", "SomeValue"));
            var rulesRepo = RulesRepositoryHelpers.Empty();

            var value = EngineCore.CalculateKey(new HashSet<Identity> { identity }, context, "path/to/key", rulesRepo).Map(x => x.Value);

            Assert.AreEqual("SomeValue", value);
        }

        [Test]
        public void ExternalPathRefernceInContext()
        {
            var identity = new Identity("device", "1");
            var context = CreateContext(identity);
            var rulesRepo = RulesRepositoryHelpers.With("path/to/key2", FakeRule.Create(ctx => new ConfigurationValue("SomeValue")))
                                                  .With("path/to/key", FakeRule.Create(ctx => ctx("@@key_path/to/key2").Map(x=>new ConfigurationValue(x))));

            var value = EngineCore.CalculateKey(new HashSet<Identity> { identity }, context, "path/to/key", rulesRepo).Map(x => x.Value);

            Assert.AreEqual("SomeValue", value);
        }

        [Test]
        public void RulesThatCheckContextValue()
        {
            var identity = new Identity("device", "1");
            var context = CreateContext(identity, new Tuple<string, string>("PartnerBrand", "ABC"));
            var rulesRepo = RulesRepositoryHelpers
                .With("path/to/key", FakeRule.Create(ctx => ctx("device.PartnerBrand") == "ABC" ? new ConfigurationValue("SomeValue") : Option<ConfigurationValue>.None));

            var value = EngineCore.CalculateKey(new HashSet<Identity> { identity }, context, "path/to/key", rulesRepo).Map(x => x.Value);
            Assert.AreEqual("SomeValue", value);

            rulesRepo = rulesRepo
                .With("path/to/other/key", FakeRule.Create(ctx => ctx("device.OtherProp") == "DEF" ? new ConfigurationValue("SomeValue") : Option<ConfigurationValue>.None));

            value = EngineCore.CalculateKey(new HashSet<Identity> { identity }, context, "path/to/other/key", rulesRepo).Map(x => x.Value);
            Assert.IsTrue(value.IsNone);

            rulesRepo = rulesRepo
                .With("path/to/other/key", FakeRule.Create(ctx => ctx("device.PartnerBrand") == "ABC" ? new ConfigurationValue("SomeValue") : Option<ConfigurationValue>.None));

            value = EngineCore.CalculateKey(new HashSet<Identity> { identity }, context, "path/to/other/key", rulesRepo).Map(x => x.Value);

            Assert.AreEqual("SomeValue", value);
        }

        
    }
}